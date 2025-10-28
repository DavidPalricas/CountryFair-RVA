using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Handles camera rotation based on Meta Quest 3 head tracking.
/// </summary>
public class RotateCamera : MonoBehaviour
{
    /// <summary>Smoothing factor for rotation interpolation.</summary>
    [Header("Head Tracking Settings")]
    [SerializeField]
    private readonly float rotationSmoothness = 5f;

    /// <summary>Reference to the XR Rig or head tracking transform.</summary>
    [SerializeField]
    private Transform headTransform;

    /// <summary>Current horizontal rotation angle in degrees.</summary>
    private float yRotation;

    /// <summary>Target rotation from head tracking.</summary>
    private Quaternion targetRotation;

    /// <summary>
    /// Initializes the camera and gets the head tracking reference.
    /// </summary>
    private void Start()
    {
        // If no head transform assigned, try to find it automatically
        if (headTransform == null)
        {
            headTransform = GetComponent<Transform>();
        }

        yRotation = transform.localEulerAngles.y;
    }

    /// <summary>
    /// Main update loop that reads head tracking and applies camera rotation.
    /// </summary>
    private void Update()
    {
        ReadHeadTracking();
        ApplyCameraRotation();
    }

    /// <summary>
    /// Reads the current head rotation from XR Input.
    /// </summary>
    private void ReadHeadTracking()
    {
        // Get node states into a list (InputTracking.GetNodeStates does not use 'out')
        List<XRNodeState> nodeStates = new();
        InputTracking.GetNodeStates(nodeStates);

        // Find the head node and read its rotation if available
        for (int i = 0; i < nodeStates.Count; i++)
        {
            XRNodeState ns = nodeStates[i];
            if (ns.nodeType == XRNode.Head)
            {
                if (ns.TryGetRotation(out Quaternion headRotation))
                {
                    targetRotation = headRotation;
                }
                break;
            }
        }
    }

    /// <summary>
    /// Applies the head tracking rotation with smoothing to the camera.
    /// </summary>
    private void ApplyCameraRotation()
    {
        // Smoothly interpolate to the target rotation
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            rotationSmoothness * Time.deltaTime
        );
    }
}