using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TapStrapDisplayController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI gestureText;
    public TextMeshProUGUI longDistText;
    public TextMeshProUGUI latDistText;

    [Header("Display Settings")]
    public float gestureFeedbackDuration = 2f;

    private float gestureTimer = 0f;
    private string lastGestureName = "";

    // Tap code mappings matching your ROS node
    private enum TapGesture
    {
        None = 0,
        IncreaseLongDist = 2,    // Index finger
        ResetLongDist = 3,       // Index + Middle (double tap to confirm)
        DecreaseLongDist = 4,    // Middle finger
        ShiftLeft = 8,           // Ring finger
        ResetLatDist = 9,        // Ring + Index (double tap to confirm)
        ShiftRight = 16          // Pinky finger
    }

    void Update()
    {
        // Fade out gesture feedback after duration
        if (gestureTimer > 0)
        {
            gestureTimer -= Time.deltaTime;
            if (gestureTimer <= 0)
            {
                gestureText.text = "Gesture: —";
            }
        }
    }

    public void UpdateTapGesture(int tapCode)
    {
        string gestureName = InterpretTapCode(tapCode);

        if (!string.IsNullOrEmpty(gestureName))
        {
            lastGestureName = gestureName;
            gestureText.text = $"Gesture: {gestureName}";
            gestureTimer = gestureFeedbackDuration;
        }
    }

    public void UpdateDistances(float longDist, float latDist)
    {
        longDistText.text = $"Long: {longDist:F1} m";
        latDistText.text = $"Lat: {latDist:F1} m";
    }

    private string InterpretTapCode(int code)
    {
        return code switch
        {
            2 => "↑ Increase Follow Dist",
            3 => "⟳ Reset Long (confirm)",
            4 => "↓ Decrease Follow Dist",
            8 => "← Shift Left",
            9 => "⟳ Reset Lat (confirm)",
            16 => "→ Shift Right",
            _ => null
        };
    }
}