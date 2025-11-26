using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tracks hand movement to calculate frisbee throw vector
/// Uses rotation and velocity with the last 20-30 position samples
/// </summary>
public class TrackFrisbeeThrow : MonoBehaviour
{
    [Header("Hand Tracking")]
    [SerializeField]
    private Transform leftHandTransform;
    
    [SerializeField]
    private Transform rightHandTransform;

    [Header("Tracking Settings")]
    [Tooltip("Number of position samples to store (20-30 recommended)")]
    [SerializeField]
    private int maxSamples = 25;

    [Tooltip("Minimum speed to consider a valid throw (m/s)")]
    [SerializeField]
    private float minimumThrowSpeed = 1.5f;

    [Tooltip("Weight of rotation vs velocity in final direction (0=only velocity, 1=only rotation)")]
    [SerializeField]
    [Range(0f, 1f)]
    private float rotationWeight = 0.3f;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugInfo = true;

    // Public properties
    public Vector3 LeftThrowVector { get; private set; }
    public Vector3 RightThrowVector { get; private set; }
    public float LeftThrowSpeed { get; private set; }
    public float RightThrowSpeed { get; private set; }


    // Sample queues for position history
    private readonly Queue<Vector3> leftHandSamples = new();
    private readonly Queue<Vector3> rightHandSamples = new();
    
    // Sample queues for rotation history
    private readonly Queue<Quaternion> leftHandRotations = new();
    private readonly Queue<Quaternion> rightHandRotations = new();

    private bool _isTracking  = false;

    private void Update()
    {
        if (_isTracking)
        {
            TrackHand(leftHandTransform, leftHandSamples, leftHandRotations);
            TrackHand(rightHandTransform, rightHandSamples, rightHandRotations);
        }
    }

    /// <summary>
    /// Adds current hand position and rotation to the sample queue
    /// </summary>
    private void TrackHand(Transform handTransform, Queue<Vector3> positionSamples, Queue<Quaternion> rotationSamples)
    {
        // Add current position
        positionSamples.Enqueue(handTransform.position);
        
        // Add current rotation
        rotationSamples.Enqueue(handTransform.rotation);

        // Keep only the last N samples (20-30)
        while (positionSamples.Count > maxSamples)
        {
            positionSamples.Dequeue();
        }
        
        while (rotationSamples.Count > maxSamples)
        {
            rotationSamples.Dequeue();
        }
    }

    /// <summary>
    /// Calculate throw vector when frisbee is released
    /// Call this when the player releases the frisbee
    /// </summary>
    public void CalculateLeftThrow()
    {
        CalculateThrowVector(
            leftHandSamples, 
            leftHandRotations, 
            leftHandTransform,
            out Vector3 throwVector, 
            out float throwSpeed
        );
        
        LeftThrowVector = throwVector;
        LeftThrowSpeed = throwSpeed;

        if (showDebugInfo)
        {
            Debug.Log($"Left Throw - Vector: {throwVector}, Speed: {throwSpeed:F2} m/s");
        }
    }

    /// <summary>
    /// Calculate throw vector for right hand
    /// </summary>
    public void CalculateRightThrow()
    {
        CalculateThrowVector(
            rightHandSamples, 
            rightHandRotations, 
            rightHandTransform,
            out Vector3 throwVector, 
            out float throwSpeed
        );
        
        RightThrowVector = throwVector;
        RightThrowSpeed = throwSpeed;

        if (showDebugInfo)
        {
            Debug.Log($"Right Throw - Vector: {throwVector}, Speed: {throwSpeed:F2} m/s");
        }
    }

