using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// State representing the frisbee in flight after being thrown.
/// Simulates realistic frisbee physics including aerodynamic forces (lift and drag), spin dynamics,
/// and gravity. Monitors for ground contact and score area triggers to determine when to transition
/// to the landed state.
/// 
/// Physics Reference: https://web.mit.edu/womens-ult/www/smite/frisbee_physics.pdf
/// Aerodynamic Coefficients from Morrison 2005 research paper on frisbee aerodynamics.
/// </summary>
public class OnMovement : FrisbeeState
{       
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

    /// <summary>Whether the frisbee has touched the ground during flight.</summary>
    private bool _touchedGround = false;

    /// <summary>Whether the frisbee is currently within the score area trigger zone.</summary>
    private bool _isOnScoreArea = false;

    /// <summary>Event invoked when the player successfully scores by landing the frisbee in the score area.
    /// </summary>
    /// <remarks>
    /// This event is trigger in the <see cref="GiveFrisbeeToPlayer"/> state to alert the dog that the player has scored,
    /// and it must choose a new position accordingly.
    /// </remarks>
    public UnityEvent playerScored;

    /// <summary>
    /// Initializes the state by setting up physics component references.
    /// </summary>
    protected override void Awake()
    {  
        base.Awake();
    }

    /// <summary>
    /// Called when entering the OnAir state (when frisbee is thrown).
    /// </summary>
    public override void Enter()
   {
        base.Enter();
   }

    /// <summary>
    /// Called every physics frame while in the OnAir state.
    /// Applies aerodynamic forces (lift, drag) and gravity to simulate realistic frisbee flight physics.
    /// </summary>
    public override void Execute()
    {
        base.Execute();

        ApplyAerodynamicForces();
    }

    /// <summary>
    /// Called when exiting the OnAir state (when frisbee lands).
    /// </summary>
    public override void Exit()
    {   
        base.Exit();
    }

    /// <summary>
    /// Applies aerodynamic forces (lift, drag) and gravity to the frisbee during flight.
    /// Called every physics frame to update the frisbee's velocity based on aerodynamic equations.
    /// When velocity drops below threshold and frisbee has touched ground (frisbee landed), triggers
    /// the transition "StoppedOnGround" to the <see cref="Landed"/> state.
    /// Invokes playerScored event if the frisbee stopped within the score area.
    /// </summary>
    private void ApplyAerodynamicForces()
    {   
        Vector3 velocity = _rigidbody.linearVelocity;
        float vMagnitude = velocity.magnitude;

        const float STOP_THRESHOLD = 0.1f;

        if (vMagnitude < STOP_THRESHOLD)
        {   
            if (_touchedGround)
            {   
                if (_isOnScoreArea)
                {
                    playerScored.Invoke();
                }

                fSM.ChangeState("StoppedOnGround");
            }

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
    /// Detects when the frisbee collides with the ground.
    /// Sets the _touchedGround flag to enable state transition when velocity reaches threshold.
    /// </summary>
    /// <param name="other">The collision data associated with this collision event.</param>
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground")) 
        {
            _touchedGround = true;
        }
    }

    /// <summary>
    /// Detects when the frisbee leaves contact with the ground.
    /// Clears the _touchedGround flag if the frisbee bounces back up.
    /// </summary>
    /// <param name="other">The collision data associated with this collision exit event.</param>
    private void OnCollisionExit(Collision other)
    {
         if (other.gameObject.CompareTag("Ground")) 
        {
           _touchedGround = false;
        }
    }

    /// <summary>
    /// Detects when the frisbee enters the score area trigger zone.
    /// Sets the _isOnScoreArea flag to track potential scoring.
    /// </summary>
    /// <param name="other">The collider that this object has entered.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("ScoreArea")) 
        {
            _isOnScoreArea = true;
        }
    }

    /// <summary>
    /// Detects when the frisbee exits the score area trigger zone.
    /// Clears the _isOnScoreArea flag if the frisbee leaves the scoring zone.
    /// </summary>
    /// <param name="other">The collider that this object has exited.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("ScoreArea")) 
        {
           _isOnScoreArea = false;
        }
    }
}