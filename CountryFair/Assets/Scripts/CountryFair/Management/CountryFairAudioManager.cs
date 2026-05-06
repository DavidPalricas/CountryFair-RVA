using UnityEngine;
using FMODUnity;
using FMOD.Studio;


/// <summary>
/// Audio manager for the CountryFair hub scene.
/// Starts a spatialized crowd-noise loop on top of the shared ambience music from <see cref="AudioManager"/>.
/// </summary>
public class CountryFairAudioManager : AudioManager
{
    /// <summary>FMOD event reference for the crowd ambience loop spatialized to the fair area.</summary>
   [SerializeField]
   private EventReference crowdNoise;

    private EventInstance  _crowdNoiseInstance;

   protected override void Awake()
   {  
      base.Awake();

      if (crowdNoise.IsNull)
        {
            Debug.LogError("Crowd noise EventReference is not assigned in CountryFairAudioManager.");
        }
   }

   protected override void Start()
   {
      base.Start();

      PlayCrowdSound();
   }


   private void PlayCrowdSound()
    {   
        _crowdNoiseInstance = RuntimeManager.CreateInstance(crowdNoise);
        RuntimeManager.AttachInstanceToGameObject(_crowdNoiseInstance, transform);
        _crowdNoiseInstance.start();
    }


    public override void PlaySpatialSoundEffect(GameSoundEffects soundEffect, GameObject target)
    {
        EventReference eventToPlay;

        switch (soundEffect)
        {  
           case GameSoundEffects.BUTTON_PRESSED:
                eventToPlay = buttonPressedSound;
                break;
                
            default:
                Debug.LogError("Invalid sound effect: " + soundEffect);
                return;
        }

        RuntimeManager.PlayOneShotAttached(eventToPlay, target);
    }

    private void OnDestroy()
    { 
        _crowdNoiseInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _crowdNoiseInstance.release();
    }
}