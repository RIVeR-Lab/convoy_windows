using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirectionIndicator : MonoBehaviour
{
    [Header("Arrow Settings")]
    public float rotationSpeed = 5f;
    public bool smoothRotation = true;

    private RectTransform arrowTransform;
    private float targetAngle = 0f;

    void Start()
    {
        arrowTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (smoothRotation)
        {
            // Smooth rotation to target
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            arrowTransform.rotation = Quaternion.Lerp(
                arrowTransform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    public void SetDirection(float angle)
    {
        targetAngle = angle;

        if (!smoothRotation)
        {
            // Instant rotation
            arrowTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void PointToTarget(Vector3 targetPosition)
    {
        // Calculate angle to target
        Vector3 direction = targetPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        SetDirection(angle - 90); // Subtract 90 if arrow points up by default
    }
}
