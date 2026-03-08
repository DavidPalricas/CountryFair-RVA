using UnityEngine;
using FMODUnity;


public class MiniGameAudioManager : AudioManager
{   
    [Header("Carny Wise Feedback Sound Effects")]
    [SerializeField]
    protected EventReference carnyWiseIncreaseDiffSound;

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