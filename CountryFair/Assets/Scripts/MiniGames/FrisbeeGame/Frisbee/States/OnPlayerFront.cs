using UnityEngine;

/// <summary>
/// State representing the frisbee being held in the player's hand before throwing.
/// Manages the ready-to-throw state, including throw parameters, input detection, material transparency effects,
/// and resetting the frisbee to its held position when returned by the dog.
/// </summary>
/// <remarks>
/// <para>
/// This state handles the complete lifecycle of the frisbee while held:
/// <list type="bullet">
/// <item><description>Initializes throw parameters (velocity, spin, angle of attack)</description></item>
/// <item><description>Manages visual feedback through material transparency (can/cannot throw)</description></item>
/// <item><description>Detects player input (PrimaryIndexTrigger) to initiate throws</description></item>
/// <item><description>Applies physics on throw: velocity, spin, and aerodynamic initialization</description></item>
/// <item><description>Resets frisbee to hand when returned by the dog</description></item>
/// </list>
/// </para>
/// <para>
/// The frisbee's visual opacity changes based on whether it can be thrown (full opacity when ready, reduced when not ready).
/// This provides clear visual feedback to the player about the game state.
/// </para>
/// </remarks>
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(TrackFrisbeeThrow))]
public class OnPlayerFront : FrisbeeState
{   
    /// <summary>
    /// Initial horizontal throw velocity in meters per second.
    /// Standard frisbee throws typically range from 10-20 m/s.
    /// </summary>
    [Header("Throw Settings")]
    [SerializeField]
    private float throwForce = 14f;
    
    /// <summary>
    /// Initial vertical velocity component in meters per second.
    /// Adds upward bias to the throw trajectory, affecting the arc height.
    /// </summary>
    [SerializeField]
    private float upwardBias = 3f;
    
    /// <summary>
    /// Angular velocity of the frisbee spin in radians per second.
    /// Spin stabilizes the frisbee in flight and affects lift generation.
    /// </summary>
    [SerializeField]
    private float spinSpeed = 50f;
    
    /// <summary>
    /// Initial angle of attack in degrees.
    /// Determines the frisbee's pitch relative to the velocity vector, affecting lift and drag characteristics.
    /// Positive values pitch the nose up, negative values pitch it down.
    /// </summary>
    [SerializeField]
    private float angleOfAttack = 10f;


    /// <summary>
    /// Alpha (opacity) value for the frisbee materials when the player cannot throw.
    /// Lower values make the frisbee more transparent, providing visual feedback that throwing is disabled.
    /// </summary>
    [Header("Can't Throw Frisbee Settings")]
    [SerializeField]
    private float cannotThrowAlpha = 0.5f;

    /// <summary>
    /// Original parent transform of the frisbee (typically the player's hand).
    /// Used to reattach the frisbee when resetting to the held position.
    /// </summary>
    private Transform _originalParent;

    /// <summary>
    /// Initial local position of the frisbee relative to the player's hand.
    /// Stored for reset functionality when the frisbee is returned.
    /// </summary>
    private Vector3 _initialPosition;

    /// <summary>
    /// Initial local rotation of the frisbee relative to the player's hand.
    /// Stored for reset functionality when the frisbee is returned.
    /// </summary>
    private Quaternion  _initialRotation;

    /// <summary>
    /// Flag indicating whether the dog has reached its target position.
    /// Set to true when the dog reaches its target position via <see cref="DogReachedTarget"/>.
    /// Its set to true becuase the dog starts in the target position at game start.
    /// </summary>
    private bool _dogInTarget = true;

    /// <summary>
    /// Array of materials from the frisbee's Renderer component.
    /// Used to modify opacity based on throw availability.
    /// </summary>
    private Material[] materials;

    private TrackFrisbeeThrow _trackFrisbeeThrow;

