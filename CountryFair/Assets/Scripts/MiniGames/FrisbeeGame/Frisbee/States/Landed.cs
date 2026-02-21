using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// State representing the frisbee after it has landed and stopped on the ground.
/// Handles the landing event invocation and disabling of trajectory visualization.
/// Waits for the dog to retrieve the frisbee before transitioning to the <see cref="OnPlayerFront"/> state.
/// </summary>
public class Landed: FrisbeeState
{    
    [SerializeField]
    private MeshRenderer frisbeeMeshRenderer;

    public bool TutorialActive { get; set; } = true;

    [Header("Landed Events")]

    /// <summary>Event invoked when the player successfully scores by landing the frisbee in the score area.
    /// </summary>
    /// <remarks>
    /// This event is trigger in the <see cref="GiveFrisbeeToPlayer"/> state to alert the dog that the player has scored,
    /// and it must choose a new position accordingly.
    /// </remarks>
    [SerializeField]
    private UnityEvent <int> playerScored;
    
    [SerializeField]
    private UnityEvent <AudioManager.GameSoundEffects> scoreSoundEffectEvent;

    [SerializeField]
    private UnityEvent playerMissed;

    private readonly AudioManager.GameSoundEffects _scoreSoundEffect = AudioManager.GameSoundEffects.POINT_SCORED;

    private int _scorePoints = 1;

    /// <summary>
    /// Initializes the state by setting up physics component references.
    /// </summary>
    protected override void Awake()
    {  
        base.Awake();

        if (frisbeeMeshRenderer == null)
        {
            Debug.LogError("MeshRenderer component is not assigned in Landed script.");
        }
    }


    public override void LateStart()
    {
        base.LateStart();
    }

    /// <summary>
    /// Called when entering the Landed state.
    /// Invokes the frisbeeLanded event to notify other systems and disables the trajectory visualization.
    /// </summary>
    public override void Enter()
   {
        base.Enter();

        _rigidbody.linearVelocity = Vector3.zero;

        if (FrisbeeOnScoreArea())
        {
            if (TutorialActive)
            {
                fSM.ChangeState("ReturnToPlayer");

                return;
            }

            playerScored.Invoke(_scorePoints);

            return;
        }

        playerMissed.Invoke();

        fSM.ChangeState("ReturnToPlayer");
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
        if (fSM.CurrentState == this)
        {
            fSM.ChangeState("RetrievedByDog");
        }
    }

    private bool FrisbeeOnScoreArea()
    {   
        float frisbeeRadius = frisbeeMeshRenderer.bounds.extents.magnitude;

        Collider[] overlapResults = new Collider[10];

        Physics.OverlapSphereNonAlloc(transform.position, frisbeeRadius, overlapResults);

        foreach (Collider collider in overlapResults)
        {
            if (collider != null && collider.gameObject.CompareTag("ScoreArea")) 
            {    
                GameObject scoreArea = collider.gameObject;

                if (!scoreArea.TryGetComponent<ScoreAreaAnimations>(out var scoreAreaAnimations))
                {
                    Debug.LogError("ScoreAreaAnimations component is missing on the Score Area object.");

                    return false;
                }

                if (!scoreArea.TryGetComponent<ScoreAreaProperties>(out var scoreAreaProperties))
                {
                    Debug.LogError("ScoreAreaProperties component is missing on the Score Area object.");

                    return false;
                }

                scoreAreaAnimations.ScoreAnimation();
                scoreSoundEffectEvent.Invoke(_scoreSoundEffect);

                _scorePoints = scoreAreaProperties.ScorePoints;

                return true;
            }
        }

        return false;
    }
}