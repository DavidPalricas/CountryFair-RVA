
using UnityEngine;

/// <summary>
/// State where the dog navigates back to the previous target position.
/// The target position is retrieved from the game manager's current target position.
/// </summary>
/// <remarks>
/// This state is used when the dog needs to return to a previously established position,
/// such as when repositioning after a failed catch attempt.
/// A temporary GameObject is created to represent the previous target position for rotation purposes.
/// </remarks>
public class GoToPreviousTarget : DogState{
    /// <summary>
    /// Reference to a temporary GameObject's transform representing the previous target position.
    /// Created in <see cref="Enter"/> and destroyed in <see cref="Exit"/>.
    /// </summary>
    private  Transform _previousTargetTransform;

    /// <summary>
    /// Initializes the GoToPreviousTarget state by calling the base DogState initialization.
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
    /// needed to retrieve the previous target position.
    /// </remarks>
    public override void LateStart()
    {
        base.LateStart();
    }

    /// <summary>
    /// Called when entering the GoToPreviousTarget state.
    /// Retrieves the previous target position from the game manager and sets up navigation to that position.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Creates a temporary GameObject positioned at the game manager's stored currentTargetPos.
    /// This temporary GameObject provides a transform reference for the <see cref="DogState.RotateDogTowardsTarget"/> method.
    /// </para>
    /// <para>
    /// Instructs the NavMeshAgent to navigate to the previous target position.
    /// </para>
    /// </remarks>
    public override void Enter()
   {
        base.Enter();
       
        _previousTargetTransform =  new GameObject("PreviousTarget").transform;
        _previousTargetTransform.position = _gameManager.currentTargetPos;

        _agent.SetDestination(_previousTargetTransform.position);
   }

    /// <summary>
    /// Called every frame while in the GoToPreviousTarget state.
    /// Rotates the dog toward the previous target position and checks if the dog has reached it.
    /// </summary>
    /// <remarks>
    /// Continuously rotates the dog to face the previous target while moving.
    /// When the dog reaches the previous target position, triggers the transition "PositionReached" to the <see cref="DogIdle"/> state.
    /// </remarks>
    public override void Execute()
    {
        base.Execute();

        RotateDogTowardsTarget(_previousTargetTransform);

        if (DogStoped())
         {  
            fSM.ChangeState("PositionReached");
         }

    }

    /// <summary>
    /// Called when exiting the GoToPreviousTarget state.
    /// Destroys the temporary target GameObject to clean up the scene.
    /// </summary>
    /// <remarks>
    /// The temporary target GameObject created in <see cref="Enter"/> is only needed for rotation calculations,
    /// so it's destroyed to prevent unnecessary objects from accumulating in the scene.
    /// </remarks>
    public override void Exit()
    {  
        base.Exit();

       // Then destroy the temporary target object becuase it is only needed for the dog rotating towards it
        Destroy(_previousTargetTransform.gameObject);
    }
}
