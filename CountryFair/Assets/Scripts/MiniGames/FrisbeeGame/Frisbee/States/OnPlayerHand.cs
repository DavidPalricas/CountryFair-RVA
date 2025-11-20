using UnityEngine;

/// <summary>
/// State representing the frisbee being held by the player before throwing.
/// Handles the initialization of throw parameters, input detection for throwing, and resetting the frisbee
/// to its held position when returned by the dog.
/// </summary>
public class OnPlayerHand : FrisbeeState
{   
    /// <summary>Throw Settings - Controls initial velocity and spin properties</summary>
    
    /// <summary>Initial horizontal throw velocity in meters per second (standard frisbee: ~14 m/s).</summary>
    [Header("Throw Settings")]
    [SerializeField]
    private float throwForce = 14f;
    
    /// <summary>Initial vertical velocity component in meters per second.</summary>
    [SerializeField]
    private float upwardBias = 3f;
    
    /// <summary>Angular velocity of the frisbee spin in radians per second.</summary>
    [SerializeField]
    private float spinSpeed = 50f;
    
    /// <summary>Initial angle of attack in degrees - determines lift and drag characteristics.</summary>
    [SerializeField]
    private float angleOfAttack = 10f;

    /// <summary>Original parent transform to reattach the frisbee after reset.</summary>
    private Transform _originalParent;

    /// <summary>Initial position of the frisbee before being thrown (for reset functionality).</summary>
    private Vector3 _initialPosition;

    /// <summary>Initial rotation of the frisbee before being thrown (for reset functionality).</summary>
    private Quaternion  _initialRotation;

    /// <summary>
    /// Initializes the state by storing the original parent transform , rotation and position for reset functionality.
    /// </summary>
    protected override void Awake()
    {  
        base.Awake();
        _originalParent = transform.parent;

        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    /// <summary>
    /// Initiates the frisbee throw by applying initial velocity and spin, enabling physics simulation.
    /// Detaches the frisbee from the player's hand and transitions it to flight mode.
    /// Sets the trajectory visualization to active.
    /// </summary>
    private void ThrowFrisbee()
    {
        
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
    /// Resets the frisbee to its held state by making it kinematic and disabling physics.
    /// Clears the trajectory visualization.
    /// </summary>
    private void PlayerHoldingFrisbee()
    {    
        ResetTransform();
        
        // In the dog state to catch the frisbee, the frisbee visibility is turned off, so we need to enable it here
        gameObject.SetActive(true);

        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        _collider.enabled = false;
    }
  
    /// <summary>
    /// Resets the frisbee's transform to its original parent, position, and rotation.
    /// </summary>
    private void ResetTransform()
    {
        transform.parent = _originalParent;
        transform.SetPositionAndRotation(_initialPosition, _initialRotation);
    }

    /// <summary>
    /// Called when entering the OnPlayerHand state.
    /// Resets the frisbee to its held position and disables physics.
    /// </summary>
    public override void Enter()
   {
        base.Enter();

        PlayerHoldingFrisbee();
   }

    /// <summary>
    /// Called every frame while in the OnPlayerHand state.
    /// Detects when the player triggers the throw input (PrimaryIndexTrigger) and initiates the throw
    /// and triggers the transition "FrisbeeThrown" to the <see cref="OnMovement"/> state.
    /// </summary>
    public override void Execute()
    {
         base.Execute();

         if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            ThrowFrisbee();
            fSM.ChangeState("FrisbeeThrown");
        }
    }

    /// <summary>
    /// Called when exiting the OnPlayerHand state.
    /// </summary>
    public override void Exit()
    {   
        base.Exit();
    }
}