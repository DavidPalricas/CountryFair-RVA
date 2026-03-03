using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using DG.Tweening;

/// <summary>
/// Abstract base class for all dog behavior states in the Frisbee mini-game.
/// Extends <see cref="State"/> and provides common functionality for navigation, rotation, and game references.
/// </summary>
/// <remarks>
/// This class requires a NavMeshAgent component for AI navigation.
/// All dog states share common references to the player transform, game manager, and navigation agent.
/// Derived classes should override state lifecycle methods (Enter, Execute, Exit) to implement specific behaviors.
/// </remarks>
[RequireComponent(typeof(NavMeshAgent))]
 public abstract class DogState: AnimatableState
{   
    /// <summary>
    /// Speed at which the dog rotates to face its target.
    /// Higher values result in faster rotation.
    /// </summary>
    [SerializeField]
    private float rotationSpeed = 2f;

    [SerializeField]
    protected UnityEvent<AudioManager.GameSoundEffects, GameObject> barkEvent;

    /// <summary>
    /// Reference to the NavMeshAgent component used for AI navigation and pathfinding.
    /// Automatically retrieved in <see cref="Awake"/> and configured to disable automatic rotation.
    /// </summary>
    protected NavMeshAgent _agent;

    /// <summary>
    /// Reference to the player's transform component.
    /// Initialized in <see cref="LateStart"/> and used for navigation and positioning relative to the player.
    /// </summary>
    protected Transform _playerTransform;

    protected static Vector3 _currentTargetPos;

    private Coroutine _rotateCoroutine;


    private readonly AudioManager.GameSoundEffects _barkSoundEffect = AudioManager.GameSoundEffects.DOG_BARK;

    /// <summary>
    /// Initializes the dog state by setting up state properties and configuring the NavMeshAgent.
    /// </summary>
    /// <remarks>
    /// Unity lifecycle callback invoked when the script instance is being loaded.
    /// Calls <see cref="State.SetStateProprieties"/> to initialize FSM reference and state name.
    /// Retrieves the NavMeshAgent component and disables automatic rotation (manual rotation is handled by <see cref="RotateDogTowardsTarget"/>).
    /// Derived classes should call base.Awake() when overriding this method.
    /// </remarks>
    protected override void Awake()
    {    
        base.Awake();

        _agent = GetComponent<NavMeshAgent>();

        _agent.updateRotation = false;
    }


    public override void Enter()
    {
        base.Enter();
    }

    public override void Execute()
    {
        base.Execute();
    }

    public override void Exit()
    {
        base.Exit();
    }

    /// <summary>
    /// Initializes references to the player and game manager by finding them in the scene.
    /// Called once during FSM initialization for all states.
    /// </summary>
    /// <remarks>
    /// Searches for GameObjects with "Player" and "GameManager" tags and caches their references.
    /// Logs errors if either reference cannot be found.
    /// Derived classes can override this to add additional initialization but should call base.LateStart() to ensure these references are set.
    /// </remarks>
    public override void LateStart()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (_playerTransform == null)
        {
            Debug.LogError("Player GameObject not found in the scene.");

            return;
        }
    }

    /// <summary>
    /// Checks whether the dog has stopped moving and reached its destination.
    /// </summary>
    /// <remarks>
    /// Returns true when both conditions are met:
    /// <list type="bullet">
    /// <item><description>The path is fully calculated (not pending)</description></item>
    /// <item><description>The remaining distance to the destination is within the stopping distance threshold</description></item>
    /// </list>
    /// This method is commonly used in Execute methods to detect when the dog has completed navigation to trigger state transitions.
    /// </remarks>
    /// <returns>True if the dog has stopped at its destination; otherwise, false.</returns>
    protected bool DogStoped()
    {
        return !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance;
    }



    protected void RotateDogTowardsTarget(Transform targetTransform)
    {   
        Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
        directionToTarget.y = 0;

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            
            transform.DOKill(); 
            transform.DORotateQuaternion(targetRotation, 0.5f).SetEase(Ease.InOutSine);
        }
    }



    protected void Bark()
    {
         barkEvent.Invoke(_barkSoundEffect, gameObject);
    }
}