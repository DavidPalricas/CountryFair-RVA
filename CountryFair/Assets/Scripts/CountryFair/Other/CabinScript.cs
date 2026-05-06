using UnityEngine;

/// <summary>
/// Keeps a Ferris-wheel cabin gondola upright by counter-rotating against its parent wheel each frame.
/// Attach to a cabin that is a direct child of the rotating wheel bone.
/// </summary>
public class CabinScript : MonoBehaviour
{
    Transform wheel;

    void Start()
    {
        wheel = transform.parent;
    }

    /// <summary>Cancels the parent wheel's Z rotation so the cabin stays level while the wheel spins.</summary>
    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            -wheel.eulerAngles.z
        );
    }
}