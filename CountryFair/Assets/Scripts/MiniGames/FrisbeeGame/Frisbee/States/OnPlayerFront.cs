using System;
using Oculus.Interaction;
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
public class OnPlayerFront : FrisbeeState
{   
  /// <summary>
    /// Alpha (opacity) value for the frisbee materials when the player cannot throw.
    /// Lower values make the frisbee more transparent, providing visual feedback that throwing is disabled.
    /// </summary>
   
    private float cannotThrowAlpha = 0.5f;


   [SerializeField]
   private Renderer frisbeeRenderer;


   [SerializeField]
   private Grabbable frisbeeGrabbable;
    
    /// <summary>
    /// Original parent transform of the frisbee (typically the player's hand).
    /// Used to reattach the frisbee when resetting to the held position.
    /// </summary>
    private Transform _originalParent;

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

        _initialRotation = transform.rotation;


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

        SetUpMaterialsTransparency();
    }


    /// <summary>
    /// Initiates the frisbee throw by configuring physics and applying initial forces.
    /// Uses the tracked hand movement vector from TrackFrisbeeThrow component.
    /// // Physics Reference: https://web.mit.edu/womens-ult/www/smite/frisbee_physics.pdf
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
    public void ThrowFrisbee()
    {   
        // Detach the frisbee from its hand placeholder to allow it not move when the hand moves
        transform.parent = null;

        _rigidbody.isKinematic = false;

        // Gravity will be applied manually via aerodynamic forces
        _rigidbody.useGravity = true; 

        _collider.isTrigger = false;


        _trajectoryLine.enabled = true; 

        fSM.ChangeState("FrisbeeThrown");
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

        _rigidbody.useGravity = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

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
        materials = frisbeeRenderer.materials;
        
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

        transform.localPosition = Vector3.zero;

        transform.rotation = _initialRotation;
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

    public void DogWillCatchFrsibee()
    {
        _dogInTarget = false;

    }
}