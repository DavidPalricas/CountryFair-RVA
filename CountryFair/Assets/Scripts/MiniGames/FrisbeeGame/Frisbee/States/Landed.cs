using UnityEngine.Events;

/// <summary>
/// State representing the frisbee after it has landed and stopped on the ground.
/// Handles the landing event invocation and disabling of trajectory visualization.
/// Waits for the dog to retrieve the frisbee before transitioning to the <see cref="OnPlayerHand"/> state.
/// </summary>
public class Landed: FrisbeeState
{   
    /// <summary>Event invoked when the frisbee has successfully landed on the ground.
    /// </summary>
    /// <remarks>
    /// This event is triggered in the <see cref="DogIdle.FrisbeeLanded"/> to trigger the dog to catch the frisbee.
    /// </remarks>
    public UnityEvent frisbeeLanded;

    /// <summary>
    /// Initializes the state by setting up physics component references.
    /// </summary>
    protected override void Awake()
    {  
        base.Awake();
    }

    /// <summary>
    /// Called when entering the Landed state.
    /// Invokes the frisbeeLanded event to notify other systems and disables the trajectory visualization.
    /// </summary>
    public override void Enter()
   {
        base.Enter();

        frisbeeLanded.Invoke();
        _trajectoryLine.enabled = false;
   }

    /// <summary>
    /// Called every frame while in the Landed state.
    /// Currently no per-frame logic needed while waiting for dog retrieval.
    /// </summary>
    public override void Execute()
    {
         base.Execute();
    }

    /// <summary>
    /// Called when exiting the Landed state (when dog retrieves the frisbee).
    /// </summary>
    public override void Exit()
    {   
        base.Exit();
    }

    /// <summary>
    /// Called by the event <see cref="GiveFrisbeeToPlayer.frisbeeGivenToPlayer"/> when the dog gives the frisbee back to the player.
    /// Triggers the transition "RetrievedByDog" to the <see cref="OnPlayerHand"/> state.
    /// </summary>
    public void FrisbeeGivenByDog()
    {   
        fSM.ChangeState("RetrievedByDog");
    }
}