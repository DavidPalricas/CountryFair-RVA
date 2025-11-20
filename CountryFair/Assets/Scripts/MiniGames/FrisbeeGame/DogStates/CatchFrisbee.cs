using UnityEngine;

/// <summary>
/// State where the dog navigates to and catches the thrown frisbee.
/// The dog moves toward the frisbee's position, and upon arrival, deactivates it and transitions to the next state.
/// </summary>
/// <remarks>
/// This state is triggered when the frisbee is thrown and the dog needs to retrieve it.
/// Upon successfully reaching the frisbee, this state deactivates the frisbee GameObject and  triggers the transition "FrisbeeCaught", 
/// to the <see cref="GiveFrisbeeToPlayer"/> state.
/// </remarks>
public class CatchFrisbee : DogState
{  
    /// <summary>
    /// Reference to the frisbee's transform component.
    /// Found in the scene during <see cref="LateStart"/> using the "Frisbee" tag.
    /// </summary>
    private Transform _frisbeeTransform;

    /// <summary>
    /// Initializes the CatchFrisbee state by calling the base DogState initialization.
    /// </summary>
    /// <remarks>
    /// Calls <see cref="DogState.Awake"/> to set up state properties and NavMeshAgent configuration.
    /// </remarks>
    protected override void Awake()
    {  
        base.Awake();
    }

    /// <summary>
    /// Initializes the frisbee reference by finding it in the scene.
    /// </summary>
    /// <remarks>
    /// Searches for a GameObject with the "Frisbee" tag and caches its transform.
    /// Note: This override does not call base.LateStart() because this state doesn't require
    /// the player transform or game manager references that the base class initializes.
    /// Logs an error if the frisbee cannot be found.
    /// </remarks>
    public override void LateStart()
    {
        _frisbeeTransform = GameObject.FindGameObjectWithTag("Frisbee").transform;

        if (_frisbeeTransform == null)
        {
            Debug.LogError("Frisbee GameObject not found in the scene.");
        }
    }

    /// <summary>
    /// Called when entering the CatchFrisbee state.
    /// Sets the dog's navigation destination to the frisbee's current position.
    /// </summary>
    /// <remarks>
    /// Retrieves the frisbee's position and instructs the NavMeshAgent to navigate to that location.
    /// The dog will begin moving toward the frisbee when this method completes.
    /// </remarks>
    public override void Enter()
   {
        base.Enter();

        Vector3 frisbeePos = _frisbeeTransform.position;

        _agent.SetDestination(frisbeePos);
   }

    /// <summary>
    /// Called every frame while in the CatchFrisbee state.
    /// Rotates the dog toward the frisbee and checks if the dog has reached it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Continuously rotates the dog to face the frisbee while moving.
    /// </para>
    /// <para>
    /// When the dog stops at the frisbee's location:
    /// <list type="number">
    /// <item><description>Deactivates the frisbee GameObject (simulating the catch)</description></item>
    /// <item><description>Triggers the transition "FrisbeeCaught" to the <see cref="GiveFrisbeeToPlayer"/> state</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public override void Execute()
    {
         base.Execute();

        RotateDogTowardsTarget(_frisbeeTransform);

         if (DogStoped())
         {  
            _frisbeeTransform.gameObject.SetActive(false);
            fSM.ChangeState("FrisbeeCaught");
         }
    }

    /// <summary>
    /// Called when exiting the CatchFrisbee state.
    /// </summary>
    /// <remarks>
    /// Performs base cleanup for state exit. No additional cleanup is required for this state.
    /// </remarks>
    public override void Exit()
    {   
        base.Exit();
    }
}
