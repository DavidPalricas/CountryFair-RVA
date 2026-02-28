using System.Linq;
using UnityEngine;


/// <summary>
/// Manages cheat code detection and activation for the Frisbee mini-game.
/// Listens for keyboard input and triggers specific actions when valid cheat codes are entered.
/// </summary>
public class FrisbeeCheatCodes : CheatCodes
{   
    [SerializeField]
    private Transform dogScoreAreaTransform;

    /// <summary>
    /// Reference to the frisbee's OnPlayerFront state component.
    /// </summary>
    private OnPlayerFront _frisbeePlayerFrontState;

    /// <summary>
    /// Reference to the frisbee's Landed state component.
    /// </summary>
    private Landed _frisbeeLandedState = null;

    /// <summary>
    /// Transform of the frisbee game object.
    /// </summary>
     private Transform _frisbeeTransform = null;

    /// <summary>
    /// Finite State Machine component controlling the frisbee's behavior.
    /// </summary>
     private FSM _frisbeeFSM = null;

    /// <summary>
    /// Initializes component references and calculates maximum cheat code length.
    /// Finds and caches references to the Frisbee and ScoreArea game objects and their components.
    /// Unity callback called when the script instance is being loaded.
    /// </summary>
    protected override void Start()
    {   
        base.Start();

        GameObject frisbee = GameObject.FindGameObjectWithTag("Frisbee");

        if (frisbee == null)
        {
            Debug.LogError("Frisbee GameObject not found.");
            return;
        }

        _frisbeeTransform = frisbee.transform;


        _frisbeeFSM = frisbee.GetComponent<FSM>();

        if (_frisbeeFSM == null)
        {
            Debug.LogError("FSM component not found on Frisbee GameObject.");

            return;
        }

        _frisbeePlayerFrontState = frisbee.GetComponent<OnPlayerFront>();

        if (_frisbeePlayerFrontState == null)
        {
            Debug.LogError("OnPlayerFront component not found.");

            return;
        }

        _frisbeeLandedState = frisbee.GetComponent<Landed>();

        if (_frisbeeLandedState == null)
        {
            Debug.LogError("Landed component not found.");

            return;
        }

     
        if (dogScoreAreaTransform == null)
        {
            Debug.LogError("DogScoreArea Transform not assigned.");
            return;
        }
 
        string[] frisbeeCheatCodes = new string[] {"dog"};
        
        _cheatCodes = _cheatCodes.Concat(frisbeeCheatCodes).ToArray();
    }

    /// <summary>
    /// Activates the specified cheat code and clears the input buffer.
    /// Executes different actions based on the cheat code provided.
    /// </summary>
    /// <param name="cheatCode">The cheat code string to activate.</param>
    protected override void ActivateCheat(string cheatCode){
       base.ActivateCheat(cheatCode);

        switch (cheatCode)
        {  
            case "reset":
                _miniGameManager.ResetDifficulty();
                return;

            case "return":
                returnToFair.Return();
                return;

            case "happy":
                _activateEmoji.UpdateVisuals(ActivateEmoji.EmojiType.HAPPY);
                return;

            case "neutral":
                _activateEmoji.UpdateVisuals(ActivateEmoji.EmojiType.NEUTRAL);
                return;

            case "sad":
                _activateEmoji.UpdateVisuals(ActivateEmoji.EmojiType.SAD);
                return;

            case "angry":
                _activateEmoji.UpdateVisuals(ActivateEmoji.EmojiType.ANGRY);
                return;

            case "disgust":
                _activateEmoji.UpdateVisuals(ActivateEmoji.EmojiType.DISGUST);
                return;

            case "surprise":
                _activateEmoji.UpdateVisuals(ActivateEmoji.EmojiType.SURPRISE);
                return;

            case "fear":
                _activateEmoji.UpdateVisuals(ActivateEmoji.EmojiType.FEAR);
                return;

            case "miss":
                _frisbeePlayerFrontState.ThrowFrisbee(false);
                return;

            case "score":
                  ForceScorePoint(false);      
                return;
            
            case "dog":
                ForceScorePoint(true);
                return;
                
            default:
                Debug.LogError("Invalid cheat code: " + cheatCode);
                return;
        }
    }

    /// <summary>
    /// Forces the frisbee to score a point by teleporting it to the score area.
    /// Sets the frisbee as a child of the score area and triggers the scoring state.
    /// Only works if the frisbee is in the OnPlayerFront state and the score area is active.
    /// </summary>
    private void ForceScorePoint(bool isDog)
    {   
        _frisbeeTransform.parent =  null;
        _frisbeeTransform.localPosition = isDog ? dogScoreAreaTransform.position : GetExtraScoreAreaPosition();
        
        if (_frisbeeFSM.CurrentState == _frisbeePlayerFrontState && dogScoreAreaTransform.gameObject.activeSelf)
        {
            _frisbeeFSM.ChangeState("ForcedPoint");
            return;
        }
    }


    private Vector3 GetExtraScoreAreaPosition()
    {
        GameObject[] scoreAreas = GameObject.FindGameObjectsWithTag("ScoreArea").
                                  Where(scoreArea => scoreArea != dogScoreAreaTransform.gameObject).ToArray();

        if (scoreAreas.Length == 0)
        {
            Debug.LogError("No Extra ScoreArea objects found in the scene.");
            return Vector3.zero;
        }

        return scoreAreas[Utils.RandomValueInRange(0, scoreAreas.Length)].transform.position;
    }
}
