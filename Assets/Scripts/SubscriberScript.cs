using UnityEngine;
using NativeWebSocket;
using System.Text;
using System;

[Serializable]
public class ROSBridgeMessage
{
    public string op;
    public string topic;
    public string type;
    public MessageData msg;
}

/**
[Serializable]
public class MessageDataResult
{
    public string data;
    // Add other fields as needed
}**/

public class HL2SSROSSubscriber : MonoBehaviour
{
    WebSocket websocket;

    // Configure these in Inspector
    public string rosbridgeIP = "192.168.1.100";  // Your ROS2 PC IP
    public int rosbridgePort = 9090;
    public string[] topicsToSubscribe = { "/cmd_vel", "/example_topic" };

    // For displaying data
    public TMPro.TextMeshPro displayText;

    async void Start()
    {
        // Don't interfere with hl2ss streams
        string url = $"ws://{rosbridgeIP}:{rosbridgePort}";
        websocket = new WebSocket(url);

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connected to ROS2!");
            foreach (string topic in topicsToSubscribe)
            {
                SubscribeToTopic(topic);
            }
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = Encoding.UTF8.GetString(bytes);
            ProcessROSMessage(message);
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("WebSocket Error: " + e);
        };

        await websocket.Connect();
    }

    void SubscribeToTopic(string topic)
    {
        string subscribeMsg = JsonUtility.ToJson(new
        {
            op = "subscribe",
            topic = topic,
            type = "std_msgs/String"  // Change based on your message type
        });

        websocket.SendText(subscribeMsg);
        Debug.Log($"Subscribed to {topic}");
    }

    void ProcessROSMessage(string jsonMessage)
    {
        try
        {
            // Parse the message
            var parsed = JsonUtility.FromJson<ROSBridgeMessage>(jsonMessage);

            // Update UI on main thread
            if (displayText != null)
            {
                displayText.text = $"{parsed.topic}: {parsed.msg.data}";
            }

            Debug.Log($"Received from {parsed.topic}: {parsed.msg.data}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing message: {e}");
        }
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }
}