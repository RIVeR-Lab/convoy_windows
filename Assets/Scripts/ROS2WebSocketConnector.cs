using NativeWebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using UnityEngine;

using WebSocket = NativeWebSocket.WebSocket;
using Debug = UnityEngine.Debug;

[Serializable]
public class ROSMessage
{
    public string op;
    public string topic;
    public MessageData msg;
}

[Serializable]
public class ROSSubscription
{
    public string op;
    public string topic;
    public string type;
}

[Serializable]
public class MessageData
{
    public string data;
}

[Serializable]
public class HUDData
{
    public float speed;
    public float direction;
    public bool connection_status;
    public string vehicle_mode;
    public string warning_message;
    public bool is_alert;
    public float obstacle_distance;
    public long timestamp;
    public float heart_rate;
    public float rr_interval;
    public float hrv_interval;
    public float alpha;
    public int tap;
    public float cmd_long;
    public float cmd_lat;
}

[Serializable]
//public class VehiclePose
//{
  //  public float x;
   // public float y;
   // public float yaw;  // Vehicle heading in radians
//}

public class ROS2WebSocketConnector : MonoBehaviour
{
    [Header("HUD Reference")]
    public HUDController hudController; // Drag HUD GameObject here in Inspector

    [Header("Connection Settings")]
    public string rosIP = "192.168.0.223";
    public int rosPort = 9090;

    [Header("Topics")]
    public string hudDataTopic = "/hololens/hud/json";
    public string hudPathTopic = "/hololens/hud/path";

    [Header("AR Path Display")]
    public ARPathDisplay arPathDisplay;

    private WebSocket websocket;
    //private HUDController hudController;
    private bool isConnected = false;

    // Latest data
    private HUDData latestHUDData;
    private bool hasNewData = false;

    // Add debug counters
    private int messagesReceived = 0;
    private float lastMessageTime = 0;

    async void Start()
    {
        // Try Inspector assignment first
        if (hudController == null)
        {
            // Fall back to GetComponent
            hudController = GetComponent<HUDController>();
        }

        if (hudController == null)
        {
            // Final fallback - search scene
            hudController = FindObjectOfType<HUDController>();
        }

        if (hudController == null)
        {
            Debug.LogError("HUDController not found!");
            return;
        }
        await ConnectToROS2();
    }

