using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HeartRatePanelController : MonoBehaviour
{
    [Header("Heart Rate Display")]
    public TextMeshProUGUI heartRateText;
    public TextMeshProUGUI hrvText;           // Heart Rate Variability (from RR interval)
    public TextMeshProUGUI alphaText;

    [Header("Visual Elements")]
    public Image heartIcon;
    public Image alphaBar;                     // Progress bar for alpha value
    public Image alphaBarBackground;

    [Header("Pulse Animation")]
    public bool enablePulseAnimation = true;
    public float pulseScale = 1.2f;
    public float pulseDuration = 0.1f;

    [Header("Color Thresholds")]
    public float lowHeartRate = 60f;
    public float highHeartRate = 100f;
    public Color normalColor = Color.green;
    public Color elevatedColor = Color.yellow;
    public Color highColor = Color.red;

    [Header("Alpha Display")]
    public Color userControlColor = new Color(0.2f, 0.6f, 1f);    // Blue for user
    public Color autoControlColor = new Color(1f, 0.5f, 0.2f);    // Orange for auto

    private float lastHeartRate = 0f;
    private float lastBeatTime = 0f;
    private Coroutine pulseCoroutine;

    void Start()
    {
        // Initialize displays
        UpdateHeartRate(0f);
        UpdateHRV(0f);
        UpdateAlpha(0.5f);
    }

    public void UpdateHeartRate(float heartRate)
    {
        if (heartRateText != null)
        {
            heartRateText.text = $"{heartRate:F0}";
        }

        // Update heart icon color based on heart rate
        if (heartIcon != null)
        {
            if (heartRate < lowHeartRate)
                heartIcon.color = normalColor;
            else if (heartRate < highHeartRate)
                heartIcon.color = elevatedColor;
            else
                heartIcon.color = highColor;
        }

        // Trigger pulse animation based on heart rate
        if (enablePulseAnimation && heartRate > 0 && heartIcon != null)
        {
            float beatInterval = 60f / heartRate;
            if (Time.time - lastBeatTime >= beatInterval)
            {
                lastBeatTime = Time.time;
                TriggerPulse();
            }
        }

        lastHeartRate = heartRate;
    }

    public void UpdateHRV(float hrvInterval)
    {
        if (hrvText != null)
        {
            // Convert RR interval to HRV display (in ms)
            float hrvMs = hrvInterval; //* 1000f;
            hrvText.text = $"{hrvMs:F0} ms";
        }
    }

    public void UpdateAlpha(float alpha)
    {
        // Alpha: 0 = full auto, 1 = full user control
        if (alphaText != null)
        {
            float userPercent = alpha * 100f;
            alphaText.text = $"User: {userPercent:F0}%";
        }

        if (alphaBar != null)
        {
            alphaBar.fillAmount = alpha;

            // Gradient color from auto (orange) to user (blue)
            alphaBar.color = Color.Lerp(autoControlColor, userControlColor, alpha);
        }
    }

    void TriggerPulse()
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseAnimation());
    }

    IEnumerator PulseAnimation()
    {
        Vector3 originalScale = heartIcon.transform.localScale;
        Vector3 targetScale = originalScale * pulseScale;

        // Scale up
        float elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pulseDuration;
            heartIcon.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // Scale down
        elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pulseDuration;
            heartIcon.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        heartIcon.transform.localScale = originalScale;
    }

    // Get cognitive load assessment based on heart rate
    public string GetCognitiveLoadLevel()
    {
        if (lastHeartRate < lowHeartRate)
            return "Low";
        else if (lastHeartRate < highHeartRate)
            return "Moderate";
        else
            return "High";
    }
}
