using UnityEngine;
using UnityEngine.AI;

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
 public abstract class DogState: State
{   
    /// <summary>
    /// Speed at which the dog rotates to face its target.
    /// Higher values result in faster rotation.
    /// </summary>
    [SerializeField]
    private float rotationSpeed = 2f;

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

    /// <summary>
    /// Reference to the Frisbee game manager.
    /// Initialized in <see cref="LateStart"/> and provides access to game state and adaptive parameters.
    /// </summary>
    protected FrisbeeGameManager _gameManager;

    /// <summary>
    /// Initializes the dog state by setting up state properties and configuring the NavMeshAgent.
    /// </summary>
    /// <remarks>
    /// Unity lifecycle callback invoked when the script instance is being loaded.
    /// Calls <see cref="State.SetStateProprieties"/> to initialize FSM reference and state name.
    /// Retrieves the NavMeshAgent component and disables automatic rotation (manual rotation is handled by <see cref="RotateDogTowardsTarget"/>).
    /// Derived classes should call base.Awake() when overriding this method.
    /// </remarks>
    protected virtual void Awake()
    {   
        SetStateProprieties();

        _agent = GetComponent<NavMeshAgent>();

        _agent.updateRotation = false;
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
        }

        _gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<FrisbeeGameManager>();

        if (_gameManager == null)
        {
            Debug.LogError("GameManager game obejct not found or FrisbeeGameManager component.");
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

    /// <summary>
    /// Smoothly rotates the dog to face the specified target transform.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Calculates the horizontal direction to the target (Y component is zeroed to prevent tilting)
    /// and smoothly interpolates the dog's rotation using Slerp.
    /// </para>
    /// <para>
    /// The direction is inverted before creating the rotation to ensure the dog faces the target correctly.
    /// This method should be called in the Execute method of states that require the dog to face a specific target while moving.
    /// </para>
    /// </remarks>
    /// <param name="targetTransform">The transform of the target to face. Must not be null.</param>
    protected void RotateDogTowardsTarget(Transform targetTransform)
    {
        Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
        directionToTarget.y = 0; 

        if (directionToTarget != Vector3.zero)
        {   
            // The direction is inverted to make the dog face the target correctly
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}