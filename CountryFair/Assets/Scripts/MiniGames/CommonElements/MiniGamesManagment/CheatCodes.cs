using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
[RequireComponent(typeof(MiniGameManager))]

public class CheatCodes : MonoBehaviour
{
    private string _playerInput = "";

    /// <summary>
    /// Maximum length of any cheat code, used to limit input buffer size.
    /// </summary>
    protected int _maxCheatLength;


    protected MiniGameManager _miniGameManager;

    protected ActivateEmoji _activateEmoji;

       /// <summary>
    /// Array of valid cheat codes that can be activated.
    /// "throw" - Forces the frisbee to be thrown.
    /// "score" - Forces a score point to be registered.
    /// </summary>
    protected string[] _cheatCodes = new string[] {"reset", "miss", "score", "neutral","sad", "happy","angry","disgust","surprise","fear","neutral"};

    protected virtual void Start()
    {
         _maxCheatLength = _cheatCodes.Max(c => c.Length);

        _activateEmoji = GameObject.FindGameObjectWithTag("Emojis").GetComponent<ActivateEmoji>();  


        _miniGameManager = GetComponent<MiniGameManager>();

        if (_activateEmoji == null)
        {
            Debug.LogError("ActivateEmoji component not found in the scene. Please ensure there is a GameObject with the 'Emojis' tag and an ActivateEmoji component.");
        }
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

    protected virtual void ActivateCheat(string cheatCode)
    {
        _playerInput = string.Empty;
    }
} 