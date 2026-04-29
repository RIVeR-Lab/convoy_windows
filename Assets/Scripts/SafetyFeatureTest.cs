using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SafetyFeatureTest : MonoBehaviour
{
    [Header("HUD Reference")]
    public HUDController hudController;

    [Header("Speed Simulation")]
    public float testSpeed = 0f;
    public bool simulateDriving = true;
    public float simulationSpeedMultiplier = 20f;

    [Header("Manual Control Settings")]
    public float manualSpeedIncrement = 10f;

    [Header("Debug Display")]
    public TextMeshProUGUI debugDisplay;
    public bool showDebugInfo = true;

    [Header("Obstacle Simulation")]
    public bool simulateObstacles = false;
    public float obstacleDistance = 20f;

    void Start()
    {
        if (hudController == null)
        {
            Debug.LogError("HUD Controller not assigned to SafetyFeatureTest!");
        }

        Debug.Log("Safety Feature Test Started:");
        Debug.Log("- Press +/- to manually adjust speed");
        Debug.Log("- Press E for emergency warning");
        Debug.Log("- Press O to toggle obstacle simulation");
        Debug.Log("- Press Space to toggle auto-drive");
    }

    void Update()
    {
        // Toggle automatic driving simulation
        if (Input.GetKeyDown(KeyCode.Space))
        {
            simulateDriving = !simulateDriving;
            Debug.Log($"Auto-drive: {(simulateDriving ? "ON" : "OFF")}");
        }

        // Automatic speed simulation
        if (simulateDriving)
        {
            // Creates smooth speed oscillation between 0-120 km/h
            testSpeed = Mathf.PingPong(Time.time * simulationSpeedMultiplier, 120f);
            hudController.UpdateSpeed(testSpeed);
        }

        // Manual speed controls
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus)) // + key
        {
            simulateDriving = false; // Stop auto when manual control used
            testSpeed += manualSpeedIncrement * Time.deltaTime;
            testSpeed = Mathf.Min(testSpeed, 150f); // Cap at 150 km/h
            hudController.UpdateSpeed(testSpeed);
        }

        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus)) // - key
        {
            simulateDriving = false; // Stop auto when manual control used
            testSpeed -= manualSpeedIncrement * Time.deltaTime;
            testSpeed = Mathf.Max(0, testSpeed); // Don't go below 0
            hudController.UpdateSpeed(testSpeed);
        }

        // Quick speed presets (number keys)
        if (Input.GetKeyDown(KeyCode.Alpha0)) SetSpeed(0);
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetSpeed(20);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetSpeed(40);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetSpeed(60);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetSpeed(80);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SetSpeed(100);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SetSpeed(120);

        // Emergency warning test
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Emergency triggered!");
            if (hudController.safetyFrame != null)
            {
                hudController.safetyFrame.TriggerEmergency(true);
            }
        }

        // Obstacle simulation
        if (Input.GetKeyDown(KeyCode.O))
        {
            simulateObstacles = !simulateObstacles;
            Debug.Log($"Obstacle simulation: {(simulateObstacles ? "ON" : "OFF")}");
        }

        if (simulateObstacles)
        {
            // Simulate approaching obstacle
            obstacleDistance = Mathf.PingPong(Time.time * 5f, 15f);
            hudController.ShowObstacleWarning(obstacleDistance);
        }

        // Update debug display
        if (showDebugInfo && debugDisplay != null)
        {
            UpdateDebugDisplay();
        }

        // Reset everything
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetTest();
        }
    }

    void SetSpeed(float speed)
    {
        simulateDriving = false;
        testSpeed = speed;
        hudController.UpdateSpeed(testSpeed);
        Debug.Log($"Speed set to: {speed} km/h");
    }

    void UpdateDebugDisplay()
    {
        string debugText = $"<b>Safety Test Controls</b>\n";
        debugText += $"Speed: {testSpeed:F1} km/h\n";
        debugText += $"Auto-Drive: {(simulateDriving ? "ON" : "OFF")} (Space)\n";
        debugText += $"Obstacles: {(simulateObstacles ? $"ON - {obstacleDistance:F1}m" : "OFF")} (O)\n";
        debugText += $"\n<b>Controls:</b>\n";
        debugText += "+/- : Adjust Speed\n";
        debugText += "0-6 : Speed Presets\n";
        debugText += "E : Emergency\n";
        debugText += "R : Reset";

        debugDisplay.text = debugText;
    }

    void ResetTest()
    {
        testSpeed = 0;
        simulateDriving = false;
        simulateObstacles = false;
        hudController.UpdateSpeed(0);
        if (hudController.safetyFrame != null)
        {
            hudController.safetyFrame.TriggerEmergency(false);
            hudController.safetyFrame.UpdateSpeed(0);
        }
        Debug.Log("Test reset to initial state");
    }
}