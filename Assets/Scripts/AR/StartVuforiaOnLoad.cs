using UnityEngine;
using Vuforia;

public class StartVuforiaOnLoad : MonoBehaviour
{
    private void OnEnable()
    {
        // Find and enable the AR Camera GameObject
        GameObject arCamera = GameObject.Find("ARCamera"); // Ensure the name matches your AR Camera's name
        if (arCamera != null)
        {
            arCamera.SetActive(true);

            // Enable Vuforia Behaviour to start Vuforia
            VuforiaBehaviour vuforiaBehaviour = arCamera.GetComponent<VuforiaBehaviour>();
            if (vuforiaBehaviour != null && !VuforiaApplication.Instance.IsRunning)
            {
                vuforiaBehaviour.enabled = true; // Enabling this should automatically start Vuforia
                VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted; // Optional: add a callback to confirm start
            }
        }
        else
        {
            Debug.LogError("AR Camera not found in the scene!");
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid any potential memory leaks
        VuforiaApplication.Instance.OnVuforiaStarted -= OnVuforiaStarted;
    }

    private void OnVuforiaStarted()
    {
        Debug.Log("Vuforia started successfully.");
    }
}