using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class EditorCanvasScaler : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private float editorScale = 1.0f;  // Large scale for editing
    [SerializeField] private float buildScale = 0.001f; // Tiny scale for HoloLens

    [Header("Current State (Read Only)")]
    [SerializeField] private string currentMode = "Editor";

    void Awake()
    {
        UpdateScale();
    }

    void Start()
    {
        UpdateScale();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Updates immediately when you change values in inspector
        if (!Application.isPlaying)
        {
            UpdateScale();
        }
    }

    // This ensures the scale updates as you work
    void Update()
    {
        if (!Application.isPlaying && transform.localScale.x != editorScale)
        {
            UpdateScale();
        }
    }
#endif

    void UpdateScale()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // EDIT MODE: Use large scale so you can see past TMP bounding boxes
            transform.localScale = Vector3.one * editorScale;
            currentMode = "Editor Mode - Large Scale";
        }
        else
        {
            // PLAY MODE: Use small scale to test as it will appear on HoloLens
            transform.localScale = Vector3.one * buildScale;
            currentMode = "Play Mode - Build Scale";
        }
#else
        // BUILD: Always use small scale on device
        transform.localScale = Vector3.one * buildScale;
#endif
    }
}