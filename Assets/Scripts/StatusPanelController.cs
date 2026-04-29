using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusPanelController : MonoBehaviour
{
    [Header("Status Text Elements")]
    public TextMeshProUGUI connectionText;
    public TextMeshProUGUI modeText;
    public TextMeshProUGUI warningText;

    [Header("Visual Settings")]
    public Color connectedColor = Color.green;
    public Color disconnectedColor = Color.red;
    public Color normalColor = Color.yellow;
    public Color alertColor = Color.red;

    private bool isConnected = false;

    void Start()
    {
        // Initialize status
        UpdateConnectionStatus(false);
        SetVehicleMode("Manual");
        SetWarning("System Ready", false);

        // Start demo animation (remove in production)
        //StartCoroutine(DemoStatusChanges());
    }

    public void UpdateConnectionStatus(bool connected)
    {
        isConnected = connected;
        connectionText.text = connected ? "ROS2: Connected" : "ROS2: Disconnected";
        connectionText.color = connected ? connectedColor : disconnectedColor;
    }

    public void SetVehicleMode(string mode)
    {
        modeText.text = $"Mode: {mode}";

        // Change color based on mode
        switch (mode.ToLower())
        {
            case "auto":
                modeText.color = Color.cyan;
                break;
            case "manual":
                modeText.color = Color.white;
                break;
            case "convoy":
                modeText.color = Color.green;
                break;
            default:
                modeText.color = Color.gray;
                break;
        }
    }

    public void SetWarning(string message, bool isAlert = false)
    {
        warningText.text = message;
        warningText.color = isAlert ? alertColor : normalColor;

        // Make text blink if alert
        if (isAlert)
        {
            StartCoroutine(BlinkWarning());
        }
    }

    IEnumerator BlinkWarning()
    {
        for (int i = 0; i < 3; i++)
        {
            warningText.enabled = false;
            yield return new WaitForSeconds(0.3f);
            warningText.enabled = true;
            yield return new WaitForSeconds(0.3f);
        }
    }

    // Demo animation for testing

    IEnumerator DemoStatusChanges()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            UpdateConnectionStatus(!isConnected);

            yield return new WaitForSeconds(2f);
            SetVehicleMode("Auto");

            yield return new WaitForSeconds(2f);
            SetWarning("Obstacle Detected!", true);

            yield return new WaitForSeconds(3f);
            SetVehicleMode("Convoy");
            SetWarning("Following Lead Vehicle", false);
        }
    }
}
