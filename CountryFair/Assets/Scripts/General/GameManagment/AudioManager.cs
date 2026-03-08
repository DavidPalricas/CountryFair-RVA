using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Base audio manager class that handles ambient music and provides virtual methods for game-specific sound effects.
/// This class uses FMOD for audio playback and should be inherited by specific game audio managers.
/// </summary>
public class AudioManager: MonoBehaviour
{  
    [Header("Ambience Music")]
    /// <summary>
    /// FMOD event reference for the ambient background music.
    /// </summary>
    [SerializeField]
    protected EventReference ambienceMusic;
    
    [Header("Button Pressed Sound Effect")]
    /// <summary>
    /// FMOD event reference for the button pressed sound effect.
    /// </summary>
    [SerializeField]
    protected EventReference buttonPressedSound;
    
    /// <summary>
    /// Tracks whether the ambient music has been started to prevent multiple instances.
    /// </summary>
    private static bool ambienceMusicStarted = false;

    /// <summary>
    /// The FMOD event instance for the ambient music that persists across scenes.
    /// </summary>
    private static EventInstance ambienceMusicInstance;

    /// <summary>
    /// Enumeration of all available game sound effects across different mini-games.
    /// </summary>
    public enum GameSoundEffects {
        /* General Sound Effects */
        BUTTON_PRESSED,
        
        /* Carny Wise Sound Effects */
        CARNYWISE_INCREASE_DIFF,

        CARNYWISE_DECREASE_DIFF,

        /* Frisbee Game Sound Effects*/
        /// <summary>Dog barking sound effect.</summary>
        DOG_BARK,
        /// <summary>Frisbee throwing sound effect.</summary>
        FRISBEE_THROW,
        /// <summary>Point scored sound effect.</summary>
        POINT_SCORED,
        /// <summary>Dog footsteps sound effect.</summary>
        DOG_FOOTSTEPS,

        DOG_HAPPY_WHINE,

        /* Archery Game Sound Effects */
        ///<summary>Balloon popping sound effect.</summary>
        BALLOON_POP,
        
        ///<summary>Arrow shooting sound effect.</summary>
        ARROW_SHOT,
        
        ///<summary>Cheering sound effect.</summary>
        CHEER_SOUND_EFFECT
    }


    protected virtual void Awake()
    {
        if (ambienceMusic.IsNull)
        {
            Debug.LogError("Ambience music EventReference is not assigned in AudioManager.");

            return;
        }

        if (buttonPressedSound.IsNull)
        {
            Debug.LogError("Button pressed sound EventReference is not assigned in AudioManager.");

            return;
        }
    }

    /// <summary>
    /// Initializes the audio manager. Starts ambient music if not already playing, otherwise resumes it.
    /// </summary>
    protected virtual void Start()
    {
        if (!ambienceMusicStarted)
        {
            ambienceMusicStarted = true;
            PlayAmbienceMusic();

            return;
        }

        IsToPauseMusic(false);
    }

    /// <summary>
    /// Plays a spatial sound effect attached to a specific game object in 3D space.
    /// This method must be overridden in derived classes.
    /// </summary>
    /// <param name="soundEffect">The type of sound effect to play.</param>
    /// <param name="target">The game object to which the sound will be attached.</param>
    public virtual void PlaySpatialSoundEffect(GameSoundEffects soundEffect, GameObject target)
    {
        Debug.LogError("The PlaySpatialAudio method must be overridden in a derived class.");
    }

    /// <summary>
    /// Plays a non-spatial sound effect at the default listener position.
    /// This method must be overridden in derived classes.
    /// </summary>
    /// <param name="soundEffect">The type of sound effect to play.</param>
    public virtual void PlaySoundEffect(GameSoundEffects soundEffect)
    {
        Debug.LogError("The PlayGlobalSoundEffect method must be overridden in a derived class.");
    }

    /// <summary>
    /// Creates and starts the ambient music instance.
    /// </summary>
    private void PlayAmbienceMusic()
    {
        ambienceMusicInstance = RuntimeManager.CreateInstance(ambienceMusic);
        ambienceMusicInstance.start();
    }

    /// <summary>
    /// Pauses the ambient music when the audio manager is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        IsToPauseMusic(true);
    }

    /// <summary>
    /// Pauses or resumes the ambient music.
    /// </summary>
    /// <param name="pauseMusic">True to pause the music, false to resume it.</param>
    private void IsToPauseMusic(bool pauseMusic)
    {
        ambienceMusicInstance.setPaused(pauseMusic);
    }
}
