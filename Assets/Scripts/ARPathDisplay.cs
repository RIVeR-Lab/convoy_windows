using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class PathPoint
{
    public float x;
    public float y;
}

[System.Serializable]
public class PathData
{
    public PathPoint[] left_wall;
    public PathPoint[] right_wall;
    public PathPoint[] center_line;
    public float track_width;
    public float curvature;
    public float recommended_speed;
    public float vehicle_yaw;
}
public class ARPathDisplay : MonoBehaviour
{
    [Header("Line Renderers")]
    public LineRenderer leftWallLine;
    public LineRenderer rightWallLine;
    public LineRenderer centerPathLine;

    [Header("Prefabs")]
    public GameObject arrowPrefab;
    public GameObject wallWarningPrefab;
    public Transform pathIndicatorParent;

    [Header("Vehicle Tracking")]
    public float vehicleYaw = 0f;  // Updated from ROS odometry
    public bool useVehicleOrientation = true;  // Toggle for testing

    [Header("Visualization Settings")]
    [Range(0.1f, 2.0f)]
    public float pathHeight = 0.5f;
    [Range(0.5f, 5.0f)]
    public float pathScale = 2.5f;
    [Range(5, 50)]
    public int maxPathPoints = 30;

    [Header("Ground Settings")]
    [Range(0.5f, 2.5f)]
    public float groundOffset = 1.5f;  // Distance from head to ground

    [Range(-5.0f, 5.0f)]
    public float centerOffset = 0f;  // Positive = right, Negative = left

    [Range(-30.0f, 30.0f)]
    public float forwardOffset = 0f; // Positive = into the page(Infront of camera), Negative = out of the page (Behind camera)

    [Header("Coordinate Mapping")]
    // public bool swapXY = false;
    // public bool invertX = false;
    // public bool invertY = false;

    [Range(-180f, 180f)]
    public float rotationOffset = 0f;

    [Tooltip("Set to 0 for editor testing, 1.5 for HoloLens")]
    public float editorGroundAdjustment = 1.5f;  // Add this
    [Header("Colors")]
    public Gradient safeGradient;
    public Gradient warningGradient;
    public Gradient dangerGradient;

    [Header("Update Settings")]
    public float updateInterval = 0.1f;

    [Header("Camera Reference")]
    public Transform hololensCamera; // Public so you can assign in Inspector

    private List<GameObject> activeArrows = new List<GameObject>();
    private float lastUpdateTime;

    void Start()
    {
        if (hololensCamera == null)
        {
            // Try to find MR camera
            GameObject mrCamera = GameObject.Find("Main Camera");
            if (mrCamera != null)
            {
                hololensCamera = mrCamera.transform;
            }
            else
            {
                Debug.LogError("Camera not found! Assign manually.");
            }
        }
        SetupGradients();
        SetupLineRenderers();
    }

    void SetupGradients()
    {
        // Safe gradient (green fading)
        safeGradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0] = new GradientColorKey(Color.green, 0.0f);
        colorKeys[1] = new GradientColorKey(Color.green * 0.3f, 1.0f);

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphaKeys[1] = new GradientAlphaKey(0.2f, 1.0f);

        safeGradient.SetKeys(colorKeys, alphaKeys);

        // Warning gradient (yellow)
        warningGradient = new Gradient();
        colorKeys[0] = new GradientColorKey(Color.yellow, 0.0f);
        colorKeys[1] = new GradientColorKey(Color.yellow * 0.3f, 1.0f);
        warningGradient.SetKeys(colorKeys, alphaKeys);

