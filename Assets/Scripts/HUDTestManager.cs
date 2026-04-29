using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class HUDTestManager : MonoBehaviour
{
    public HUDController hudController;
    public ROS2WebSocketConnector ros2Connector;

    void Start()
    {
        // Test the HUD with local data first
        StartCoroutine(TestSequence());
    }

    IEnumerator TestSequence()
    {
        yield return new WaitForSeconds(2f);

        Debug.Log("Testing local HUD updates...");
        hudController.TestAllSystems();

        yield return new WaitForSeconds(2f);

        Debug.Log("Waiting for ROS2 connection...");
        // ROS2 data should start flowing automatically
    }
}
