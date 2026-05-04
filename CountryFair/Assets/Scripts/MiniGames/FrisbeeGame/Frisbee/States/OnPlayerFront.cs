using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

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
[RequireComponent(typeof(FollowPlayerHead))]
public class OnPlayerFront : FrisbeeState
{   
    [SerializeField]
   private Grabbable frisbeeGrabbable;
   
   [SerializeField]
    private UnityEvent <AudioManager.GameSoundEffects, GameObject> throwSoundEffectEvent;

    [Header("Rendering Parameters")]
    /// <summary>
    /// Alpha (opacity) value for the frisbee materials when the player cannot throw.
    /// Lower values make the frisbee more transparent, providing visual feedback that throwing is disabled.
    /// </summary>
    
    [SerializeField]  
    private float cannotThrowAlpha = 0.5f;

   [SerializeField]
   private Renderer frisbeeRenderer;

    /// <summary>
    /// Array of materials from the frisbee's Renderer component.
    /// Used to modify opacity based on throw availability.
    /// </summary>
    private Material[] _materials;

    private readonly AudioManager.GameSoundEffects _throwSoundEffect = AudioManager.GameSoundEffects.FRISBEE_THROW;

    private FollowPlayerHead _followPlayerHead;

    /// <summary>
    /// Flag indicating whether the dog has reached its target position.
    /// Set to true when the dog reaches its target position via <see cref="DogReachedTarget"/>.
    /// Its set to true becuase the dog starts in the target position at game start.
    /// </summary>
    /// <value>
    /// True if the dog is in position to catch the frisbee, false otherwise.
    /// </value>
    /// <notes>>
    /// This flag is changed via inspector when the dog will catch the frisbee (see <see cref="CatchFrisbee"/> state) and the dog game object.
    /// </notes>>
    public bool DogInTarget { get; set; } = true;
    
    /// <summary>
    /// Fkag indicating if the frisbee is being grabbed by the right hand.
    /// </summary> <summary>
    /// Set to true if the frisbee is held by the right hand, false if held by the left hand.
    /// </summary>
    /// <value>
    /// True if grabbed by right hand, false otherwise.
    /// </value>
    /// <notes>>
    /// This flag is set externally via inspector by the grab interactables on the frisbee for each hand.
    /// </notes>>
    public bool IsBeingGrabbedByRightHand {get; set;} = false;

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
    
        if (frisbeeRenderer == null)
        {
            Debug.LogError("Component Renderer is not assigned in OnPlayerFront script.");

            return;
        }

        if (frisbeeGrabbable == null)
        {
            Debug.LogError("Component Grabbable is not assigned in OnPlayerFront script.");

            return;
        }

        _followPlayerHead = GetComponent<FollowPlayerHead>();

        SetUpMaterialsTransparency();
    }
 
   

    /// <summary>
    /// Called when entering the OnPlayerHand state.
    /// Resets the frisbee to its held position and configures it for being held.
    /// </summary>
    /// <remarks>
    /// Updates the material opacity based on <see cref="DogInTarget"/> status and calls
    /// <see cref="InPlayerFront"/> to reset physics and transform.
    /// This state is entered when the dog returns the frisbee to the player or at game start.
    /// </remarks>
    public override void Enter()
   {
        base.Enter();

        ChangeMaterialsOpacity();

        InPlayerFront();
   }

    /// <summary>
    /// Called every frame while in the OnPlayerHand state.
    /// Monitors for throw input and initiates the throw sequence when conditions are met.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Detects when the player presses the PrimaryIndexTrigger (VR controller trigger).
    /// If <see cref="DogInTarget"/> is true, initiates the throw by:
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
    /// Initiates the frisbee throw by configuring physics and applying initial forces.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The throw sequence:
    /// <list type="number">
    /// <item><description>Detaches the frisbee from the player's hand (sets parent to null)</description></item>
    /// <item><description>Enables physics simulation (non-kinematic, with gravity)</description></item>
    /// <item><description>Disables trigger mode on collider for physical interactions</description></item>
    /// <item><description>Enables trajectory visualization line</description></item>
    /// <item><description>Transitions to "FrisbeeThrown" state</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Physics Reference: https://web.mit.edu/womens-ult/www/smite/frisbee_physics.pdf
    /// </para>
    /// </remarks>
    public void ThrowFrisbee(bool thrownByRightHand)
    {   
        if (fSM.CurrentState == this && thrownByRightHand == IsBeingGrabbedByRightHand)
        {   
            _followPlayerHead.enabled = false;
            
            _rigidbody.isKinematic = false;

            _rigidbody.useGravity = true;

            _collider.isTrigger = false;

            _trajectoryLine.enabled = true; 

            frisbeeGrabbable.enabled = false;

            throwSoundEffectEvent.Invoke(_throwSoundEffect, gameObject);

            fSM.ChangeState("FrisbeeThrown");
        }
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
    private void InPlayerFront()
    {   
        ResetTransform();

        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        _collider.isTrigger = true;

        frisbeeGrabbable.enabled = true;

        _followPlayerHead.enabled = true;
    }

    /// <summary>
    /// Changes the opacity of all frisbee materials based on whether the player can throw.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <see cref="DogInTarget"/> is true:
    /// <list type="bullet">
    /// <item><description>Alpha is set to 1.0 (fully opaque)</description></item>
    /// <item><description>Render queue is set to 2000 (opaque rendering)</description></item>
    /// <item><description>ZWrite is enabled for proper depth rendering</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// When <see cref="DogInTarget"/> is false:
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

        if (DogInTarget)
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

        foreach (Material mat in _materials)
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
        _materials = frisbeeRenderer.materials;
        
        foreach (Material mat in _materials)
        {
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.SetInt("_AlphaClip", 0);
            
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            
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
        transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Called externally when the dog reaches its target position and is ready to catch.
    /// Enables throwing and updates the visual feedback, making the frisbee fully opaque.
    /// </summary>
    /// <remarks>
    /// This method should be called by the dog's idle state (via UnityEvent) to signal that
    /// the game is ready for the next throw. Sets <see cref="DogInTarget"/> to true and updates
    /// material opacity to full visibility, indicating to the player that throwing is now allowed.
    /// </remarks>
    public void DogReachedTarget()
    {
        DogInTarget = true;
        ChangeMaterialsOpacity();
    }
}
    