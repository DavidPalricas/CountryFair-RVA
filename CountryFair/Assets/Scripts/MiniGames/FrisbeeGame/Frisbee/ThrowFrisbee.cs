using UnityEngine;

/// <summary>
/// Simulates realistic frisbee physics including aerodynamic forces, spin dynamics, and trajectory visualization.
/// 
/// This class implements aerodynamic equations based on frisbee physics research to accurately simulate
/// lift, drag, and gravity forces acting on a thrown frisbee and enables its flight trajectory to be visualized
/// 
/// Physics Reference: https://web.mit.edu/womens-ult/www/smite/frisbee_physics.pdf
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(FrisbeeTrajectory))]
public class ThrowFrisbee : MonoBehaviour
{
    /// <summary>Throw Settings - Controls initial velocity and spin properties</summary>
    
    /// <summary>Initial horizontal throw velocity in meters per second (standard frisbee: ~14 m/s).</summary>
    [Header("Throw Settings")]
    [SerializeField]
    private float throwForce = 14f;
    
    /// <summary>Initial vertical velocity component in meters per second.</summary>
    [SerializeField]
    private float upwardBias = 0f;
    
    /// <summary>Angular velocity of the frisbee spin in radians per second.</summary>
    [SerializeField]
    private float spinSpeed = 50f;
    
    /// <summary>Initial angle of attack in degrees - determines lift and drag characteristics.</summary>
    [SerializeField]
    private float angleOfAttack = 10f;

    /// <summary>Physical Properties - Standard measurements for a competition frisbee</summary>
    
    /// <summary>Frisbee mass in kilograms (standard: 0.175 kg).</summary>
    [Header("Physical Properties")]
    [SerializeField]
    private float mass = 0.175f;
    
    /// <summary>Frisbee cross-sectional area in square meters (standard: 0.0568 m²).</summary>
    [SerializeField]
    private float area = 0.0568f;
    
    /// <summary>Air density at sea level in kg/m³ (standard: 1.23 kg/m³).</summary>
    [SerializeField]
    private float airDensity = 1.23f;

    /// <summary>Aerodynamic Coefficients - From Morrison 2005 research paper on frisbee aerodynamics</summary>
    
    /// <summary>Lift coefficient at zero angle of attack (α=0).</summary>
    [Header("Aerodynamic Coefficients (from Morrison 2005)")]
    [SerializeField]
    private float cl0 = 0.1f;
    
    /// <summary>Lift coefficient rate of change with respect to angle of attack.</summary>
    [SerializeField]
    private float cLa = 1.4f;
    
    /// <summary>Drag coefficient at zero angle of attack (α=0).</summary>
    [SerializeField]
    private float cd0 = 0.08f;
    
    /// <summary>Drag coefficient rate of change with respect to angle of attack squared.</summary>
    [SerializeField]
    private float cDa = 2.72f;
    
    /// <summary>Reference angle of attack for drag calculations in degrees.</summary>
    [SerializeField]
    private float alpha0 = -4f;

    /// <summary>Whether the frisbee has been thrown and is currently in motion.</summary>
    private bool _wasThrown = false;
    
    /// <summary>Reference to the rigidbody component for physics calculations.</summary>
    private Rigidbody _rigidbody;
    
    /// <summary>Initial position of the frisbee before being thrown (for reset functionality).</summary>
    private Vector3 _initialPosition;
    
    /// <summary>Initial rotation of the frisbee before being thrown (for reset functionality).</summary>
    private Quaternion _initialRotation;
        
    /// <summary>Current angle of attack in radians - updated during flight based on disc orientation.</summary>
    private float _currentAlpha;

    /// <summary>Original parent transform to reattach the frisbee after reset.</summary>
    private Transform _originalParent;

    /// <summary>Reference to the trajectory visualization component.</summary>
    private FrisbeeTrajectory _trajectoryLine;
    
    /// <summary>Reference to the collider component for enabling/disabling collisions.</summary>
    private Collider _collider;

    /// <summary>
    /// Initializes the frisbee by setting up the rigidbody, initial position, and trajectory line renderer.
    /// </summary>
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _trajectoryLine = GetComponent<FrisbeeTrajectory>();

        _rigidbody.mass = mass;
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;

        _originalParent = transform.parent;

