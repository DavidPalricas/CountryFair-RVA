

using System.Linq;
using UnityEngine;

public class CountryFairCheatCodes : CheatCodes
{
     [SerializeField]
     private ShowTentData archeryTentData;


    [SerializeField]
    private ShowTentData frisbeeTentData;


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
   