using UnityEngine;
using System.Linq;
using System;


[RequireComponent(typeof(MiniGameManager))]
public class MiniGameCheatCodes : CheatCodes
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


    private MiniGameManager _miniGameManager;

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

        RegisterBaseCheats();
    }

    /// <summary>
    /// Registers all shared cheat codes available across every mini-game.
    /// Called during Start() before subclass cheats are added.
    /// </summary>
    protected override void RegisterBaseCheats()
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
    /// Checks if the current input buffer contains any valid cheat code and executes it.
    /// </summary>
    protected override void CheckCheatCode()
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