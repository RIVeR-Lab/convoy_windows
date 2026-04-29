using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulsingWarning : MonoBehaviour
{
    private Renderer warningRenderer;
    private Color baseColor;

    void Start()
    {
        warningRenderer = GetComponentInChildren<Renderer>();
        baseColor = warningRenderer.material.color;
    }

    void Update()
    {
        float alpha = Mathf.PingPong(Time.time, 1f);
        Color newColor = baseColor;
        newColor.a = alpha * 0.7f;
        warningRenderer.material.color = newColor;
    }
}
