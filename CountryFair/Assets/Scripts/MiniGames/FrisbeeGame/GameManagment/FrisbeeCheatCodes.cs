using System.Linq;
using UnityEngine;

/// <summary>
/// Manages cheat code detection and activation for the Frisbee mini-game.
/// Listens for keyboard input and triggers specific actions when valid cheat codes are entered.
/// </summary>
public class FrisbeeCheatCodes : MiniGameCheatCodes
{   
    [Header ("Dog Dependencies")]
    [SerializeField]
    private Transform dogScoreAreaTransform;

    [SerializeField]
    private DogIdle dogIdleState;
    
    [Header("Frisbee Dependencies")]

    [SerializeField]
    /// <summary>
    /// Reference to the frisbee's OnPlayerFront state component.
    /// </summary>
    private OnPlayerFront frisbeePlayerFrontState;
    
    [SerializeField]
    /// <summary>
    /// Reference to the frisbee's Landed state component.
    /// </summary>
    private Landed frisbeeLandedState = null;
     
    [SerializeField]
    /// <summary>
    /// Transform of the frisbee game object.
    /// </summary>
    private Transform frisbeeTransform = null;
    
    [SerializeField]
    /// <summary>
    /// Finite State Machine component controlling the frisbee's behavior.
    /// </summary>
    private FSM frisbeeFSM = null;

    /// <summary>
    /// Initializes component references and registers frisbee-specific cheat codes.
    /// Unity callback called when the script instance is being loaded.
    /// </summary>
    protected override void Awake()
    {   
        base.Awake();
        if (frisbeeFSM == null)
        {
            Debug.LogError("FSM component not found on Frisbee GameObject.");
            return;
        }

        if (dogIdleState == null)
        {
            Debug.LogError("DogIdle component reference not assigned in the inspector.");
            return;
        }

        if (frisbeePlayerFrontState == null)
        {
            Debug.LogError("OnPlayerFront component not found.");
            return;
        }

        if (frisbeeLandedState == null)
        {
            Debug.LogError("Landed component not found.");
            return;
        }

        if (dogScoreAreaTransform == null)
        {
            Debug.LogError("DogScoreArea Transform not assigned.");
            return;
        }

        // Register frisbee-specific cheat
        RegisterCheat("dog", () => ForceScorePoint(true));

        _tutorialCompleted = GameManager.GetInstance().FrisbeeTutorialCompleted;

        _maxCheatLength = _cheatCommands.Keys.Max(c => c.Length);
    }

    /// <summary>
    /// Throws the frisbee without scoring.
    /// </summary>
    protected override void OnMissCheat()
    {
        frisbeePlayerFrontState.ThrowFrisbee(false);
    }

    /// <summary>
    /// Teleports the frisbee to a random extra score area, forcing a score point.
    /// </summary>
    protected override void OnScoreCheat()
    {
        ForceScorePoint(false);
    }

    /// <summary>
    /// Forces the frisbee to score a point by teleporting it to the score area.
    /// Sets the frisbee as a child of the score area and triggers the scoring state.
    /// Only works if the frisbee is in the OnPlayerFront state and the score area is active.
    /// </summary>
    private void ForceScorePoint(bool isDog)
    {
        frisbeeTransform.parent = null;
        frisbeeTransform.localPosition = isDog ? dogScoreAreaTransform.position : GetExtraScoreAreaPosition();

        if (frisbeeFSM.CurrentState == frisbeePlayerFrontState && dogScoreAreaTransform.gameObject.activeSelf)
        {
            frisbeeFSM.ChangeState("ForcedPoint");
        }
    }


    private Vector3 GetExtraScoreAreaPosition()
    {
        GameObject[] scoreAreas = GameObject.FindGameObjectsWithTag("ScoreArea")
                                  .Where(scoreArea => scoreArea != dogScoreAreaTransform.gameObject)
                                  .ToArray();

        if (scoreAreas.Length == 0)
        {
            Debug.LogError("No Extra ScoreArea objects found in the scene.");
            return Vector3.zero;
        }

        return scoreAreas[Utils.RandomValueInRange(0, scoreAreas.Length)].transform.position;
    }
}