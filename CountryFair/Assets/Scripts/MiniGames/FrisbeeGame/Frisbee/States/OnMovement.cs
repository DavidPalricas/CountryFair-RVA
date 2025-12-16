using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// State representing the frisbee during flight, applying realistic aerodynamic forces.
/// Manages physics simulation including lift, drag, angle of attack calculations,
/// and collision detection for landing and out-of-bounds scenarios.
/// </summary>
/// <remarks>
/// <para>
/// This state handles the complete flight simulation:
/// <list type="bullet">
/// <item><description>Applies dynamic lift based on velocity squared and angle of attack</description></item>
/// <item><description>Calculates aerodynamic drag depending on disc orientation</description></item>
/// <item><description>Detects ground collision and adjusts physics accordingly</description></item>
/// <item><description>Monitors out-of-bounds conditions and triggers appropriate state changes</description></item>
/// </list>
/// </para>
/// <para>
/// Aerodynamic behavior:
/// - When flying flat (low angle of attack): minimal drag, stable flight
/// - When tilted (high angle of attack): increased drag acts as parachute effect
/// - Lift is proportional to speed squared, making faster throws glide further
/// </para>
/// </remarks>
public class OnMovement : FrisbeeState
{
    [Header("Aerodynamic Settings")]
    
    /// <summary>
    /// Lift force strength coefficient applied to the frisbee during flight.
    /// Higher values increase upward force, making the disc rise more during flight.
    /// Used in the lift calculation: lift = speed² × liftStrength × aerodynamicFactor
    /// </summary>
    [Tooltip("Lift force strength. Higher values make the disc rise more.")]
    [SerializeField]
    private float liftStrength = 0.15f;
    
    /// <summary>
    /// Base drag coefficient when the frisbee cuts through air in knife-edge orientation.
    /// Represents minimal air resistance during stable, flat flight.
    /// Currently not actively used in the simplified aerodynamic model.
    /// </summary>
    [Tooltip("Drag coefficient when the disc cuts through air (knife-edge orientation).")]
    [SerializeField]
    private float baseDrag = 0.05f;
    
    /// <summary>
    /// High angle drag coefficient when the frisbee falls belly-first (parachute effect).
    /// Creates stronger braking force during descent with high angle of attack.
    /// Currently not actively used in the simplified aerodynamic model.
    /// </summary>
    [Tooltip("Drag coefficient when the disc falls belly-first (parachute effect). Higher values create stronger braking on descent.")]
    [SerializeField]
    private float highAngleDrag = 3.5f;
    
    /// <summary>
    /// Flag indicating whether the frisbee has made contact with the ground.
    /// Used to determine when to transition to the stopped state.
    /// </summary>
    private bool _touchedGround = false;

    /// <summary>
    /// Original angular drag value from the Rigidbody component.
    /// Stored to restore default physics behavior when exiting this state.
    /// </summary>
    private float _defaultAngularDrag;

    /// <summary>
    /// Unity event invoked when the frisbee exits the play area bounds.
    /// Listeners can use this to trigger game logic such as score penalties or UI updates.
    /// </summary>
    public UnityEvent playerMissed;

    /// <summary>
    /// Initializes the OnMovement state by storing default physics values.
    /// </summary>
    /// <remarks>
    /// Unity lifecycle callback invoked when the script instance is being loaded.
    /// Stores the default angular drag value for restoration when exiting flight state.
    /// </remarks>
    protected override void Awake()
    {
        base.Awake();
        _defaultAngularDrag = _rigidbody.angularDamping;
    }

    /// <summary>
    /// Called when entering the OnMovement state.
    /// Configures physics for realistic flight by reducing angular drag.
    /// </summary>
    /// <remarks>
    /// Sets angular damping to a low value (0.1) to ensure the frisbee spins freely in the air,
    /// simulating realistic disc flight behavior with minimal rotational resistance.
    /// </remarks>
    public override void Enter()
    {
        base.Enter();

        _rigidbody.angularDamping = 0.1f;
    }

    /// <summary>
    /// Called every frame while in the OnMovement state.
    /// Continuously applies aerodynamic forces to simulate realistic frisbee flight.
    /// </summary>
    /// <remarks>
    /// Invokes <see cref="ApplySimpleAerodynamics"/> each frame to calculate and apply
    /// lift and drag forces based on current velocity and orientation.
    /// </remarks>
    public override void Execute()
    {
        base.Execute();
        ApplySimpleAerodynamics();
    }

    /// <summary>
    /// Called when exiting the OnMovement state.
    /// Restores default physics values and resets state flags.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Cleanup operations:
    /// <list type="bullet">
    /// <item><description>Restores original angular drag value</description></item>
    /// <item><description>Disables trajectory visualization line</description></item>
    /// <item><description>Resets ground contact flag</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public override void Exit()
    {
        base.Exit();
        _rigidbody.angularDamping = _defaultAngularDrag;

        _trajectoryLine.enabled = false;

        _touchedGround = false;
    }

