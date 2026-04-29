using System;
using System.Collections;
using System.Text;
using UnityEngine;
using TMPro;
#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Connectivity;
#endif

public class HL2SSIntegration : MonoBehaviour
{
    [Header("HL2SS Stream Settings")]
    [Tooltip("Research Mode - Depth & IMU sensors")]
    public bool enableRM = false;
    [Tooltip("Photo Video Camera")]
    public bool enablePV = true;
    [Tooltip("Microphone")]
    public bool enableMC = false;
    [Tooltip("Spatial Input - Hand tracking")]
    public bool enableSI = true;
    [Tooltip("Remote Configuration")]
    public bool enableRC = true;
    [Tooltip("Scene Mesh")]
    public bool enableSM = false;
    [Tooltip("Scene Understanding")]
    public bool enableSU = false;
    [Tooltip("Voice Input")]
    public bool enableVI = false;
    [Tooltip("Message Queue")]
    public bool enableMQ = true;
    [Tooltip("Eye Tracking")]
    public bool enableEET = false;

    [Header("HUD References")]
    public HUDController hudController;
    public StatusPanelController statusPanel;

    [Header("Connection Info")]
    public TextMeshProUGUI ipAddressText;
    private string deviceIP = "Not Connected";

    [Header("Message Processing")]
    public float messageCheckInterval = 0.1f; // Check every 100ms
    private float lastMessageCheck = 0f;

    [Header("Vehicle Data")]
    private float currentSpeed = 0f;
    private float currentDirection = 0f;
    private bool isConnected = false;

    void Start()
    {
        InitializeHL2SS();
        StartCoroutine(MessageProcessingLoop());
    }

    void InitializeHL2SS()
    {
        Debug.Log("Initializing HL2SS...");

        // Initialize HL2SS with selected streams
        hl2ss.Initialize(
            enableRM,   // Research Mode
            enablePV,   // Photo Video
            enableMC,   // Microphone
            enableSI,   // Spatial Input
            enableRC,   // Remote Configuration
            enableSM,   // Scene Mesh
            enableSU,   // Scene Understanding
            enableVI,   // Voice Input
            enableMQ,   // Message Queue (important for ROS2)
            enableEET   // Eye Tracking
        );

        // Get and display IP address
        deviceIP = hl2ss.GetIPAddress();
        Debug.Log($"HL2SS Initialized - Device IP: {deviceIP}");

        if (ipAddressText != null)
        {
            ipAddressText.text = $"IP: {deviceIP}";
        }

        // Update status panel
        if (statusPanel != null)
        {
            statusPanel.SetWarning($"HL2SS Ready - IP: {deviceIP}", false);
        }

        // Print to HL2SS debug
        hl2ss.Print($"HL2SS Unity Integration Started at {DateTime.Now}");

        // Update coordinate system for spatial tracking
        UpdateCoordinateSystem();
    }

    void UpdateCoordinateSystem()
    {
#if WINDOWS_UWP
        bool coordSystemUpdated = hl2ss.UpdateCoordinateSystem();
        if (coordSystemUpdated)
        {
            Debug.Log("Coordinate system updated successfully");
            hl2ss.Print("Coordinate system synced");
        }
        else
        {
            Debug.LogWarning("Failed to update coordinate system");
        }
#endif
    }

    IEnumerator MessageProcessingLoop()
    {
        while (true)
        {
            ProcessIncomingMessages();
            yield return new WaitForSeconds(messageCheckInterval);
        }
    }

    void ProcessIncomingMessages()
    {
        uint command;
        byte[] data;

        // Pull all available messages
        while (hl2ss.PullMessage(out command, out data))
        {
            ProcessMessage(command, data);

            // Acknowledge message processing
            hl2ss.AcknowledgeMessage(command);
        }
    }

