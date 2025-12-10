using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// State representing the frisbee after it has landed and stopped on the ground.
/// Handles the landing event invocation and disabling of trajectory visualization.
/// Waits for the dog to retrieve the frisbee before transitioning to the <see cref="OnPlayerFront"/> state.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class Landed: FrisbeeState
{   
    /// <summary>Event invoked when the frisbee has successfully landed on the ground.
    /// </summary>
    /// <remarks>
    /// This event is triggered in the <see cref="DogIdle.FrisbeeLanded"/> to trigger the dog to catch the frisbee.
    /// </remarks>
    public UnityEvent frisbeeLanded;

        /// <summary>Event invoked when the player successfully scores by landing the frisbee in the score area.
    /// </summary>
    /// <remarks>
    /// This event is trigger in the <see cref="GiveFrisbeeToPlayer"/> state to alert the dog that the player has scored,
    /// and it must choose a new position accordingly.
    /// </remarks>
    public UnityEvent playerScored;


    public UnityEvent playerMissed;

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

         Debug.Log("Frisbee Landed in poistion " + transform.position);

        if (FrisbeeOnScoreArea())
        {
            playerScored.Invoke();
        }
        else
        {
            playerMissed.Invoke();
        }
       
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
    /// Triggers the transition "RetrievedByDog" to the <see cref="OnPlayerFront"/> state.
    /// </summary>
    public void FrisbeeGivenByDog()
    {   
        fSM.ChangeState("RetrievedByDog");
    }

    private bool FrisbeeOnScoreArea()
    {   
        float frisbeeRadius = GetComponent<MeshRenderer>().bounds.extents.magnitude;

        Collider[] overlapResults = new Collider[10];

        Physics.OverlapSphereNonAlloc(transform.position, frisbeeRadius, overlapResults);

        foreach (Collider collider in overlapResults)
        {
            if (collider != null && collider.gameObject.CompareTag("ScoreArea")) 
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        
        float frisbeeRadius = GetComponent<MeshRenderer>().bounds.extents.magnitude;
        Collider[] overlapResults = new Collider[10];
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, frisbeeRadius, overlapResults);
        
        bool isOnScoreArea = false;
        foreach (Collider collider in overlapResults)
        {
            if (collider != null && collider.gameObject.CompareTag("ScoreArea")) 
            {
                isOnScoreArea = true;
                break;
            }
        }
        
        // Verde se estiver na área de pontuação, vermelho caso contrário
        Gizmos.color = isOnScoreArea ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, frisbeeRadius);
        
        // Opcional: desenhar uma esfera sólida semi-transparente
        Gizmos.color = isOnScoreArea ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, frisbeeRadius);
    }
}