    /// <summary>
    /// Applies realistic aerodynamic forces to the frisbee during flight.
    /// Calculates lift and drag based on velocity, orientation, and angle of attack.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Physics implementation based on real frisbee aerodynamics:
    /// <list type="number">
    /// <item><description><b>Angle of Attack (AoA):</b> Calculated using dot product between disc's up vector and inverted velocity direction.
    /// <list type="bullet">
    /// <item>AoA = -1: Air hits the top completely (pushes downward)</item>
    /// <item>AoA = 0: Air cuts through disc like a knife (zero lift)</item>
    /// <item>AoA = +1: Air hits the belly completely (maximum lift - parachute effect)</item>
    /// </list>
    /// </description></item>
    /// <item><description><b>Aerodynamic Factor:</b> Ensures lift is only generated when air hits the underside (AoA ≥ 0).
    /// Negative AoA values are clamped to zero to prevent upward lift when disc is inverted.</description></item>
    /// <item><description><b>Dynamic Lift Calculation:</b> Uses velocity squared (speed²) to simulate real aerodynamics:
    /// <list type="bullet">
    /// <item>Slow throws drop quickly (minimal lift)</item>
    /// <item>Fast throws glide far (strong lift)</item>
    /// <item>Multiplied by aerodynamic factor to correct for disc orientation</item>
    /// </list>
    /// </description></item>
    /// <item><description><b>Lift Clamping:</b> Maximum lift force is limited to 15N to prevent unrealistic behavior from VR tracking glitches.</description></item>
    /// <item><description><b>Induced Drag:</b> When generating lift (high AoA), additional drag force is applied opposite to velocity direction.
    /// This prevents infinite gliding and creates realistic energy loss during flight.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Transitions to the landed state ("StoppedOnGround" transition name) when velocity drops below threshold and frisbee has touched ground.
    /// </para>
    /// </remarks>
    private void ApplySimpleAerodynamics()
    {
        Vector3 velocity = _rigidbody.linearVelocity; 
        float speed = velocity.magnitude;
        const float STOP_THRESHOLD = 0.1f;

        if (speed < STOP_THRESHOLD)
        {
            if (_touchedGround) fSM.ChangeState("StoppedOnGround");
            return;
        }

        Vector3 moveDirection = velocity.normalized;

        float angleOfAttack = Vector3.Dot(transform.up, -moveDirection);

        float aerodynamicFactor = Mathf.Max(0, angleOfAttack);

        float dynamicLift = speed * speed * liftStrength * aerodynamicFactor;

        dynamicLift = Mathf.Clamp(dynamicLift, 0, 15f); 

        Vector3 liftForce = transform.up * dynamicLift;

        _rigidbody.AddForce(liftForce);

        Debug.Log("AeorDymanic Details: " +
                 "\n Frisbee Foward: " + transform.forward.ToString("F2") +
                  "\n Speed: " + speed.ToString("F2") +
                  "\n Velocity: " + velocity.ToString("F2") +
                  "\n MoveDir: " + moveDirection.ToString("F2") +
                  "\n Angle of attack: " + angleOfAttack.ToString("F2") +
                  "\n AeroFactor: " + aerodynamicFactor.ToString("F2") +
                  "\n DynamicLift: " + dynamicLift.ToString("F2") + 
                "\n LiftForce: " + liftForce.ToString("F2")
                  );
        
        if (aerodynamicFactor > 0)
        {
            _rigidbody.AddForce(-velocity.normalized * (dynamicLift * 0.1f));
        }
    }

    /// <summary>
    /// Unity collision callback invoked when the frisbee collides with another object.
    /// Handles landing logic by killing rotation and setting ground contact flag.
    /// </summary>
    /// <param name="other">The collision data associated with this collision event.</param>
    /// <remarks>
    /// When colliding with objects tagged "Ground":
    /// <list type="bullet">
    /// <item><description>Sets <see cref="_touchedGround"/> flag to enable state transition when speed drops</description></item>
    /// <item><description>Increases angular damping to 10.0 to rapidly kill spin rotation</description></item>
    /// <item><description>Removes all rigidbody constraints to allow natural settling</description></item>
    /// </list>
    /// Only processes collisions when this state is currently active.
    /// </remarks>
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground") && fSM.CurrentState == this)
        {
            _touchedGround = true;
            _rigidbody.angularDamping = 10.0f;
        }

        _rigidbody.constraints = RigidbodyConstraints.None;
    }

    /// <summary>
    /// Unity collision callback invoked when the frisbee stops colliding with an object.
    /// Handles frisbee bouncing off the ground by resetting flight physics.
    /// </summary>
    /// <param name="other">The collision data associated with this collision exit event.</param>
    /// <remarks>
    /// When leaving collision with objects tagged "Ground":
    /// <list type="bullet">
    /// <item><description>Clears <see cref="_touchedGround"/> flag to prevent premature state transition</description></item>
    /// <item><description>Restores low angular damping (0.1) to allow free spinning during flight</description></item>
    /// </list>
    /// This allows the frisbee to bounce and continue flying if it has sufficient velocity.
    /// Only processes collisions when this state is currently active.
    /// </remarks>
    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground") && fSM.CurrentState == this)
        {
            _touchedGround = false;
            _rigidbody.angularDamping = 0.1f;
        }
    }

    /// <summary>
    /// Unity trigger callback invoked when the frisbee exits a trigger collider.
    /// Handles out-of-bounds detection and triggers appropriate game events.
    /// </summary>
    /// <param name="other">The collider that the frisbee has exited.</param>
    /// <remarks>
    /// When exiting triggers tagged "OutOfBounds":
    /// <list type="bullet">
    /// <item><description>Invokes <see cref="playerMissed"/> event for game logic (score updates, UI feedback)</description></item>
    /// <item><description>Transitions to "FrisbeeOutOfBounds" state for proper cleanup and reset</description></item>
    /// </list>
    /// Only processes trigger exits when this state is currently active.
    /// </remarks>
    private void OnTriggerExit(Collider other)
    {
          if (other.gameObject.CompareTag("OutOfBounds") && fSM.CurrentState == this)
        {   
            playerMissed.Invoke();
            fSM.ChangeState("FrisbeeOutOfBounds");
        }
    }
}