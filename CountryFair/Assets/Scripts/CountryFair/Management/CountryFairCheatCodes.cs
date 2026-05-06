

using System.Linq;
using UnityEngine;

/// <summary>
/// Cheat code handler for the CountryFair hub scene.
/// Provides developer shortcuts to skip the intro dialogue and jump directly into either mini-game.
/// </summary>
public class CountryFairCheatCodes : CheatCodes
{
    /// <summary>Reference to the Archery tent component used to trigger scene loading via cheat.</summary>
    [SerializeField]
    private ShowTentData archeryTentData;

    /// <summary>Reference to the Frisbee tent component used to trigger scene loading via cheat.</summary>
    [SerializeField]
    private ShowTentData frisbeeTentData;

    /// <summary>Reference to the dialogue script used to skip the intro sequence via cheat.</summary>
    [SerializeField]
    private CountryFairDialogue countryFairDialogue;


    private bool _introCompleted = false;

    private void Awake()
    {
       if (archeryTentData == null)
        {
            Debug.LogError("Archery Tent Data reference is not assigned in the inspector.");

            return;
        }

        if (frisbeeTentData == null)
        {
            Debug.LogError("Frisbee Tent Data reference is not assigned in the inspector.");

            return;
        }

        if (countryFairDialogue == null)
        {
            Debug.LogError("Country Fair Dialogue reference is not assigned in the inspector.");

            return;
        }


        _introCompleted = GameManager.GetInstance().IntroCompleted;

        RegisterBaseCheats();

       _maxCheatLength = _cheatCommands.Keys.Max(code => code.Length);
    }


    protected override void RegisterBaseCheats()
    {
        RegisterCheat("intro", () => CompleteIntro());
        RegisterCheat("frisbee", () => GoToMiniGame(true));
        RegisterCheat("archery", () => GoToMiniGame(false));
    }


    protected override void CheckCheatCode()
    {   
        foreach (var (code, command) in _cheatCommands)
        {
            if (_playerInput.Contains(code))
            {
                _playerInput = string.Empty;

                command.Invoke();

                return;
            }
        }
    }

    private void CompleteIntro()
    {    
        if (!_introCompleted)
        {
            _introCompleted = true;
            countryFairDialogue.IntroComplete();
        }
    }

    private void GoToMiniGame(bool isFrisbee)
    {
       if (!_introCompleted)
        {  
            Debug.LogWarning("Intro must be completed before accessing minigames. Finish the intro first");
            return;
        }
       
       if (isFrisbee)
        {
            frisbeeTentData.GoToMiniGame();
            return;
        }

         archeryTentData.GoToMiniGame();
    }
}
   