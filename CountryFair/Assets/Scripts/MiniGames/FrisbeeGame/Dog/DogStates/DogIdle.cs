using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

/// <summary>
/// Represents the idle waiting state of the dog in the Frisbee game.
/// The dog remains stationary at its target position, faces the player, and waits for the frisbee to be thrown.
/// </summary>
/// <remarks>
/// This state activates the score area (the catch zone where the frisbee can land) and continuously rotates
/// the dog to face the player. When the frisbee lands in the catch zone, the state transitions to "FrisbeeLanded"
/// which eventually leads to the <see cref="CatchFrisbee"/> state.
/// The current position is saved to the game manager for potential return navigation.
/// </remarks>
public class DogIdle : DogState
{   
    /// <summary>
    /// Reference to the score area GameObject representing the catch zone.
    /// This area is activated when the dog is idle and ready to catch, and deactivated when the dog leaves this state.
    /// </summary>
    /// <remarks>
    /// The score area typically contains a collider or trigger that detects when the frisbee lands within the catchable zone.
    /// </remarks>
    [SerializeField]
    private GameObject scoreArea;

    [Header("Jumps Settings")]
    [SerializeField]
    private float jumpDuration = 1.6f;

    [SerializeField]
    private float jumpPower = 0.3f;
    [SerializeField]
    private int jumpNumbers = 2;

    /// <summary>
    /// Event invoked when the dog reaches its target position and enters the idle state.
    /// Its used to notify the player that the dog is ready for catching the frisbee see <see cref="OnPlayerFront.DogReachedTarget/>.
    /// </summary>
    public UnityEvent positionReached;

    /// <summary>
    /// Initializes the DogIdle state and ensures the score area is initially deactivated.
    /// </summary>
    /// <remarks>
    /// Unity lifecycle callback invoked when the script instance is being loaded.
    /// Calls <see cref="DogState.Awake"/> to set up state properties and NavMeshAgent configuration,
    /// then deactivates the score area so it only appears when the dog is actively in the idle state.
    /// </remarks>
    protected override void Awake()
    {   
        base.Awake();
        
        scoreArea.SetActive(false);
    }

    /// <summary>
    /// Initializes references to the player and game manager.
    /// </summary>
    /// <remarks>
    /// Calls <see cref="DogState.LateStart"/> to set up player transform and game manager references
    /// required for rotation and position tracking.
    /// </remarks>
    public override void LateStart()
    {
        base.LateStart();
    }
    
    /// <summary>
    /// Called when entering the DogIdle state.
    /// Saves the current position, activates the score area, and notifies listeners that the position is reached.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When entering the idle state:
    /// <list type="number">
    /// <item><description>Validates the game manager reference</description></item>
    /// <item><description>Saves the dog's current position to <see cref="FrisbeeGameManager.currentTargetPos"/> for potential return navigation</description></item>
    /// <item><description>Activates the score area to enable frisbee landing detection</description></item>
    /// <item><description>Invokes the <see cref="positionReached"/> event to notify the game that the dog is ready</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public override void Enter()
    {
        base.Enter();

        if (_gameManager == null)
        {
            Debug.LogError("GameManager reference is null in DogIdle state.");
            return;
        }

        _gameManager.currentTargetPos = transform.position;

        scoreArea.SetActive(true);

        positionReached.Invoke();
    }

    /// <summary>
    /// Called every frame while in the DogIdle state.
    /// Continuously rotates the dog to face the player while waiting.
    /// </summary>
    /// <remarks>
    /// This creates the behavior of the dog watching and tracking the player,
    /// maintaining visual engagement while waiting for the frisbee to be thrown.
    /// </remarks>
    public override void Execute()
    {
        base.Execute();

        RotateDogTowardsTarget(_playerTransform);
    }

    /// <summary>
    /// Triggers the state transition when the frisbee lands in the catch zone.
    /// Should be called by external components (e.g., collision detection on the score area) when the frisbee lands.
    /// </summary>
    /// <remarks>
    /// Initiates the transition to the "FrisbeeLanded" state, which will eventually lead to the
    /// <see cref="CatchFrisbee"/> state where the dog navigates to retrieve the frisbee.
    /// </remarks>
    public void FrisbeeLanded()
    {   scoreArea.SetActive(false);

        RotateDogTowardsTarget(_frisbeeTransform);

        transform.DOJump(transform.position, jumpPower, jumpNumbers, jumpDuration).OnComplete( () => 
        {
            fSM.ChangeState("FrisbeeLanded");
        });
    }

    /// <summary>
    /// Called when exiting the DogIdle state.
    /// Deactivates the score area to prevent frisbee detection while the dog is no longer idle.
    /// </summary>
    /// <remarks>
    /// The score area is deactivated because the dog is no longer waiting to catch,
    /// preventing false triggers during other states like movement or returning.
    /// </remarks>
    public override void Exit()
    {
        base.Exit();
    }
}