    /// <summary>
    /// Initializes the OnPlayerHand state by storing initial transform values and configuring material transparency.
    /// </summary>
    /// <remarks>
    /// Unity lifecycle callback invoked when the script instance is being loaded.
    /// Stores the original parent transform, position, and rotation for later reset operations,
    /// and configures the frisbee's materials to support transparency effects for visual feedback.
    /// </remarks>
    protected override void Awake()
    {  
        base.Awake();
        _originalParent = transform.parent;

        _initialPosition = transform.position;
        _initialRotation = transform.rotation;

        _trackFrisbeeThrow = GetComponent<TrackFrisbeeThrow>();
        SetUpMaterialsTransparency();
    }

/// <summary>
/// Initiates the frisbee throw by configuring physics and applying initial forces.
/// Uses the tracked hand movement vector from TrackFrisbeeThrow component.
/// </summary>
/// <remarks>
/// <para>
/// The throw sequence:
/// <list type="number">
/// <item><description>Disables further throwing until the dog is ready again</description></item>
/// <item><description>Detaches the frisbee from the player's hand (sets parent to null)</description></item>
/// <item><description>Enables physics simulation (non-kinematic, custom gravity via aerodynamics)</description></item>
/// <item><description>Enables the collider for interaction with the environment</description></item>
/// <item><description>Sets the initial angle of attack for aerodynamic calculations</description></item>
/// <item><description>Calculates throw direction from hand tracking data (velocity + rotation)</description></item>
/// <item><description>Applies linear velocity in the tracked throw direction</description></item>
/// <item><description>Applies angular velocity (spin around vertical axis)</description></item>
/// <item><description>Enables trajectory visualization line</description></item>
/// </list>
/// </para>
/// </remarks>
private void ThrowFrisbee()
{   
    // Because the dog will go catch the frisbee.
    _dogInTarget = false;
   
    // Detach the frisbee from its hand placeholder to allow it not move when the hand moves
    transform.parent = null;

    _rigidbody.isKinematic = false;
    // Gravity will be applied manually via aerodynamic forces
    _rigidbody.useGravity = false; 

    _collider.isTrigger = false;

    // Set initial angle of attack
    _currentAlpha = angleOfAttack * Mathf.Deg2Rad;

    // Get throw direction from tracking component based on which hand threw
    // The tracking component calculates this from hand velocity and rotation
    Vector3 throwDirection;
    float throwSpeed;
    
    if (_trackFrisbeeThrow.RightThrowSpeed > _trackFrisbeeThrow.LeftThrowSpeed)
    {
        // Right hand throw
        throwDirection = _trackFrisbeeThrow.RightThrowVector;
        throwSpeed = _trackFrisbeeThrow.RightThrowSpeed;
    }
    else
    {
        // Left hand throw
        throwDirection = _trackFrisbeeThrow.LeftThrowVector;
        throwSpeed = _trackFrisbeeThrow.LeftThrowSpeed;
    }

    // Ensure the throw direction is normalized
    throwDirection = throwDirection.normalized;

    // Apply throw force using the tracked direction and speed
    // Use the tracked speed as a multiplier for more realistic throws
    float finalThrowForce = throwForce * Mathf.Clamp(throwSpeed / 3f, 0.5f, 2f); // Scale based on hand speed
    _rigidbody.linearVelocity = throwDirection * finalThrowForce + Vector3.up * upwardBias;


    Debug.Log($"Throw Direction: {throwDirection}, Throw Speed: {finalThrowForce} m/s");

    // Apply spin around the world up axis (vertical spin)
    _rigidbody.angularVelocity = Vector3.up * spinSpeed;

    _trajectoryLine.enabled = true; 
}

    
    /// <summary>
    /// Resets the frisbee to its held state in the player's hand.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Restoration sequence:
    /// <list type="bullet">
    /// <item><description>Resets transform to original parent, position, and rotation</description></item>
    /// <item><description>Ensures the GameObject is active (may have been deactivated by dog catch state)</description></item>
    /// <item><description>Makes the Rigidbody kinematic to disable physics simulation</description></item>
    /// <item><description>Disables gravity (will be handled manually when thrown)</description></item>
    /// <item><description>Zeros out all velocities</description></item>
    /// <item><description>Disables the collider to prevent interactions while held</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private void PlayerHoldingFrisbee()
    {    
        ResetTransform();
        
        // In the dog state to catch the frisbee, the frisbee visibility is turned off, so we need to enable it here
        gameObject.SetActive(true);

        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        _collider.isTrigger = true;
    }

    /// <summary>
    /// Changes the opacity of all frisbee materials based on whether the player can throw.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <see cref="_dogInTarget"/> is true:
    /// <list type="bullet">
    /// <item><description>Alpha is set to 1.0 (fully opaque)</description></item>
    /// <item><description>Render queue is set to 2000 (opaque rendering)</description></item>
    /// <item><description>ZWrite is enabled for proper depth rendering</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// When <see cref="_dogInTarget"/> is false:
    /// <list type="bullet">
    /// <item><description>Alpha is set to <see cref="cannotThrowAlpha"/> (semi-transparent)</description></item>
    /// <item><description>Render queue is set to 3000 (transparent rendering)</description></item>
    /// <item><description>ZWrite is disabled for proper transparency sorting</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This provides clear visual feedback to the player about whether they can throw the frisbee.
    /// </para>
    /// </remarks>
    private void ChangeMaterialsOpacity()
    {    
        float alphaToChange;

        int renderQueue, zWriteValue;
        if (_dogInTarget)
        {   
            const float MAX_ALPHA = 1f;

            alphaToChange = MAX_ALPHA;
            renderQueue = 2000;
            zWriteValue = 1;
        }
        else
        {
            
            alphaToChange = cannotThrowAlpha;
            renderQueue = 3000;
            zWriteValue = 0;
        }

        foreach (Material mat in materials)
        {
            Color color = mat.color;
            color.a = alphaToChange;
            mat.color = color;

            mat.SetInt("_ZWrite", zWriteValue);
            mat.renderQueue = renderQueue;
        }
    }

