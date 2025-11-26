using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Oculus.Interaction;
using System;
using System.Linq;

/// <summary>
/// Validates if the player's hand follows the correct throwing trajectory
/// using Meta Interaction SDK RayInteractable components on waypoints.
/// Calculates throw force based on hand velocity/acceleration.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class MovementValidator : MonoBehaviour
{
    [Header("Trajectory Settings")]
    [Tooltip("Parent GameObject containing all waypoints in order")]
    [SerializeField]
    private Transform trajectoryParent;

    [Tooltip("Minimum percentage of waypoints that must be hit (0-1)")]
    [SerializeField]
    [Range(0f, 1f)]
    private float minimumWaypointsPercentage = 0.7f;

    [Header("Hand Tracking")]
    [Tooltip("Transform of the player's hand (XR Controller)")]
    [SerializeField]
    private Transform handTransform;

    [Header("Force Calculation")]
    [Tooltip("Minimum hand speed to trigger throw (m/s)")]
    [SerializeField]
    private float minimumThrowSpeed = 2f;

    [Tooltip("Maximum hand speed for force calculation (m/s)")]
    [SerializeField]
    private float maximumThrowSpeed = 8f;

    [Tooltip("Multiplier for converting hand speed to throw force")]
    [SerializeField]
    private float forceMultiplier = 2f;

    [Header("Visual Feedback")]
    [Tooltip("Show debug information in console")]
    [SerializeField]
    private bool showDebugInfo = true;

    [Header("Events")]
    [Tooltip("Called when trajectory is valid and throw is triggered")]
    public UnityEvent<float, float> OnValidThrow;

    [Tooltip("Called when trajectory validation fails")]
    public UnityEvent OnInvalidTrajectory;


    private Dictionary<GameObject, bool> waypointHitStatus = new();

    
    private bool isTracking = false;

    // Hand velocity tracking
    private Vector3 previousHandPosition;
    private Vector3 handVelocity;
    private Vector3 previousHandVelocity;
    private Vector3 handAcceleration;
    private float maxSpeedReached = 0f;

    private void Start()
    {
        InitializeWaypoints();
        previousHandPosition = handTransform.position;
    }

    /// <summary>
    /// Collects all waypoint children and their RayInteractable components
    /// </summary>
    private void InitializeWaypoints()
    {
        if (trajectoryParent == null)
        {
            Debug.LogError("Trajectory parent not assigned!");
            return;
        }

       GameObject[] waypoints =  Utils.GetChildren(trajectoryParent);

        foreach (GameObject waypoint in waypoints)
        {
            waypointHitStatus[waypoint] = false;
        }
    }

    private void Update()
    {
        if (isTracking)
        {
            UpdateHandVelocity();
            transform.rotation = Quaternion.identity;
        }
    }




    public void WaypointHit(GameObject waypoint)
    {
        if (waypointHitStatus.ContainsKey(waypoint))
        {
            waypointHitStatus[waypoint] = true;
            int index = waypointHitStatus.Keys.ToList().IndexOf(waypoint);
            ChangeWaypointColor(index, Color.green);

            if (showDebugInfo)
            {
                Debug.Log($"Waypoint {index + 1} hit!");
            }

            ValidateTrajectory();
       
       /*
        bool isValid = ValidateTrajectory();
        
        if (isValid)
        {
            float calculatedForce = CalculateThrowForce();
            float calculatedUpwardBias = CalculateUpwardBias();

            if (showDebugInfo)
            {
                Debug.Log($"Valid trajectory! Force: {calculatedForce:F2}, Upward: {calculatedUpwardBias:F2}, Max Speed: {maxSpeedReached:F2} m/s");
            }
            
            OnValidThrow?.Invoke(calculatedForce, calculatedUpwardBias);
            return;
        }
        */
      
        int hitCount = 0;
        foreach (bool hit in waypointHitStatus.Values)
        {
            if (hit)
            {
                hitCount++;
            } 
        } 
            
        if (showDebugInfo)
        {
            Debug.Log($"Invalid trajectory - only {hitCount}/{waypointHitStatus.Count} waypoints hit");
        }
        
        OnInvalidTrajectory?.Invoke();
        }
    }



    /// <summary>
    /// Calculates hand velocity and acceleration each frame
    /// </summary>
    private void UpdateHandVelocity()
    {
        Vector3 currentPosition = handTransform.position;
        handVelocity = (currentPosition - previousHandPosition) / Time.deltaTime;
        
        // Calculate acceleration
        handAcceleration = (handVelocity - previousHandVelocity) / Time.deltaTime;
        previousHandVelocity = handVelocity;

        // Track maximum speed reached
        float currentSpeed = handVelocity.magnitude;
        if (currentSpeed > maxSpeedReached)
        {
            maxSpeedReached = currentSpeed;
        }

        previousHandPosition = currentPosition;
    }

    /// <summary>
    /// Starts tracking the hand trajectory
    /// </summary>
    public void StartTracking()
    {
        isTracking = true;
        maxSpeedReached = 0f;

        previousHandPosition = handTransform.position;
        handVelocity = Vector3.zero;
        previousHandVelocity = Vector3.zero;

        ResetWaypointColors();
        
        if (showDebugInfo)
        {
            Debug.Log("Started trajectory tracking (Meta Ray Interactable mode)");
        }
    }

    /// <summary>
    /// Stops tracking and validates the trajectory
    /// </summary>
    public void StopTracking()
    {
        isTracking = false;   


        foreach(GameObject waypoint in waypointHitStatus.Keys.ToList())
        {
            waypointHitStatus[waypoint] = false;
        }
    }

    /// <summary>
    /// Checks if enough waypoints were hit
    /// </summary>
    private bool ValidateTrajectory()
    {
        int waypointsHitCount = 0;

        foreach (bool hit in waypointHitStatus.Values)
        {
            if (hit)
            {
                waypointsHitCount++;
            } 
        }

        float percentage = (float)waypointsHitCount / waypointHitStatus.Count;
        bool isValid = percentage >= minimumWaypointsPercentage;

        if (showDebugInfo)
        {
            Debug.Log($"Waypoints hit: {waypointsHitCount}/{waypointHitStatus.Count} ({percentage:P0}) - Valid: {isValid}");
        }

        return isValid;
    }

    /// <summary>
    /// Calculates throw force based on maximum hand speed reached
    /// </summary>
    private float CalculateThrowForce()
    {
        float clampedSpeed = Mathf.Clamp(maxSpeedReached, minimumThrowSpeed, maximumThrowSpeed);
        float normalizedSpeed = (clampedSpeed - minimumThrowSpeed) / (maximumThrowSpeed - minimumThrowSpeed);
        float throwForce = Mathf.Lerp(10f, 20f, normalizedSpeed) * forceMultiplier;

        return throwForce;
    }

    /// <summary>
    /// Calculates upward bias based on hand movement direction
    /// </summary>
    private float CalculateUpwardBias()
    {
        float upwardVelocity = handVelocity.y;
        float upwardBias = Mathf.Clamp(upwardVelocity * 2f, 0f, 6f);
        return upwardBias;
    }

    /// <summary>
    /// Changes the color of a waypoint for visual feedback
    /// </summary>
    private void ChangeWaypointColor(int index, Color color)
    {
        if (index < 0 || index >= waypointHitStatus.Count)
        {
            return;
        } 

        GameObject[] waypoints = waypointHitStatus.Keys.ToArray();

        if (waypoints[index].TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = color;
        }
    }

    /// <summary>
    /// Resets all waypoint colors to default
    /// </summary>
    public void ResetWaypointColors()
    {
        Color defaultColor = new(0.8f, 0.8f, 0.8f, 0.5f);
        
        for (int i = 0; i < waypointHitStatus.Count; i++)
        {
            ChangeWaypointColor(i, defaultColor);
        }
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (waypointHitStatus == null || waypointHitStatus.Count == 0)
        {
            return;
        }

        // Draw line through waypoints
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypointHitStatus.Count - 1; i++)
        {
            GameObject[] waypoints = waypointHitStatus.Keys.ToArray();
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].transform.position, waypoints[i + 1].transform.position);
            }
        }

        // Draw spheres at waypoints
        for (int i = 0; i < waypointHitStatus.Count; i++)
        {
            GameObject[] waypoints = waypointHitStatus.Keys.ToArray();
            if (waypoints[i] != null)
            {
                Gizmos.color = waypointHitStatus[waypoints[i]] ? Color.green : new Color(1f, 1f, 1f, 0.3f);
                Gizmos.DrawWireSphere(waypoints[i].transform.position, 0.05f);
            }
        }

        // Draw hand position when tracking
        if (isTracking && handTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(handTransform.position, 0.03f);
        }
    }
}