using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// State where the dog returns to the player with the frisbee in its mouth.
/// Upon reaching the player, the dog gives the frisbee back and transitions based on whether the player scored.
/// </summary>
/// <remarks>
/// This state activates a visual representation of the frisbee in the dog's mouth.
/// When the dog reaches the player, it invokes the <see cref="frisbeeGivenToPlayer"/> event
/// and triggers the transition "PlayerScored" to the <see cref="GoToNewTarget"/> state  or the transition "PlayerMissed"  to the <see cref="GoToPreviousTarget"/> state
/// based on the <see cref="_playerScored"/> flag.
/// </remarks>
public class GiveFrisbeeToPlayer : DogState
{  
   /// <summary>
   /// Visual representation of the frisbee held in the dog's mouth.
   /// This GameObject is activated when entering the state and deactivated on exit.
   /// </summary>
   [SerializeField]
    private GameObject frisbeeInDogMouth;

    /// <summary>
    /// Flag indicating whether the player scored on this round.
    /// Set to true by calling <see cref="PlayerScored"/> and used to determine the next state transition.
    /// </summary>
    private bool _playerScored = false;

    /// <summary>
    /// Event invoked when the dog successfully returns the frisbee to the player.
    /// This event is used to trigger the <see cref="Landed.FrisbeeGivenByDog"/> method to notify that the frisbee has been given back.
    /// </summary>
    public UnityEvent frisbeeGivenToPlayer;
   
    /// <summary>
    /// Initializes the GiveFrisbeeToPlayer state and validates the frisbee reference.
    /// </summary>
    /// <remarks>
    /// Calls base initialization and ensures the frisbeeInDogMouth GameObject is assigned.
    /// Deactivates the frisbee GameObject initially so it only appears when the state is entered.
    /// Logs an error if the frisbee reference is not set in the Inspector.
    /// </remarks>
    protected override void Awake()
    {  
        base.Awake();

        if(frisbeeInDogMouth == null)
        {
            Debug.LogError("Frisbee reference not set in ChooseNewPosition state.");

            return;
        }

        frisbeeInDogMouth.SetActive(false);    
    }

    /// <summary>
    /// Initializes references to the player and game manager.
    /// </summary>
    /// <remarks>
    /// Calls <see cref="DogState.LateStart"/> to set up player and game manager references
    /// needed for navigation and game state checking.
    /// </remarks>
    public override void LateStart()
    {    
        base.LateStart();
    }

    /// <summary>
    /// Called when entering the GiveFrisbeeToPlayer state.
    /// Activates the frisbee visual in the dog's mouth and sets navigation to the player.
    /// </summary>
    /// <remarks>
    /// Makes the frisbee GameObject visible in the dog's mouth and instructs the NavMeshAgent
    /// to navigate to the player's current position.
    /// </remarks>
    public override void Enter()
   {
        base.Enter();

        frisbeeInDogMouth.SetActive(true);

        _agent.SetDestination(_playerTransform.position);
   }

    /// <summary>
    /// Called every frame while in the GiveFrisbeeToPlayer state.
    /// Rotates the dog toward the player and checks if the dog has reached them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Continuously rotates the dog to face the player while moving.
    /// </para>
    /// <para>
    /// When the dog reaches the player:
    /// <list type="number">
    /// <item><description>Invokes the <see cref="frisbeeGivenToPlayer"/> event</description></item>
    /// <item><description>Transitions to "PlayerScored" if <see cref="_playerScored"/> is true, otherwise "PlayerMissed"</description></item>
    /// <item><description>Resets the <see cref="_playerScored"/> flag for the next round</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public override void Execute()
    {
         base.Execute();

         RotateDogTowardsTarget(_playerTransform);

         if (DogStoped())
         {  
            frisbeeGivenToPlayer.Invoke();

            string transitionName = _playerScored ? "PlayerScored" : "PlayerMissed";

            fSM.ChangeState(transitionName);
            
            // Reset the flag for the next time (overrides to not use an if statement)
            _playerScored = false;
         }
    }

    /// <summary>
    /// Called when exiting the GiveFrisbeeToPlayer state.
    /// Deactivates the frisbee visual in the dog's mouth.
    /// </summary>
    /// <remarks>
    /// Hides the frisbee GameObject that was visible in the dog's mouth during this state.
    /// </remarks>
    public override void Exit()
    {
        base.Exit();

        frisbeeInDogMouth.SetActive(false);
    }
    

    /// <summary>
    /// Marks that the player scored during this round.
    /// Sets the <see cref="_playerScored"/> flag to true, which determines the next state transition.
    /// </summary>
    /// <remarks>
    /// This method should be called by external game logic (e.g., score detection system)
    /// before the dog reaches the player to ensure the correct state transition occurs.
    /// </remarks>
    public void PlayerScored()
    {
        _playerScored = true;
    }
}
