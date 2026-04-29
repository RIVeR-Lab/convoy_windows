using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("HUD Elements")]
    public TextMeshProUGUI speedText;
    public Transform directionArrow;

    [Header("HUD Settings")]
    public float hudDistance = 2.0f;
    public float hudHeight = 0.0f;
    public bool followHead = true;

    [Header("Panel Controllers")]
    public StatusPanelController statusPanel; // Reference to status panel

    [Header("Heart Rate Panel")]
    public HeartRatePanelController heartRatePanel;

    [Header("TapStrap Panel")]
    public TapStrapDisplayController tapStrapPanel;

    [Header("Safety Features")]
    public SafetyFrameController safetyFrame;
    public bool enableSafetyFeatures = true;
    public float obstacleWarningDistance = 10f;

    private Transform cameraTransform;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        UpdateHUDPosition();

        // Initialize safety frame if present
        if (enableSafetyFeatures && safetyFrame != null)
        {
            safetyFrame.UpdateSpeed(0f); // Start at 0 speed
        }
    }

    void Update()
    {
        if (followHead)
        {
            UpdateHUDPosition();
        }
    }

    void UpdateHUDPosition()
    {
        Vector3 newPos = cameraTransform.position +
                        cameraTransform.forward * hudDistance;
        newPos.y = cameraTransform.position.y + hudHeight;

        transform.position = newPos;
        transform.rotation = Quaternion.LookRotation(
            transform.position - cameraTransform.position);
    }

    public void UpdateSpeed(float speed)
    {
        // Update speed text
        speedText.text = $"{speed:F1} m/s";

        // Update safety frame based on speed
        if (enableSafetyFeatures && safetyFrame != null)
        {
            safetyFrame.UpdateSpeed(speed);

            // Additional warnings for very high speed
            if (speed > 100f)
            {
                AddSpeedWarning();
            }
        }
    }

    public void UpdateDirection(float angle)
    {
        directionArrow.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Delegate connection status to StatusPanelController
    public void UpdateConnectionStatus(bool connected)
    {
        if (statusPanel != null)
        {
            statusPanel.UpdateConnectionStatus(connected);
        }
    }

    public void UpdateHeartRate(float heartRate)
    {
        if (heartRatePanel != null)
            heartRatePanel.UpdateHeartRate(heartRate);
    }

    public void UpdateHRV(float hrvInterval)
    {
        if (heartRatePanel != null)
            heartRatePanel.UpdateHRV(hrvInterval);
    }

    public void UpdateAlpha(float alpha)
    {
        if (heartRatePanel != null)
            heartRatePanel.UpdateAlpha(alpha);
    }

    public void UpdateTapGesture(int tapCode)
    {
        if (tapStrapPanel != null)
            tapStrapPanel.UpdateTapGesture(tapCode);
    }

    public void UpdateCommandedDistances(float longDist, float latDist)
    {
        if (tapStrapPanel != null)
            tapStrapPanel.UpdateDistances(longDist, latDist);
    }

    // Safety warning methods
    void AddSpeedWarning()
    {
        // Flash frame or show warning text
        if (safetyFrame != null)
        {
            safetyFrame.TriggerEmergency(true);
            StartCoroutine(ClearEmergency());
        }
    }

    IEnumerator ClearEmergency()
    {
        yield return new WaitForSeconds(2f);
        if (safetyFrame != null)
        {
            safetyFrame.TriggerEmergency(false);
        }
    }

    public void ShowObstacleWarning(float distance)
    {
        if (enableSafetyFeatures && safetyFrame != null)
        {
            if (distance < obstacleWarningDistance)
            {
                float intensity = 1f - (distance / obstacleWarningDistance);
                safetyFrame.SetFrameOpacity(intensity);
                safetyFrame.SetFrameColor(Color.red);
            }
            else
            {
                // Clear obstacle warning when distance is safe
                safetyFrame.SetFrameOpacity(0f);
            }
        }
    }

    // Additional utility methods
    public void SetVehicleMode(string mode)
    {
        if (statusPanel != null)
        {
            statusPanel.SetVehicleMode(mode);
        }
    }

    public void SetWarning(string message, bool isAlert = false)
    {
        if (statusPanel != null)
        {
            statusPanel.SetWarning(message, isAlert);
        }
    }

    // Method to test all systems
    public void TestAllSystems()
    {
        Debug.Log("Testing HUD Systems...");
        UpdateSpeed(50f);
        UpdateDirection(45f);
        UpdateConnectionStatus(true);
        SetVehicleMode("Manual");
        SetWarning("Systems Check OK", false);
        Debug.Log("HUD Systems Test Complete");
    }
}
