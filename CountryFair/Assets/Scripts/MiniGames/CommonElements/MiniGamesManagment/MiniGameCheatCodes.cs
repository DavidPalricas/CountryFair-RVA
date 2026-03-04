using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;


[RequireComponent(typeof(MiniGameManager))]
public class MiniGameCheatCodes : MonoBehaviour
{
    [Header("Mini Game Dependencies")]

    [SerializeField]
    private ReturnToFair returnToFair;

    [SerializeField]
    private CarnyWise carnyWise;

    [SerializeField]
    private Tutorial tutorial;


    [SerializeField]
    private ActivateEmoji activateEmoji;

    private string _playerInput = "";

    private MiniGameManager _miniGameManager;
    
    /// <summary>
    /// Dictionary mapping cheat code strings to their corresponding commands (Actions).
    /// Subclasses register their own cheats via RegisterCheat().
    /// </summary>
    protected Dictionary<string, System.Action> _cheatCommands = new();

    /// <summary>
    /// Maximum length of any cheat code, used to limit input buffer size.
    /// </summary>
    protected int _maxCheatLength;

    protected bool _tutorialCompleted = false;

    private readonly string[] _cheatCodesOnTutorial = new string[] {"return", "tutorial"};

    protected virtual void Awake()
    {
        if (carnyWise == null)
        {
            Debug.LogError("CarnyWise component reference is not assigned in the inspector.");

            return;
        }

        if (returnToFair == null)
        {
            Debug.LogError("ReturnToFair component reference is not assigned in the inspector.");

            return;
        }

        if (tutorial == null)
        {
            Debug.LogError("Tutorial component reference is not assigned in the inspector.");

            return;
        }

        if (activateEmoji == null)
        {
            Debug.LogError("ActivateEmoji component not found in the scene. Please ensure there is a GameObject with the 'Emojis' tag and an ActivateEmoji component.");
            return;
        }


        _miniGameManager = GetComponent<MiniGameManager>();

        RegisterBasecheats();
    }

    /// <summary>
    /// Registers all shared cheat codes available across every mini-game.
    /// Called during Start() before subclass cheats are added.
    /// </summary>
    private void RegisterBasecheats()
    {
        RegisterCheat("return", () => returnToFair.Return());
        RegisterCheat("tutorial", () => SkipTutorial());
        RegisterCheat("reset",  () => ResetDifficulty());
        RegisterCheat("miss",   OnMissCheat);
        RegisterCheat("score",  OnScoreCheat);
        RegisterCheat("increase", () => IncreaseDifficulty());
        RegisterCheat("decrease", () => DecreaseDifficulty());
        RegisterCheat("happy",    () => activateEmoji.UpdateVisuals(ActivateEmoji.EmojiType.HAPPY));
        RegisterCheat("neutral",  () => activateEmoji.UpdateVisuals(ActivateEmoji.EmojiType.NEUTRAL));
        RegisterCheat("sad",      () => activateEmoji.UpdateVisuals(ActivateEmoji.EmojiType.SAD));
        RegisterCheat("angry",    () => activateEmoji.UpdateVisuals(ActivateEmoji.EmojiType.ANGRY));
        RegisterCheat("disgust",  () => activateEmoji.UpdateVisuals(ActivateEmoji.EmojiType.DISGUST));
        RegisterCheat("surprise", () => activateEmoji.UpdateVisuals(ActivateEmoji.EmojiType.SURPRISE));
        RegisterCheat("fear",     () => activateEmoji.UpdateVisuals(ActivateEmoji.EmojiType.FEAR));
    }

    /// <summary>
    /// Registers a cheat code with its associated command.
    /// Subclasses call this in Start() (after base.Start()) to add game-specific cheats.
    /// </summary>
    /// <param name="code">The cheat code string.</param>
    /// <param name="command">The action to execute when the code is entered.</param>
    protected void RegisterCheat(string code, System.Action command)
    {
        _cheatCommands[code] = command;
    }

    /// <summary>
    /// Called when the "miss" cheat is triggered. Override in subclasses for specific behaviour.
    /// </summary>
    protected virtual void OnMissCheat()
    {
        Debug.LogError("OnMissCheat should be overridden in derived classes.");
    }

    /// <summary>
    /// Called when the "score" cheat is triggered. Override in subclasses for specific behaviour.
    /// </summary>
    protected virtual void OnScoreCheat()
    {
        Debug.LogError("OnScoreCheat should be overridden in derived classes.");
    }

    /// <summary>
    /// Subscribes to keyboard text input events when the component is enabled.
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
    /// </summary>
    private void OnDisable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= OnTextInput;
    }

    /// <summary>
    /// Handles keyboard character input and checks for cheat code patterns.
    /// </summary>
    private void OnTextInput(char c)
    {
        if (!char.IsLetterOrDigit(c)) return;

        _playerInput += c.ToString().ToLower();

        if (_playerInput.Length > _maxCheatLength)
            _playerInput = _playerInput[^_maxCheatLength..];

        CheckCheatCode();
    }

    /// <summary>
    /// Checks if the current input buffer contains any valid cheat code and executes it.
    /// </summary>
    private void CheckCheatCode()
    {
        foreach (var (code, command) in _cheatCommands)
        {
            if (_playerInput.Contains(code))
            {
                _playerInput = string.Empty;

                // Every cheat code except the  skp tutorial one ("tutorial") requires the tutorial to be completed, to avoid errors this statment is added"
                if (!_tutorialCompleted && code != "tutorial" && !_cheatCodesOnTutorial.Contains(code))
                {
                    Debug.LogWarning($"Cheat code '{code}' entered but tutorial not completed. Cheat ignored.");

                    return;      
                }

                command.Invoke();

                return;
            }
        }
    }

    private void ResetDifficulty()
    {   
        if (_miniGameManager.difficultyLevel > 0)
        {
            carnyWise.DecreaseDifficulty();
            _miniGameManager.ResetDifficulty();
        }
   
     }

    private  void IncreaseDifficulty()
    {      
        carnyWise.IncreaseDifficulty();

        _miniGameManager.ChangeDifficulty(true);
    }

    private void DecreaseDifficulty()
    {   
        if (_miniGameManager.difficultyLevel > 0)
        {
            carnyWise.DecreaseDifficulty();
             
            _miniGameManager.ChangeDifficulty(false);     
        }
    }

    private void SkipTutorial()
    {   
        if (_tutorialCompleted)
        {
            Debug.LogWarning("Tutorial already completed. Cheat ignored.");
            return;
        }

        tutorial.ReadyToPlay();

        _tutorialCompleted = true;
    }
}