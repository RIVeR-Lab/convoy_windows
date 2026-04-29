using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestArrowRotation : MonoBehaviour
{
    [Header("Arrow Reference")]
    public DirectionIndicator arrow;
    public GameObject arrowGameObject;

    [Header("Test Settings")]
    public bool enableAutoRotation = false;
    public bool enableKeyboardControl = true;
    public float autoRotationSpeed = 30f;

    [Header("Debug Display")]
    public TextMeshProUGUI debugText;
    private float currentAngle = 0f;

    void Start()
    {
        // Try to find arrow if not assigned
        if (arrow == null && arrowGameObject != null)
        {
            arrow = arrowGameObject.GetComponent<DirectionIndicator>();
        }

        if (arrow == null)
        {
            Debug.LogError("Arrow not assigned! Please assign DirectionArrow in Inspector.");
        }

        Debug.Log("Test Started - Use Arrow Keys to rotate");
    }

    void Update()
    {
        if (arrow == null) return;

        // Auto rotation test
        if (enableAutoRotation)
        {
            currentAngle = Time.time * autoRotationSpeed;
            arrow.SetDirection(currentAngle);
        }

        // Keyboard control test
        if (enableKeyboardControl)
        {
            if (Input.GetKey(KeyCode.A)) // Left
            {
                currentAngle -= 90 * Time.deltaTime;
                arrow.SetDirection(currentAngle);
            }
            if (Input.GetKey(KeyCode.D)) // Right
            {
                currentAngle += 90 * Time.deltaTime;
                arrow.SetDirection(currentAngle);
            }
            if (Input.GetKey(KeyCode.W)) // Up
            {
                currentAngle = -90;
                arrow.SetDirection(currentAngle);
            }
            if (Input.GetKey(KeyCode.S)) // Down
            {
                currentAngle = 90;
                arrow.SetDirection(currentAngle);
            }

            // Number keys for specific directions
            if (Input.GetKeyDown(KeyCode.Alpha1)) arrow.SetDirection(0);    // Right
            if (Input.GetKeyDown(KeyCode.Alpha2)) arrow.SetDirection(-45);  // Up-Right
            if (Input.GetKeyDown(KeyCode.Alpha3)) arrow.SetDirection(-90);  // Up
            if (Input.GetKeyDown(KeyCode.Alpha4)) arrow.SetDirection(-135); // Up-Left
            if (Input.GetKeyDown(KeyCode.Alpha5)) arrow.SetDirection(180);  // Left
            if (Input.GetKeyDown(KeyCode.Alpha6)) arrow.SetDirection(135);  // Down-Left
            if (Input.GetKeyDown(KeyCode.Alpha7)) arrow.SetDirection(90);   // Down
            if (Input.GetKeyDown(KeyCode.Alpha8)) arrow.SetDirection(45);   // Down-Right
        }

        // Update debug display if available
        if (debugText != null)
        {
            debugText.text = $"Arrow Angle: {currentAngle:F1}°\n" +
                            $"Use Arrow Keys or Numbers 1-8";
        }
    }
}


