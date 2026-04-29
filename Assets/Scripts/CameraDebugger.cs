using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDebugger : MonoBehaviour
{
    void Start()
    {
        // Find all cameras
        GameObject[] mainCameras = GameObject.FindGameObjectsWithTag("MainCamera");

        Debug.Log($"Found {mainCameras.Length} cameras tagged as MainCamera:");
        foreach (var cam in mainCameras)
        {
            Debug.Log($"- {cam.name} (Active: {cam.activeInHierarchy})");
        }

        // Delete any auto-created cameras
        foreach (var cam in mainCameras)
        {
            if (cam.name == "Main Camera" && cam.transform.parent == null)
            {
                Debug.Log($"Deleting auto-created camera: {cam.name}");
                DestroyImmediate(cam);
            }
        }
    }
}
