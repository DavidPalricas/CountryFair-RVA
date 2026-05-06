using UnityEngine;
using FMODUnity;


/// <summary>
/// Extends <see cref="AudioManager"/> with the Carny Wise DDA sound effects shared across all mini-game scenes.
/// Concrete audio managers (<see cref="FrisbeeAudioManager"/>, <see cref="ArcheryAudioManager"/>) inherit from this class.
/// </summary>
public class MiniGameAudioManager : AudioManager
{
    [Header("Carny Wise Feedback Sound Effects")]
    /// <summary>FMOD event played when CarnyWise increases the difficulty.</summary>
    [SerializeField]
    protected EventReference carnyWiseIncreaseDiffSound;

    /// <summary>FMOD event played when CarnyWise decreases the difficulty.</summary>
    [SerializeField]
    protected EventReference carnyWiseDecreaseDiffSound;


    protected override void Awake()
    {
        base.Awake();


        if (carnyWiseIncreaseDiffSound.IsNull)
        {
            Debug.LogError("Carny Wise increase difficulty sound EventReference is not assigned in MiniGameAudioManager.");

            return;
        }

        if (carnyWiseDecreaseDiffSound.IsNull)
        {
            Debug.LogError("Carny Wise decrease difficulty sound EventReference is not assigned in MiniGameAudioManager.");
        }
    }
}