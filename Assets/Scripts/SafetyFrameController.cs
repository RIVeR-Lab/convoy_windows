using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SafetyFrameController : MonoBehaviour
{
    [Header("Frame Elements")]
    public Image borderTop;
    public Image borderBottom;
    public Image borderLeft;
    public Image borderRight;

    [Header("Speed Thresholds")]
    public float lowSpeedThreshold = 30f;   // km/h
    public float mediumSpeedThreshold = 60f; // km/h
    public float highSpeedThreshold = 90f;   // km/h

    [Header("Visual Settings")]
    public Color safeColor = Color.green;
    public Color cautionColor = Color.yellow;
    public Color warningColor = new Color(1f, 0.5f, 0f); // Orange
    public Color dangerColor = Color.red;

    [Header("Opacity Settings")]
    [Range(0, 1)] public float minOpacity = 0f;
    [Range(0, 1)] public float maxOpacity = 0.7f;
    public float fadeSpeed = 2f;

    [Header("Pulse Settings")]
    public bool enablePulse = true;
    public float pulseSpeed = 2f;
    public float pulseIntensity = 0.3f;

    private List<Image> allBorders;
    private float currentSpeed = 0f;
    private float targetOpacity = 0f;
    private float currentOpacity = 0f;
    private bool isEmergency = false;

    void Start()
    {
        // Collect all border images
        allBorders = new List<Image> { borderTop, borderBottom, borderLeft, borderRight };

        // Start with invisible frame
        SetFrameOpacity(0f);
    }

    void Update()
    {
        // Smooth opacity transition
        currentOpacity = Mathf.Lerp(currentOpacity, targetOpacity, Time.deltaTime * fadeSpeed);

        // Apply pulsing effect at high speeds
        float finalOpacity = currentOpacity;
        if (enablePulse && currentSpeed > highSpeedThreshold)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            finalOpacity = Mathf.Clamp01(currentOpacity + pulse);
        }

        SetFrameOpacity(finalOpacity);

        // Emergency flash
        if (isEmergency)
        {
            EmergencyFlash();
        }
    }

    public void UpdateSpeed(float speed)
    {
        currentSpeed = speed;

        // Determine color and opacity based on speed
        if (speed < lowSpeedThreshold)
        {
            // Low speed - no frame
            targetOpacity = minOpacity;
            SetFrameColor(safeColor);
        }
        else if (speed < mediumSpeedThreshold)
        {
            // Medium speed - subtle frame
            targetOpacity = Mathf.Lerp(0.1f, 0.3f,
                (speed - lowSpeedThreshold) / (mediumSpeedThreshold - lowSpeedThreshold));
            SetFrameColor(cautionColor);
        }
        else if (speed < highSpeedThreshold)
        {
            // High speed - visible frame
            targetOpacity = Mathf.Lerp(0.3f, 0.5f,
                (speed - mediumSpeedThreshold) / (highSpeedThreshold - mediumSpeedThreshold));
            SetFrameColor(warningColor);
        }
        else
        {
            // Very high speed - prominent frame
            targetOpacity = maxOpacity;
            SetFrameColor(dangerColor);
        }
    }

    public void TriggerEmergency(bool emergency)
    {
        isEmergency = emergency;
        if (emergency)
        {
            SetFrameColor(dangerColor);
        }
    }

    public void SetFrameOpacity(float opacity)
    {
        foreach (var border in allBorders)
        {
            if (border != null)
            {
                Color c = border.color;
                c.a = opacity;
                border.color = c;
            }
        }
    }

    public void SetFrameColor(Color color)
    {
        foreach (var border in allBorders)
        {
            if (border != null)
            {
                Color c = color;
                c.a = border.color.a; // Preserve current alpha
                border.color = c;
            }
        }
    }

    private void EmergencyFlash()
    {
        float flash = Mathf.PingPong(Time.time * 4f, 1f);
        SetFrameOpacity(flash * maxOpacity);
    }
}