        PlayerHoldingFrisbee();
    }


    /// <summary>
    /// Updates the trajectory visualization or handles throw input each frame.
    /// Detects when the player triggers the throw input and initiates the throw.
    /// </summary>
    private void Update()
    {
        if (!_wasThrown && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            Throw();
        }
    }

    /// <summary>
    /// Applies aerodynamic forces and gravity to the frisbee during flight physics updates.
    /// Called once per physics frame (FixedUpdate).
    /// </summary>
    private void FixedUpdate()
    {
        if (_wasThrown)
        {
            ApplyAerodynamicForces();
        }
    }

    /// <summary>
    /// Initiates the frisbee throw by applying initial velocity and spin, enabling physics simulation.
    /// Sets the trajectory visualization to active.
    /// </summary>
    private void Throw()
    {
        _wasThrown = true;

        // Detach the frisbee from its hand placeholder to allow it not move when the hand moves
        transform.parent = null;

        _rigidbody.isKinematic = false;
        // Gravity will be applied manually via aerodynamic forces
        _rigidbody.useGravity = false; 

        _collider.enabled = true;

        // Set initial angle of attack
        _currentAlpha = angleOfAttack * Mathf.Deg2Rad;

        // The frisbee's forward direction is its local X-axis (red arrow)
        // which corresponds to transform.right
        Vector3 throwDirection = transform.right;
        throwDirection.y = 0; // Remove vertical component
        throwDirection = throwDirection.normalized;

        // Apply throw force directly in the throw direction
        _rigidbody.linearVelocity = throwDirection * throwForce + Vector3.up * upwardBias;

        // Apply spin around the world up axis (vertical spin)
        _rigidbody.angularVelocity = Vector3.up * spinSpeed;

        _trajectoryLine.enabled = true;   
    }

    /// <summary>
    /// Applies aerodynamic forces (lift, drag) and gravity to the frisbee during flight.
    /// Called every physics frame to update the frisbee's velocity.
    /// </summary>
    private void ApplyAerodynamicForces()
    {
        Vector3 velocity = _rigidbody.linearVelocity;
        float vMagnitude = velocity.magnitude;

        const float STOP_THRESHOLD = 0.1f;

        if (vMagnitude < STOP_THRESHOLD)
        {   
            FrisbeeStopped();
    
            return;
        }

        // Calculate lift coefficient: CL = CL0 + CLa * α
        float cl = cl0 + cLa * _currentAlpha;

        // Calculate drag coefficient: CD = CD0 + CDa * (α - α0)²
        float alphaDiff = _currentAlpha - (alpha0 * Mathf.Deg2Rad);
        float cd = cd0 + cDa * alphaDiff * alphaDiff;

        // Calculate lift force: FL = 0.5 * ρ * v² * A * CL
        // Lift is perpendicular to velocity, in the upward direction
        // For a disc, lift acts upward relative to the disc's orientation
        float liftMagnitude = 0.5f * airDensity * vMagnitude * vMagnitude * area * cl;

        // Get the disc's up vector (which determines lift direction)
        Vector3 discUp = transform.up;

        // Project disc up onto the plane perpendicular to velocity to get lift direction
        Vector3 liftDirection = (discUp - Vector3.Dot(discUp, velocity.normalized) * velocity.normalized).normalized;
        Vector3 liftForce = liftDirection * liftMagnitude;

        // Calculate drag force: FD = 0.5 * ρ * v² * A * CD
        // Drag opposes the velocity
        float dragMagnitude = 0.5f * airDensity * vMagnitude * vMagnitude * area * cd;
        Vector3 dragForce = -velocity.normalized * dragMagnitude;

        // Calculate gravity force: Fg = m * g
        Vector3 gravityForce = Vector3.up * (Physics.gravity.y * mass);

        // Apply all forces
        _rigidbody.AddForce(liftForce);
        _rigidbody.AddForce(dragForce);
        _rigidbody.AddForce(gravityForce);

        // Gradually reduce spin (angular drag)
        _rigidbody.angularVelocity *= 0.995f;

        // Update angle of attack based on velocity and disc orientation
        Vector3 horizontalVelocity = new(velocity.x, 0, velocity.z);

        const float HORIZONTAL_VELOCITY_THRESHOLD = 0.1f;

        // Update the angle of attack based on the disc's orientation relative to the world
        // Only update when the frisbee has significant horizontal movement (> 0.1 m/s)
        // to avoid noisy calculations when the frisbee is nearly stationary
        if (horizontalVelocity.magnitude > HORIZONTAL_VELOCITY_THRESHOLD)
        {
            // Calculate the angle between the disc's up vector and the world's up vector (Y-axis)
            // This angle determines how tilted the frisbee is during flight
            float angle = Vector3.Angle(discUp, Vector3.up);
            
            // Convert the angle to the angle of attack (alpha):
            // When disc is horizontal: angle = 90°, so alpha = 0° (no tilt)
            // When disc is vertical: angle = 0°, so alpha = 90° (full tilt)
            // Formula: alpha = 90° - angle, then convert to radians
            _currentAlpha = (90f - angle) * Mathf.Deg2Rad;
        }
    }

    /// <summary>
    /// Resets the frisbee to its held state by making it kinematic and disabling physics.
    /// Clears the trajectory visualization.
    /// </summary>
    private void PlayerHoldingFrisbee()
    {
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        _collider.enabled = false;
    }

    /// <summary>
    /// Handles the logic when the frisbee has stopped moving after being thrown.
    /// Resets the frisbee's transform and sets it to the held state.
    /// </summary>
    private void FrisbeeStopped()
    {
        _wasThrown = false;

        ResetTransform();

        _trajectoryLine.enabled = false;

        PlayerHoldingFrisbee();
    }

    /// <summary>
    /// Resets the frisbee's transform to its original parent, position, and rotation.
    /// </summary>
    private void ResetTransform()
    {
        transform.parent = _originalParent;
        transform.SetPositionAndRotation(_initialPosition, _initialRotation);
    }
}