using UnityEngine;

/// <summary>
/// Continuously rotates the GameObject around the world X axis to simulate a Ferris wheel.
/// </summary>
public class RotateWheel : MonoBehaviour
{
    /// <summary>Rotation speed in degrees per second.</summary>
    public float speed = 20f;

    void Update()
    {
        transform.Rotate(Vector3.right * speed * Time.deltaTime);
    }
}
