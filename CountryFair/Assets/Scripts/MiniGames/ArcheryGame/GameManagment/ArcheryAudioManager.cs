using UnityEngine;
using FMODUnity;

/// <summary>
/// Archery-scene audio manager. Handles spatial balloon-pop and arrow-shot sounds in addition to
/// the Carny Wise DDA feedback sounds inherited from <see cref="MiniGameAudioManager"/>.
/// </summary>
public class ArcheryAudioManager :MiniGameAudioManager
{
    /// <summary>FMOD event played when a balloon is popped by an arrow.</summary>
    [SerializeField]
    private EventReference balloonPopSound;

    /// <summary>FMOD event played when the player releases the arrow.</summary>
    [SerializeField]
    private EventReference arrowShotSound;

    protected override void Awake()
    {   
        base.Awake();

        if (balloonPopSound.IsNull)
        {
            Debug.LogError("Balloon pop sound EventReference is not assigned in ArcheryAudioManager.");

            return;
        }

        if (arrowShotSound.IsNull)
        {
            Debug.LogError("Arrow shot sound EventReference is not assigned in ArcheryAudioManager.");
        }
    }

    protected override void Start()
    {
        base.Start();
    }


    public override void PlaySpatialSoundEffect(GameSoundEffects soundEffect, GameObject target)
    {
        EventReference eventToPlay;

        switch (soundEffect)
        {  
           case GameSoundEffects.BUTTON_PRESSED:
                eventToPlay = buttonPressedSound;
                break;

           case GameSoundEffects.BALLOON_POP:
                eventToPlay = balloonPopSound;
                break;

            case GameSoundEffects.ARROW_SHOT:
                eventToPlay = arrowShotSound;
                break;
   
            default:
                Debug.LogError("Invalid sound effect: " + soundEffect);
                return;
        }

        RuntimeManager.PlayOneShotAttached(eventToPlay, target);
    }


    public override void PlaySoundEffect(GameSoundEffects soundEffect)
    {
        EventReference eventToPlay;

        switch (soundEffect)
        {  
          case GameSoundEffects.CARNYWISE_INCREASE_DIFF:
                eventToPlay = carnyWiseIncreaseDiffSound;
                break;

            case GameSoundEffects.CARNYWISE_DECREASE_DIFF:
                eventToPlay = carnyWiseDecreaseDiffSound;
                break;

            default:
                Debug.LogError("Invalid sound effect: " + soundEffect);
                return;
        }

        RuntimeManager.PlayOneShot(eventToPlay);
    }
}