    void ProcessMessage(uint command, byte[] data)
    {
        // Command protocol (define based on your ROS2 bridge):
        // 0x01: Speed update
        // 0x02: Direction update
        // 0x03: Connection status
        // 0x04: Vehicle mode
        // 0x05: Obstacle detection
        // 0xFF: Reset/Error

        switch (command)
        {
            case 0x01: // Speed update
                ProcessSpeedUpdate(data);
                break;

            case 0x02: // Direction update
                ProcessDirectionUpdate(data);
                break;

            case 0x03: // Connection status
                ProcessConnectionStatus(data);
                break;

            case 0x04: // Vehicle mode
                ProcessVehicleMode(data);
                break;

            case 0x05: // Obstacle detection
                ProcessObstacleData(data);
                break;

            case 0xFF: // Reset or error
                HandleReset();
                break;

            default:
                Debug.LogWarning($"Unknown command received: 0x{command:X2}");
                break;
        }

        // Send acknowledgment back
        hl2ss.PushResult(command);
    }

    void ProcessSpeedUpdate(byte[] data)
    {
        if (data != null && data.Length >= 4)
        {
            currentSpeed = BitConverter.ToSingle(data, 0);

            if (hudController != null)
            {
                hudController.UpdateSpeed(currentSpeed);
            }

            Debug.Log($"Speed updated: {currentSpeed:F1} km/h");
            hl2ss.Print($"Speed: {currentSpeed:F1}");
        }
    }

    void ProcessDirectionUpdate(byte[] data)
    {
        if (data != null && data.Length >= 4)
        {
            currentDirection = BitConverter.ToSingle(data, 0);

            if (hudController != null)
            {
                hudController.UpdateDirection(currentDirection);
            }

            Debug.Log($"Direction updated: {currentDirection:F1}°");
        }
    }

    void ProcessConnectionStatus(byte[] data)
    {
        if (data != null && data.Length >= 1)
        {
            isConnected = data[0] != 0;

            if (hudController != null)
            {
                hudController.UpdateConnectionStatus(isConnected);
            }

            if (statusPanel != null)
            {
                statusPanel.UpdateConnectionStatus(isConnected);
                statusPanel.SetWarning(isConnected ? "ROS2 Connected" : "ROS2 Disconnected", !isConnected);
            }

            Debug.Log($"Connection status: {(isConnected ? "Connected" : "Disconnected")}");
        }
    }

    void ProcessVehicleMode(byte[] data)
    {
        if (data != null && data.Length > 0)
        {
            string mode = Encoding.UTF8.GetString(data);

            if (statusPanel != null)
            {
                statusPanel.SetVehicleMode(mode);
            }

            Debug.Log($"Vehicle mode: {mode}");
        }
    }

    void ProcessObstacleData(byte[] data)
    {
        if (data != null && data.Length >= 4)
        {
            float distance = BitConverter.ToSingle(data, 0);

            if (hudController != null)
            {
                hudController.ShowObstacleWarning(distance);
            }

            Debug.Log($"Obstacle detected at: {distance:F1}m");
        }
    }

    void HandleReset()
    {
        Debug.Log("Reset command received");

        // Reset all values
        currentSpeed = 0f;
        currentDirection = 0f;
        isConnected = false;

        // Update UI
        if (hudController != null)
        {
            hudController.UpdateSpeed(0);
            hudController.UpdateDirection(0);
            hudController.UpdateConnectionStatus(false);
        }

        if (statusPanel != null)
        {
            statusPanel.UpdateConnectionStatus(false);
            statusPanel.SetVehicleMode("Idle");
            statusPanel.SetWarning("System Reset", false);
        }

        // Restart message queue
        hl2ss.AcknowledgeMessage(~0U);
    }

    // Send data back to ROS2 (through hl2ss bridge)
    public void SendToROS(uint command, float value)
    {
        byte[] data = BitConverter.GetBytes(value);
        // This would need the hl2ss bridge to handle outgoing messages
        hl2ss.PushResult(command);
        hl2ss.Print($"Sent command {command}: {value}");
    }

    void OnDestroy()
    {
        // Clean shutdown
        hl2ss.Print("HL2SS Unity Integration Shutting Down");
        HandleReset();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            hl2ss.Print("Application paused");
        }
        else
        {
            hl2ss.Print("Application resumed");
            UpdateCoordinateSystem();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            UpdateCoordinateSystem();
        }
    }
}
