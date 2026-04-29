using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class PreDeploymentCheck : MonoBehaviour
{
    [Header("Optional UI Display")]
    public TextMeshProUGUI debugText; // Optional - can display results on HUD

    void Start()
    {
        RunAllChecks();
    }

    void RunAllChecks()
    {
        Debug.Log("========================================");
        Debug.Log("       PRE-DEPLOYMENT CHECK STARTED     ");
        Debug.Log("========================================");

        bool allPassed = true;
        string report = "PRE-DEPLOYMENT CHECK:\n\n";

        // Check 1: HUD Components
        bool hudCanvas = GameObject.Find("HUD_Canvas") != null;
        LogCheck("HUD Canvas", hudCanvas);
        report += $"HUD Canvas: {(hudCanvas ? "✓" : "✗")}\n";
        allPassed &= hudCanvas;

        var hudController = FindObjectOfType<HUDController>();
        bool hudControllerExists = hudController != null;
        LogCheck("HUD Controller", hudControllerExists);
        report += $"HUD Controller: {(hudControllerExists ? "✓" : "✗")}\n";
        allPassed &= hudControllerExists;

        // Check 2: Safety Frame
        var safetyFrame = FindObjectOfType<SafetyFrameController>();
        bool safetyExists = safetyFrame != null;
        LogCheck("Safety Frame", safetyExists);
        report += $"Safety Frame: {(safetyExists ? "✓" : "✗")}\n";

        // Check 3: Status Panel
        var statusPanel = FindObjectOfType<StatusPanelController>();
        bool statusExists = statusPanel != null;
        LogCheck("Status Panel", statusExists);
        report += $"Status Panel: {(statusExists ? "✓" : "✗")}\n";

        // Check 4: HL2SS Components
        var hl2ssStreaming = FindObjectOfType<Hololens2SensorStreaming>();
        bool hl2ssExists = hl2ssStreaming != null;
        LogCheck("HL2SS Streaming", hl2ssExists);
        report += $"HL2SS Streaming: {(hl2ssExists ? "✓" : "✗")}\n";
        allPassed &= hl2ssExists;

        // Check 5: Main Camera
        bool cameraReady = Camera.main != null;
        LogCheck("Main Camera", cameraReady);
        report += $"Main Camera: {(cameraReady ? "✓" : "✗")}\n";
        allPassed &= cameraReady;

        if (cameraReady)
        {
            bool cameraTagged = Camera.main.tag == "MainCamera";
            LogCheck("Camera Tagged Correctly", cameraTagged);
            report += $"Camera Tag: {(cameraTagged ? "✓" : "✗")}\n";
            allPassed &= cameraTagged;

            bool hl2ssOnCamera = Camera.main.GetComponent<Hololens2SensorStreaming>() != null;
            LogCheck("HL2SS on Camera", hl2ssOnCamera);
            report += $"HL2SS on Camera: {(hl2ssOnCamera ? "✓" : "✗")}\n";
        }

        // Check 6: MixedRealityPlayspace (should be only 1)
        var playspaces = FindObjectsOfType<GameObject>()
            .Where(go => go.name == "MixedRealityPlayspace").ToArray();
        bool singlePlayspace = playspaces.Length == 1;
        LogCheck($"MixedRealityPlayspace Count ({playspaces.Length})", singlePlayspace);
        report += $"Playspace Count: {playspaces.Length} {(singlePlayspace ? "✓" : "✗ Should be 1!")}\n";
        allPassed &= singlePlayspace;

        // Check 7: Build Platform
#if WINDOWS_UWP
        LogCheck("UWP Build", true);
        report += "Platform: UWP ✓\n";
#else
        LogCheck("UWP Build", false);
        report += "Platform: Editor (Will be UWP on build) ⚠\n";
#endif

        // Check 8: HL2SS Integration
        var hl2ssIntegration = FindObjectOfType<HL2SSIntegration>();
        bool integrationExists = hl2ssIntegration != null;
        LogCheck("HL2SS Integration Script", integrationExists);
        report += $"HL2SS Integration: {(integrationExists ? "✓" : "✗")}\n";

        // Final Report
        Debug.Log("========================================");
        if (allPassed)
        {
            Debug.Log("<color=green>    ALL CRITICAL CHECKS PASSED! ✓</color>");
            report += "\n<color=green>READY FOR DEPLOYMENT!</color>";
        }
        else
        {
            Debug.Log("<color=red>    SOME CHECKS FAILED! ✗</color>");
            report += "\n<color=red>FIX ISSUES BEFORE DEPLOYMENT!</color>";
        }
        Debug.Log("========================================");

        // Display on HUD if text component assigned
        if (debugText != null)
        {
            debugText.text = report;
        }

        // Auto-disable after 10 seconds to not clutter
        Invoke("DisableDebugDisplay", 10f);
    }

    void LogCheck(string checkName, bool passed)
    {
        if (passed)
        {
            Debug.Log($"<color=green>✓</color> {checkName}: PASSED");
        }
        else
        {
            Debug.LogError($"✗ {checkName}: FAILED");
        }
    }

    void DisableDebugDisplay()
    {
        if (debugText != null)
        {
            debugText.gameObject.SetActive(false);
        }

        // Optionally disable self
        // gameObject.SetActive(false);
    }

    // Add manual trigger for testing
    void Update()
    {
        // Press P to re-run checks
        if (Input.GetKeyDown(KeyCode.P))
        {
            RunAllChecks();
        }
    }
}