    /// <summary>
    /// Configures all frisbee materials to support transparency effects.
    /// </summary>
    /// <remarks>
    /// Sets up URP/Lit shader properties for alpha blending:
    /// <list type="bullet">
    /// <item><description>Sets surface type to Transparent</description></item>
    /// <item><description>Configures alpha blend mode</description></item>
    /// <item><description>Sets up blend modes (SrcAlpha, OneMinusSrcAlpha)</description></item>
    /// <item><description>Configures render queue for transparent rendering</description></item>
    /// <item><description>Enables appropriate shader keywords</description></item>
    /// </list>
    /// This is called once during initialization to prepare materials for runtime opacity changes.
    /// </remarks>
    private void SetUpMaterialsTransparency()
    {   
        materials = GetComponent<Renderer>().materials;  

        
        foreach (Material mat in materials)
        {
            // URP/Lit shader properties
            mat.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
            mat.SetFloat("_Blend", 0); // 0 = Alpha, 1 = Premultiply, 2 = Additive, 3 = Multiply
            
            // Configurações de renderização
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.SetInt("_AlphaClip", 0);
            
            // Render queue para transparent
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            
            // Keywords do URP
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_OFF");
        }
    }
  
    /// <summary>
    /// Resets the frisbee's transform hierarchy to its original held configuration.
    /// </summary>
    /// <remarks>
    /// Reattaches the frisbee to its original parent (player's hand) and restores
    /// the initial position and rotation stored during <see cref="Awake"/>.
    /// </remarks>
    private void ResetTransform()
    {
        transform.parent = _originalParent;
        transform.SetPositionAndRotation(_initialPosition, _initialRotation);
    }

    /// <summary>
    /// Called when entering the OnPlayerHand state.
    /// Resets the frisbee to its held position and configures it for being held.
    /// </summary>
    /// <remarks>
    /// Updates the material opacity based on <see cref="_dogInTarget"/> status and calls
    /// <see cref="PlayerHoldingFrisbee"/> to reset physics and transform.
    /// This state is entered when the dog returns the frisbee to the player or at game start.
    /// </remarks>
    public override void Enter()
   {
        base.Enter();

        _trackFrisbeeThrow.StartTracking();

        ChangeMaterialsOpacity();

        PlayerHoldingFrisbee();
   }

    /// <summary>
    /// Called every frame while in the OnPlayerHand state.
    /// Monitors for throw input and initiates the throw sequence when conditions are met.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Detects when the player presses the PrimaryIndexTrigger (VR controller trigger).
    /// If <see cref="_dogInTarget"/> is true, initiates the throw by:
    /// <list type="number">
    /// <item><description>Calling <see cref="ThrowFrisbee"/> to apply physics</description></item>
    /// <item><description>Triggering the "FrisbeeThrown" transition to the <see cref="OnMovement"/> state</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If the dog hasn't reached its position yet, the throw is blocked (visual feedback via transparency).
    /// </para>
    /// </remarks>
    public override void Execute()
    {
         base.Execute();
    }

    /// <summary>
    /// Called when exiting the OnPlayerHand state.
    /// </summary>
    /// <remarks>
    /// Performs base cleanup for state exit. The throw has already been initiated in <see cref="Execute"/>.
    /// </remarks>
    public override void Exit()
    {   
        base.Exit();
    }

    /// <summary>
    /// Called externally when the dog reaches its target position and is ready to catch.
    /// Enables throwing and updates the visual feedback, making the frisbee fully opaque.
    /// </summary>
    /// <remarks>
    /// This method should be called by the dog's idle state (via UnityEvent) to signal that
    /// the game is ready for the next throw. Sets <see cref="_dogInTarget"/> to true and updates
    /// material opacity to full visibility, indicating to the player that throwing is now allowed.
    /// </remarks>
    public void DogReachedTarget()
    {
        _dogInTarget = true;
        ChangeMaterialsOpacity();
    }

    public void ThrowGestureTriggered()
    {   
        if (fSM.CurrentState == this)
        {   
            _trackFrisbeeThrow.CalculateLeftThrow();
            _trackFrisbeeThrow.CalculateRightThrow();
            
            ThrowFrisbee();  
            _trackFrisbeeThrow.StopTracking();
            fSM.ChangeState("FrisbeeThrown");

            return;
        }
    }
}