        // Danger gradient (red)
        dangerGradient = new Gradient();
        colorKeys[0] = new GradientColorKey(Color.red, 0.0f);
        colorKeys[1] = new GradientColorKey(Color.red * 0.3f, 1.0f);
        dangerGradient.SetKeys(colorKeys, alphaKeys);
    }

    void SetupLineRenderers()
    {
        // Setup left wall
        if (leftWallLine != null)
        {
            leftWallLine.startWidth = 0.1f;
            leftWallLine.endWidth = 0.02f;
            leftWallLine.colorGradient = safeGradient;
        }

        // Setup right wall
        if (rightWallLine != null)
        {
            rightWallLine.startWidth = 0.1f;
            rightWallLine.endWidth = 0.02f;
            rightWallLine.colorGradient = safeGradient;
        }

        // Setup center path
        if (centerPathLine != null)
        {
            centerPathLine.startWidth = 0.15f;
            centerPathLine.endWidth = 0.03f;
            centerPathLine.colorGradient = safeGradient;
        }
    }

    public void UpdateVehiclePose(float yawRadians)
    {
        // Convert ROS yaw (radians, counter-clockwise) to Unity (degrees, clockwise)
        vehicleYaw = -yawRadians * Mathf.Rad2Deg;
    }

    public void UpdatePathFromJSON(string jsonData)
    {
        try
        {
            PathData pathData = JsonUtility.FromJson<PathData>(jsonData);
            UpdatePath(pathData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse path JSON: {e.Message}");
        }
    }

    public void UpdatePath(PathData pathData)
    {
        // Throttle updates
        if (Time.time - lastUpdateTime < updateInterval)
            return;
        lastUpdateTime = Time.time;

        // Update vehicle orientation from ROS data
        if (useVehicleOrientation)
        {
            UpdateVehiclePose(pathData.vehicle_yaw);
        }

        // Update walls
        UpdateWallLine(leftWallLine, pathData.left_wall, 1.0f);
        UpdateWallLine(rightWallLine, pathData.right_wall, -1.0f);

        // Update center path
        UpdateCenterLine(centerPathLine, pathData.center_line);

        // Update visual indicators based on track conditions
        UpdatePathColor(pathData.track_width, pathData.curvature);

        // Update directional arrows
        UpdateArrows(pathData.center_line);
    }

    void UpdateWallLine(LineRenderer line, PathPoint[] points, float sideOffset)
    {
        if (line == null || points == null || points.Length == 0)
            return;

        int pointCount = Mathf.Min(points.Length, maxPathPoints);
        line.positionCount = pointCount;

        Vector3 headPosition = hololensCamera.position;
        float groundY = headPosition.y - groundOffset;


        float orientationYaw = hololensCamera.eulerAngles.y + rotationOffset;
        Quaternion flatRotation = Quaternion.Euler(0, orientationYaw, 0);

        /**
        float orientationYaw;
        if (useVehicleOrientation)
        {
            orientationYaw = vehicleYaw + rotationOffset;
        }
        else
        {
            orientationYaw = hololensCamera.eulerAngles.y + rotationOffset;
        }
        **/
        //float headYaw = hololensCamera.eulerAngles.y;
        //Quaternion flatRotation = Quaternion.Euler(0, headYaw + rotationOffset, 0);
        //Quaternion flatRotation = Quaternion.Euler(0, orientationYaw, 0);

        Vector3 flatForward = flatRotation * Vector3.forward;

        Vector3[] positions = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            // Convert ROS 2D coordinates to Unity
            // ROS: x=forward, y=left → Unity: z=forward, x=right (so -y)
            Vector3 unityPoint = ROSUnityCoordinates.ROSToUnity2D(
                points[i].x,
                points[i].y
            );

            // Apply scale and offsets
            Vector3 scaledPoint = new Vector3(
                (unityPoint.x * pathScale) + centerOffset,
                0,
                unityPoint.z * pathScale
            );

            // Rotate to match head orientation
            Vector3 rotatedOffset = flatRotation * scaledPoint;

            // Position in world space
            positions[i] = new Vector3(
                headPosition.x + rotatedOffset.x + (flatForward.x * forwardOffset),
                groundY + pathHeight,
                headPosition.z + rotatedOffset.z + (flatForward.z * forwardOffset)
            );
        }

        line.SetPositions(positions);
    }

    void UpdateCenterLine(LineRenderer line, PathPoint[] points)
    {
        if (line == null || points == null || points.Length == 0)
            return;

        int pointCount = Mathf.Min(points.Length, maxPathPoints);
        line.positionCount = pointCount;

        Vector3 headPosition = hololensCamera.position;
        float headYaw = hololensCamera.eulerAngles.y;
        Quaternion flatRotation = Quaternion.Euler(0, headYaw + rotationOffset, 0);
        float groundY = headPosition.y - groundOffset;
        Vector3 flatForward = flatRotation * Vector3.forward;

        Vector3[] positions = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            // Convert ROS 2D coordinates to Unity
            Vector3 unityPoint = ROSUnityCoordinates.ROSToUnity2D(
                points[i].x,
                points[i].y
            );

            // Apply scale and offsets
            Vector3 scaledPoint = new Vector3(
                (unityPoint.x * pathScale) + centerOffset,
                0,
                unityPoint.z * pathScale
            );

            // Rotate to match head orientation
            Vector3 rotatedOffset = flatRotation * scaledPoint;

            // Position in world space
            positions[i] = new Vector3(
                headPosition.x + rotatedOffset.x + (flatForward.x * forwardOffset),
                groundY + pathHeight,
                headPosition.z + rotatedOffset.z + (flatForward.z * forwardOffset)
            );
        }

        line.SetPositions(positions);
    }
    void UpdateArrows(PathPoint[] centerLine)
    {
        foreach (var arrow in activeArrows)
        {
            Destroy(arrow);
        }
        activeArrows.Clear();

        if (arrowPrefab == null || centerLine == null)
            return;

        Vector3 headPosition = hololensCamera.position;
        float headYaw = hololensCamera.eulerAngles.y;
        Quaternion flatRotation = Quaternion.Euler(0, headYaw + rotationOffset, 0);
        float groundY = headPosition.y - groundOffset;
        Vector3 flatForward = flatRotation * Vector3.forward;

        for (int i = 2; i < centerLine.Length && i < 15; i += 3)
        {
            GameObject arrow = Instantiate(arrowPrefab, pathIndicatorParent);

            // Convert current point
            Vector3 currentUnity = ROSUnityCoordinates.ROSToUnity2D(
                centerLine[i].x,
                centerLine[i].y
            );

            Vector3 scaledPoint = new Vector3(
                (currentUnity.x * pathScale) + centerOffset,
                0,
                currentUnity.z * pathScale
            );

            Vector3 rotatedOffset = flatRotation * scaledPoint;

            arrow.transform.position = new Vector3(
                headPosition.x + rotatedOffset.x + (flatForward.x * forwardOffset),
                groundY + pathHeight + 0.1f,
                headPosition.z + rotatedOffset.z + (flatForward.z * forwardOffset)
            );

            // Point arrow along path direction
            if (i > 0)
            {
                Vector3 prevUnity = ROSUnityCoordinates.ROSToUnity2D(
                    centerLine[i - 1].x,
                    centerLine[i - 1].y
                );

                Vector3 prevScaled = new Vector3(
                    prevUnity.x * pathScale,
                    0,
                    prevUnity.z * pathScale
                );

                Vector3 direction = scaledPoint - prevScaled;
                direction.y = 0;

                if (direction.magnitude > 0.01f)
                {
                    arrow.transform.rotation = flatRotation * Quaternion.LookRotation(direction);
                }
            }

            activeArrows.Add(arrow);
        }
    }
    void UpdatePathColor(float trackWidth, float curvature)
    {
        // Change color based on difficulty
        Gradient targetGradient = safeGradient;

        if (trackWidth < 2.0f || curvature > 0.5f)
        {
            targetGradient = dangerGradient;
        }
        else if (trackWidth < 3.0f || curvature > 0.3f)
        {
            targetGradient = warningGradient;
        }

        // Apply to center line
        if (centerPathLine != null)
        {
            centerPathLine.colorGradient = targetGradient;
        }
    }

    // Called from ROS2 connector when new path data arrives
    public void OnPathDataReceived(string pathJson)
    {
        UpdatePathFromJSON(pathJson);
    }

    public static class ROSUnityCoordinates
    {
        /// <summary>
        /// Converts a 2D point from ROS coordinates to Unity coordinates
        /// ROS: X=Forward, Y=Left
        /// Unity: X=Right, Z=Forward
        /// </summary>
        public static Vector3 ROSToUnity2D(float rosX, float rosY, float height = 0f)
        {
            return new Vector3(
                -rosY,    // ROS Y (left) → Unity -X (right is positive)
                height,   // Height in Unity
                rosX      // ROS X (forward) → Unity Z (forward)
            );
        }

        /// <summary>
        /// Converts a 3D point from ROS coordinates to Unity coordinates
        /// ROS: X=Forward, Y=Left, Z=Up
        /// Unity: X=Right, Y=Up, Z=Forward
        /// </summary>
        public static Vector3 ROSToUnity3D(float rosX, float rosY, float rosZ)
        {
            return new Vector3(
                -rosY,    // ROS Y (left) → Unity -X
                rosZ,     // ROS Z (up) → Unity Y
                rosX      // ROS X (forward) → Unity Z
            );
        }

        /// <summary>
        /// Converts a quaternion from ROS to Unity
        /// </summary>
        public static Quaternion ROSToUnityRotation(float rosX, float rosY, float rosZ, float rosW)
        {
            return new Quaternion(
                rosY,     // ROS Y → Unity X
                -rosZ,    // ROS Z → Unity -Y
                -rosX,    // ROS X → Unity -Z
                rosW      // W stays the same
            );
        }
    }

    [ContextMenu("Test Path Visualization")]
    void TestVisualization()
    {
        string testJson = @"{
        ""left_wall"": [
            {""x"": 1.5, ""y"": 0}, {""x"": 1.5, ""y"": 2}, {""x"": 1.5, ""y"": 4},
            {""x"": 1.5, ""y"": 6}, {""x"": 1.5, ""y"": 8}, {""x"": 1.5, ""y"": 10}
        ],
        ""right_wall"": [
            {""x"": -1.5, ""y"": 0}, {""x"": -1.5, ""y"": 2}, {""x"": -1.5, ""y"": 4},
            {""x"": -1.5, ""y"": 6}, {""x"": -1.5, ""y"": 8}, {""x"": -1.5, ""y"": 10}
        ],
        ""center_line"": [
            {""x"": 0, ""y"": 0}, {""x"": 0, ""y"": 2}, {""x"": 0, ""y"": 4},
            {""x"": 0, ""y"": 6}, {""x"": 0, ""y"": 8}, {""x"": 0, ""y"": 10}
        ],
        ""track_width"": 3.0,
        ""curvature"": 0.1
    }";

        UpdatePathFromJSON(testJson);
    }

    [ContextMenu("Test Arrow Visibility")]
    void TestArrowVisibility()
    {
        if (arrowPrefab == null)
        {
            Debug.LogError("No arrow prefab assigned");
            return;
        }

        // Spawn directly in front of camera, 2 meters away
        Vector3 spawnPos = hololensCamera.position + hololensCamera.forward * 2f;

        GameObject testArrow = Instantiate(arrowPrefab);
        testArrow.name = "TEST_ARROW_DELETE_ME";
        testArrow.transform.position = spawnPos;
        testArrow.transform.localScale = Vector3.one * 0.5f;

        Debug.Log($"Test arrow spawned at {spawnPos}");
    }

}