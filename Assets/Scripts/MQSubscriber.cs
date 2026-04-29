using UnityEngine;
using System.Text;
using TMPro;

public class MQSubscriber : MonoBehaviour
{
    public TextMeshPro displayText;
    public float pollInterval = 0.1f; // Check for messages every 100ms
    private float nextPollTime = 0;

    void Update()
    {
        if (Time.time >= nextPollTime)
        {
            nextPollTime = Time.time + pollInterval;
            CheckForROSMessages();
        }
    }

    void CheckForROSMessages()
    {
        uint command;
        byte[] data;

        // Pull messages from hl2ss message queue
        while (hl2ss.PullMessage(out command, out data))
        {
            ProcessROSCommand(command, data);
            hl2ss.AcknowledgeMessage(command);
        }
    }

    void ProcessROSCommand(uint command, byte[] data)
    {
        // Define command IDs for different ROS topics
        // These would match what your hl2ss_ros2 wrapper sends
        switch (command)
        {
            case 1000: // Example: cmd_vel data
                ProcessCmdVel(data);
                break;
            case 1001: // Example: string message
                ProcessStringMessage(data);
                break;
            case 1002: // Example: sensor data
                ProcessSensorData(data);
                break;
            default:
                Debug.Log($"Unknown command: {command}");
                break;
        }

        // Send acknowledgment
        hl2ss.PushResult(1); // 1 = success
    }

    void ProcessStringMessage(byte[] data)
    {
        string message = Encoding.UTF8.GetString(data);
        Debug.Log($"Received ROS message: {message}");

        if (displayText != null)
        {
            displayText.text = message;
        }
    }

    void ProcessCmdVel(byte[] data)
    {
        // Parse velocity command (assuming it's encoded as floats)
        if (data.Length >= 24) // 6 floats (linear.xyz, angular.xyz)
        {
            float linearX = System.BitConverter.ToSingle(data, 0);
            float linearY = System.BitConverter.ToSingle(data, 4);
            float linearZ = System.BitConverter.ToSingle(data, 8);

            // Apply to GameObject or display
            Vector3 movement = new Vector3(linearX, linearY, linearZ);
            transform.position += movement * Time.deltaTime;
        }
    }

    void ProcessSensorData(byte[] data)
    {
        // Process other sensor data as needed
    }
}