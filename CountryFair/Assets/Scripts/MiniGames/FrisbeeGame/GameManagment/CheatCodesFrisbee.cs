using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheatCodesFrisbee : MonoBehaviour
{   
    private string _playerInput = "";

    private readonly string[] _cheatCodes = new string[] { "throw", "score"};

    private int _maxCheatLength;

    private OnPlayerFront _frisbeePlayerFrontState;

    private Landed _frisbeeLandedState = null;

     private Transform _scoreAreaTransform = null;

     private Transform _frisbeeTransform = null;

     private FSM _frisbeeFSM = null;

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


    private void OnEnable()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput += OnTextInput;
        }
    }

    private void OnDisable()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= OnTextInput;
        }
    }

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

    private void ForceScorePoint()
    {
        _frisbeeTransform.parent = null;
        _frisbeeTransform.position = _scoreAreaTransform.position;

        _frisbeeFSM.ChangeState("ForcedPoint");
    }

}