    /// <summary>
    /// Calculates throw direction using velocity from position samples and hand rotation
    /// </summary>
    private void CalculateThrowVector(
        Queue<Vector3> positionSamples, 
        Queue<Quaternion> rotationSamples,
        Transform handTransform,
        out Vector3 throwVector, 
        out float throwSpeed)
    {
        throwVector = Vector3.forward;
        throwSpeed = 0f;

        if (positionSamples.Count < 2)
        {
            Debug.LogWarning("Not enough samples to calculate throw vector!");
            return;
        }

        // 1. Calculate velocity vector from position samples
        Vector3 velocityVector = CalculateVelocityFromSamples(positionSamples, out throwSpeed);

        // 2. Get direction from hand rotation
        Quaternion averageRotation = CalculateAverageRotation(rotationSamples);
        Vector3 rotationDirection = averageRotation * Vector3.forward;

        // 3. Combine velocity and rotation based on weight
        if (throwSpeed >= minimumThrowSpeed)
        {
            // Blend velocity direction with rotation direction
            throwVector = Vector3.Lerp(
                velocityVector.normalized, 
                rotationDirection, 
                rotationWeight
            ).normalized;

            return;
        }

        // If speed too low, use rotation only
        throwVector = rotationDirection;
            
        if (showDebugInfo)
        {
            Debug.LogWarning($"Throw speed too low ({throwSpeed:F2} m/s), using rotation only");
        }
        
    }

    /// <summary>
    /// Calculates average velocity from position samples
    /// </summary>
    private Vector3 CalculateVelocityFromSamples(Queue<Vector3> samples, out float speed)
    {
        Vector3[] sampleArray = samples.ToArray();
        Vector3 totalVelocity = Vector3.zero;
        int validSamples = 0;

        // Calculate velocity between consecutive samples
        for (int i = 1; i < sampleArray.Length; i++)
        {
            Vector3 velocity = (sampleArray[i] - sampleArray[i - 1]) / Time.fixedDeltaTime;
            totalVelocity += velocity;
            validSamples++;
        }

        Vector3 averageVelocity = validSamples > 0 ? totalVelocity / validSamples : Vector3.zero;
        speed = averageVelocity.magnitude;

        return averageVelocity;
    }

    /// <summary>
    /// Calculates average rotation from rotation samples
    /// Using quaternion averaging technique
    /// </summary>
    private Quaternion CalculateAverageRotation(Queue<Quaternion> rotations)
    {
        if (rotations.Count == 0)
        {
            return Quaternion.identity;
        }

        Quaternion[] rotArray = rotations.ToArray();
        
        // Use the most recent rotations (last 5-10 samples) for better responsiveness
        int samplesToUse = Mathf.Min(10, rotArray.Length);
        int startIndex = rotArray.Length - samplesToUse;

        Vector4 cumulative = Vector4.zero;

        for (int i = startIndex; i < rotArray.Length; i++)
        {
            Quaternion q = rotArray[i];
            
            // Ensure consistent quaternion signs
            if (Vector4.Dot(cumulative, new Vector4(q.x, q.y, q.z, q.w)) < 0)
            {
                cumulative -= new Vector4(q.x, q.y, q.z, q.w);
            }
            else
            {
                cumulative += new Vector4(q.x, q.y, q.z, q.w);
            }
        }

        cumulative /= samplesToUse;
        
        return new Quaternion(cumulative.x, cumulative.y, cumulative.z, cumulative.w).normalized;
    }

    /// <summary>
    /// Starts tracking - call when player grabs frisbee
    /// </summary>
    public void StartTracking()
    {
        _isTracking = true;
        ClearSamples();
        
        if (showDebugInfo)
        {
            Debug.Log("Started tracking frisbee throw");
        }
    }

    /// <summary>
    /// Stops tracking - call when player releases frisbee
    /// </summary>
    public void StopTracking()
    {
        _isTracking = false;
        
        if (showDebugInfo)
        {
            Debug.Log("Stopped tracking frisbee throw");
        }
    }

    /// <summary>
    /// Clears all stored samples
    /// </summary>
    public void ClearSamples()
    {
        leftHandSamples.Clear();
        rightHandSamples.Clear();
        leftHandRotations.Clear();
        rightHandRotations.Clear();
    }

    // Visualize throw vectors in Scene view
    private void OnDrawGizmos()
    {
        if (!_isTracking) return;

        // Draw left hand throw vector
        if (leftHandTransform != null && leftHandSamples.Count > 0)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(leftHandTransform.position, leftHandTransform.position + LeftThrowVector * 0.5f);
            Gizmos.DrawWireSphere(leftHandTransform.position, 0.02f);
        }

        // Draw right hand throw vector
        if (rightHandTransform != null && rightHandSamples.Count > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(rightHandTransform.position, rightHandTransform.position + RightThrowVector * 0.5f);
            Gizmos.DrawWireSphere(rightHandTransform.position, 0.02f);
        }
    }
}