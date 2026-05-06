using UnityEngine;

/// <summary>
/// Keeps a world-space UI panel positioned in front of the player's center eye every LateUpdate.
/// Replaces the deleted static <c>DisplayInPlayerFront</c> component for the Frisbee scene UI.
/// </summary>
/// <remarks>Execution order 1000 guarantees this runs after all VR camera updates.</remarks>
[DefaultExecutionOrder(1000)]
public class FollowPlayerHead : MonoBehaviour
{
    /// <summary>The VR center eye anchor transform (OVRCameraRig.centerEyeAnchor or equivalent).</summary>
    [SerializeField]
    private Transform centerEyeTransform;

    [Header("Positioning Settings")]
    /// <summary>Distance in front of the player at which the panel is placed.</summary>
    [SerializeField]
     private float distanceFromPlayer = 2.0f;

    /// <summary>Vertical offset relative to eye height (negative = below eye level).</summary>
    [SerializeField]
    private float heightOffset = -0.5f;

    /// <summary>Lateral offset relative to the eye's right direction.</summary>
    [SerializeField]
    private float horizontalOffset = 0f;
    
    private void Awake()
    {
           if (centerEyeTransform == null)
        {
            Debug.LogError("Center Eye Transform is not assigned in the inspector.");
            return;
        }

        transform.SetPositionAndRotation(CalculateBaseTarget(), Quaternion.identity);
    }

    private void LateUpdate()
    {
        HandlePosition();
    }

    private void HandlePosition()
    {
        transform.position = CalculateBaseTarget();
    }

    private Vector3 CalculateBaseTarget()
    {
        Vector3 target = centerEyeTransform.position;
        target += centerEyeTransform.right * horizontalOffset;
        target += centerEyeTransform.forward * distanceFromPlayer;
        target += centerEyeTransform.up * heightOffset;
        return target;
    }
}