using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages cheat code detection and activation for the Frisbee mini-game.
/// Listens for keyboard input and triggers specific actions when valid cheat codes are entered.
/// </summary>
public class CheatCodesFrisbee : MonoBehaviour
{   
    /// <summary>
    /// Stores the current player keyboard input sequence for cheat code detection.
    /// </summary>
    private string _playerInput = "";

    /// <summary>
    /// Array of valid cheat codes that can be activated.
    /// "throw" - Forces the frisbee to be thrown.
    /// "score" - Forces a score point to be registered.
    /// </summary>
    private readonly string[] _cheatCodes = new string[] { "throw", "score"};

    /// <summary>
    /// Maximum length of any cheat code, used to limit input buffer size.
    /// </summary>
    private int _maxCheatLength;

    /// <summary>
    /// Reference to the frisbee's OnPlayerFront state component.
    /// </summary>
    private OnPlayerFront _frisbeePlayerFrontState;

    /// <summary>
    /// Reference to the frisbee's Landed state component.
    /// </summary>
    private Landed _frisbeeLandedState = null;

    /// <summary>
    /// Transform of the score area where points are registered.
    /// </summary>
     private Transform _scoreAreaTransform = null;

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
    private void Awake()
    {   
        _maxCheatLength = _cheatCodes.Max(c => c.Length);

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

        GameObject scoreArea = GameObject.FindGameObjectWithTag("ScoreArea");


        if (scoreArea == null)
        {
            Debug.LogError("ScoreArea GameObject not found.");
            return;
        }

        _scoreAreaTransform = scoreArea.transform;
    }

    /// <summary>
    /// Subscribes to keyboard text input events when the component is enabled.
    /// Unity callback called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput += OnTextInput;
        }
    }

    /// <summary>
    /// Unsubscribes from keyboard text input events when the component is disabled.
    /// Unity callback called when the object becomes disabled or inactive.
    /// </summary>
    private void OnDisable()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= OnTextInput;
        }
    }

    /// <summary>
    /// Handles keyboard character input and checks for cheat code patterns.
    /// Only processes alphanumeric characters and maintains a rolling buffer of recent input.
    /// </summary>
    /// <param name="c">The character that was input by the player.</param>
    private void OnTextInput(char c)
    {
        if (char.IsLetterOrDigit(c))
        {
            _playerInput += c.ToString().ToLower();

            if (_playerInput.Length > _maxCheatLength)
            {
                _playerInput = _playerInput[^_maxCheatLength..];
            }

            CheckCheatCode();
        }
    }

    /// <summary>
    /// Checks if the current input buffer contains any valid cheat codes.
    /// Iterates through all registered cheat codes and activates the first match found.
    /// </summary>
    private void CheckCheatCode()
    {
       foreach (string code in _cheatCodes)
       {
           if (_playerInput.Contains(code))
           {   
               ActivateCheat(code);
           
               return;
           }
       }
    }

    /// <summary>
    /// Activates the specified cheat code and clears the input buffer.
    /// Executes different actions based on the cheat code provided.
    /// </summary>
    /// <param name="cheatCode">The cheat code string to activate.</param>
    private void ActivateCheat(string cheatCode){
        _playerInput = string.Empty;

        switch (cheatCode)
        {
            case "throw":
                _frisbeePlayerFrontState.ThrowFrisbee();
                break;

            case "score":
                  ForceScorePoint();      
                break;
            default:
                Debug.LogError("Invalid cheat code: " + cheatCode);
                break;
        }
    }

    /// <summary>
    /// Forces the frisbee to score a point by teleporting it to the score area.
    /// Sets the frisbee as a child of the score area and triggers the scoring state.
    /// Only works if the frisbee is in the OnPlayerFront state and the score area is active.
    /// </summary>
    private void ForceScorePoint()
    {
        _frisbeeTransform.parent = _scoreAreaTransform;
        _frisbeeTransform.localPosition = Vector3.zero;
        
        if (_frisbeeFSM.CurrentState == _frisbeePlayerFrontState && _scoreAreaTransform.gameObject.activeSelf)
        {
            _frisbeeFSM.ChangeState("ForcedPoint");
            return;
        }
    }
}
