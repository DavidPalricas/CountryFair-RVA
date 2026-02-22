using UnityEngine;
using System.Linq;

public class  ArcheryCheatCodes : CheatCodes
{   
    private Transform _arrowTransform = null;

    private Arrow _arrowComponent = null;

    /// <summary>
    /// Initializes the maximum cheat code length based on defined cheat codes.
    /// Unity callback called when the script instance is being loaded.
    /// </summary>
    protected override void Start()
    {   
        base.Start();

        GameObject arrow = GameObject.FindGameObjectWithTag("Arrow");

        if (arrow == null)
        {
            Debug.LogError("Arrow GameObject not found.");
            return;
        }

        _arrowTransform = arrow.transform;

        if (!arrow.TryGetComponent<Arrow>(out var arrowComponent))
        {
            Debug.LogError("Arrow component not found on Arrow GameObject.");

            return;
        }

        _arrowComponent = arrowComponent;
    }

    protected override void ActivateCheat(string cheatCode)
    {
        base.ActivateCheat(cheatCode);

        switch (cheatCode)
        {    case "reset":
                _miniGameManager.ResetDifficulty();
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
                _arrowComponent.Launch(0f);
                return;

            case "score":
                // Random value of launch force, to ensure the arrow is no kinematic
                 _arrowComponent.Launch(5f);
                _arrowTransform.position = GetCorrectBalloonPosition() + new Vector3(0, 0f, 0);
                return;
            default:
                Debug.LogWarning("Unhandled cheat code: " + cheatCode);
                return;
        }
    }

    private Vector3 GetCorrectBalloonPosition()
    {  
        string balloonColorToScore = PlayerPrefs.GetString("BalloonColorToScore", "red").ToLower();
        
        Transform targetBalloonTransform = GameObject.FindGameObjectsWithTag("Balloon")
            .FirstOrDefault(balloon => balloon.GetComponent<BalloonArcheryGame>().GetBalloonColorName().ToLower() == balloonColorToScore)
            .transform;
            
        if (targetBalloonTransform == null){
            Debug.LogError("Correct balloon color not found!");
            return Vector3.zero;
        }

        return targetBalloonTransform.position;
    }
}