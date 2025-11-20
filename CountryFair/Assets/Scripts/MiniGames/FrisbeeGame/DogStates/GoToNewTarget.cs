using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// State where the dog navigates to a randomly selected new target position.
/// The target position is generated within a semicircular area in front of the player at an adaptive distance.
/// </summary>
/// <remarks>
/// This state uses adaptive parameters from the game manager to determine the dog's distance from the player.
/// The target position is randomly chosen within a 180-degree arc in front of the player and validated on the NavMesh.
/// A temporary GameObject is created to represent the target position for rotation purposes.
/// </remarks>
public class GoToNewTarget : DogState{
    /// <summary>
    /// Reference to a temporary GameObject's transform representing the new target position.
    /// Created in <see cref="Enter"/> and destroyed in <see cref="Exit"/>.
    /// </summary>
    private Transform _newTargetTransform;

    /// <summary>
    /// Initializes the GoToNewTarget state by calling the base DogState initialization.
    /// </summary>
    /// <remarks>
    /// Calls <see cref="DogState.Awake"/> to set up state properties and NavMeshAgent configuration.
    /// </remarks>
    protected override void Awake()
    {   
        base.Awake();
    }

    /// <summary>
    /// Initializes references to the player and game manager.
    /// </summary>
    /// <remarks>
    /// Calls <see cref="DogState.LateStart"/> to set up player and game manager references
    /// needed for position calculation and adaptive parameters.
    /// </remarks>
    public override void LateStart()
    {
       base.LateStart();
    }

    /// <summary>
    /// Calculates a new random target position for the dog within a semicircular area in front of the player.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The algorithm:
    /// <list type="number">
    /// <item><description>Retrieves the adaptive "DogDistance" parameter from the game manager</description></item>
    /// <item><description>Generates a random angle between -90° and +90° relative to the player's forward direction</description></item>
    /// <item><description>Calculates a position at the specified distance in the random direction</description></item>
    /// <item><description>Validates the position is on the NavMesh using NavMesh.SamplePosition</description></item>
    /// <item><description>If invalid, recursively calls itself until a valid position is found</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This ensures the dog always positions itself in a walkable area in front of the player.
    /// </para>
    /// </remarks>
    /// <returns>A valid Vector3 position on the NavMesh within the semicircular area.</returns>
    private Vector3 ChooseNewTargetPos()
    {
        float dogDistance = _gameManager.AdaptiveParameters["DogDistance"];
        
        const float MIN_ANGLE = -90f * Mathf.Deg2Rad;
        const float MAX_ANGLE = 90f * Mathf.Deg2Rad;

        float randomAngle = Utils.RandomValueInRange(MIN_ANGLE, MAX_ANGLE);
      
       Quaternion rotation = Quaternion.Euler(0, randomAngle * Mathf.Rad2Deg, 0);

       Vector3 randomDirection = rotation * _playerTransform.forward;
    
       Vector3 targetPosition = _playerTransform.position + randomDirection * dogDistance;

        const  float NAVMESH_SAMPLE_RADIUS = 200f;

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, NAVMESH_SAMPLE_RADIUS, NavMesh.AllAreas))
        {   
            
            Debug.Log("New target position chosen at: " + hit.position);
            return hit.position;
        }

        Debug.LogWarning("Failed to find a new target position retrying.");

        return ChooseNewTargetPos();
    }

    /// <summary>
    /// Called when entering the GoToNewTarget state.
    /// Generates a new target position and sets up navigation to that position.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Creates a new target position using <see cref="ChooseNewTargetPos"/> and instantiates a temporary
    /// GameObject to represent that position for rotation purposes.
    /// </para>
    /// <para>
    /// The temporary GameObject is necessary for the <see cref="DogState.RotateDogTowardsTarget"/> method
    /// to have a transform reference to calculate rotation direction.
    /// </para>
    /// </remarks>
    public override void Enter()
   {
        base.Enter();
      
        Vector3 newTargetPos = ChooseNewTargetPos();

        GameObject target = new("TargetPosition");
        target.transform.position = newTargetPos;
        _newTargetTransform = target.transform;

        _agent.SetDestination(newTargetPos);
   }

    /// <summary>
    /// Called every frame while in the GoToNewTarget state.
    /// Rotates the dog toward the new target position and checks if the dog has reached it.
    /// </summary>
    /// <remarks>
    /// Continuously rotates the dog to face the target while moving.
    /// When the dog reaches the target position, triggers the transition "PositionReached" to the <see cref="DogIdle"/> state.
    /// </remarks>
    public override void Execute()
    {
        base.Execute();

        RotateDogTowardsTarget(_newTargetTransform);

        if (DogStoped())
         {  
            fSM.ChangeState("PositionReached");
         }
    }

    /// <summary>
    /// Called when exiting the GoToNewTarget state.
    /// Destroys the temporary target GameObject to clean up the scene.
    /// </summary>
    /// <remarks>
    /// The temporary target GameObject created in <see cref="Enter"/> is no longer needed
    /// once the state is exited, so it's destroyed to prevent clutter.
    /// </remarks>
    public override void Exit()
    {  
        base.Exit();

        Destroy(_newTargetTransform.gameObject);
    }
}