    async System.Threading.Tasks.Task ConnectToROS2()
    {
        try
        {
            websocket = new WebSocket($"ws://{rosIP}:{rosPort}");

            websocket.OnOpen += OnWebSocketOpen;
            websocket.OnError += OnWebSocketError;
            websocket.OnClose += OnWebSocketClose;
            websocket.OnMessage += OnWebSocketMessage;

            await websocket.Connect();
            Debug.Log($"Connecting to ROS2 bridge at {rosIP}:{rosPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"WebSocket connection failed: {e.Message}");
        }
    }

    void OnWebSocketOpen()
    {
        Debug.Log($"WebSocket connected to ROS2 @ {rosIP}:{rosPort}!");
        isConnected = true;

        //TestPublish();  // Add this

        // Subscribe to HUD data topic
        SubscribeToTopic(hudDataTopic, "std_msgs/msg/String");
        SubscribeToTopic(hudPathTopic, "std_msgs/msg/String");

    }

    void OnWebSocketError(string error)
    {
        Debug.LogError($"WebSocket error: {error}");
    }

    void OnWebSocketClose(WebSocketCloseCode code)
    {
        Debug.Log($"WebSocket closed: {code}");
        isConnected = false;
    }

    void OnWebSocketMessage(byte[] bytes)
    {
        var message = Encoding.UTF8.GetString(bytes);
        ProcessROSMessage(message);
    }

    void SubscribeToTopic(string topic, string type)
    {
        var subscription = new ROSSubscription
        {
            op = "subscribe",
            topic = topic,
            type = type
        };

        var json = JsonUtility.ToJson(subscription);
        websocket.SendText(json);
        Debug.Log($"Subscribed to topic: {topic}");
        Debug.Log($"Subscribed JSON: {json}");
    }

    void ProcessROSMessage(string jsonMessage)
    {
        var rosMsg = JsonUtility.FromJson<ROSMessage>(jsonMessage);
        try
        {
            messagesReceived++;
            lastMessageTime = Time.time;

            Debug.Log($"📥 Message #{messagesReceived} at {Time.time:F2}s");
            Debug.Log($"Raw JSON: {jsonMessage}");

            rosMsg = JsonUtility.FromJson<ROSMessage>(jsonMessage);
            Debug.Log($"Parsed topic: {rosMsg.topic}, Has msg: {rosMsg.msg != null}");

            if (rosMsg.topic == hudDataTopic && rosMsg.msg != null)
            {
                // Parse HUD data from the message
                var hudData = JsonUtility.FromJson<HUDData>(rosMsg.msg.data);
                Debug.Log($"✅ HUD Data parsed - Speed: {hudData.speed}, Direction: {hudData.direction}");

                latestHUDData = hudData;
                hasNewData = true;
            }
            else if (rosMsg.topic == hudPathTopic && rosMsg.msg != null)
            {
                if (arPathDisplay != null)
                {
                    arPathDisplay.OnPathDataReceived(rosMsg.msg.data);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing ROS message: {e.Message}");
            Debug.LogWarning($"Topic mismatch or null msg. Expected: {hudDataTopic}, Got: {rosMsg.topic}");
        }
    }

    /**
    public void TestPublish()
    {
        if (!isConnected)
        {
            Debug.LogError("Not connected!");
            return;
        }

        // Subscribe to a known ROS topic
        var testSub = new ROSSubscription
        {
            op = "subscribe",
            topic = "/rosout",  // This topic always exists
            type = "rcl_interfaces/msg/Log"
        };

        var json = JsonUtility.ToJson(testSub);
        websocket.SendText(json);
        Debug.Log("Subscribed to /rosout for testing");
    }**/

    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
        #endif

        // Update HUD with new data
        if (hasNewData && hudController != null && latestHUDData != null)
        {
            UpdateHUD();
            hasNewData = false;
        }
    }

    void UpdateHUD()
    {
        // Update your HUD elements
        hudController.UpdateSpeed(latestHUDData.speed);
        hudController.UpdateDirection(latestHUDData.direction);
        hudController.UpdateConnectionStatus(latestHUDData.connection_status);
        hudController.SetVehicleMode(latestHUDData.vehicle_mode);


        // Handle warnings if present
        if (!string.IsNullOrEmpty(latestHUDData.warning_message))
        {
            hudController.SetWarning(latestHUDData.warning_message, latestHUDData.is_alert);
        }

        // Handle obstacle warnings for safety features
        hudController.ShowObstacleWarning(latestHUDData.obstacle_distance);

        hudController.UpdateHeartRate(latestHUDData.heart_rate);
        hudController.UpdateHRV(latestHUDData.hrv_interval);
        hudController.UpdateAlpha(latestHUDData.alpha);

        // TapStrap gesture and distance feedback
        if (latestHUDData.tap != 0)
        {
            hudController.UpdateTapGesture(latestHUDData.tap);
        }
        hudController.UpdateCommandedDistances(
            latestHUDData.cmd_long,
            latestHUDData.cmd_lat
        );
    }

    public void PublishToROS(string topic, object data)
    {
        if (!isConnected) return;

        var message = new
        {
            op = "publish",
            topic = topic,
            msg = data
        };

        var json = JsonUtility.ToJson(message);
        websocket.SendText(json);
    }

    async void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Debug.Log("Application pausing...");

            // Add null check before using websocket
            if (websocket != null && websocket.State == NativeWebSocket.WebSocketState.Open)
            {
                websocket.Close();
            }

            // Add null check for hudController
            if (hudController != null)
            {
               // hudController.ShowPausedStatus();
            }
        }
        else
        {
            Debug.Log("Application resuming...");
            // Handle resume with null checks
            if (websocket != null)
            {
                // Reconnect logic
            }
        }
    }

    async void OnDestroy()
    {
        if (websocket != null && websocket.State == NativeWebSocket.WebSocketState.Open)
        {
            await websocket.Close();
        }
    }
}
