using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class CarnyWiseDiffFeedback : UIDialog
{    
    [Header("Display Configuration")]
    [SerializeField]
    private float displayDuration = 6f;

    [Header("Carny Wise Expressions")]
    [SerializeField]
    private GameObject increaseDiffExpression;
    [SerializeField]
    private GameObject decreaseDiffExpression;


    [Header("CarnyWise UI")]
    [SerializeField]
    private GameObject carnyUI;

    [Header("Sound Feedback")]
    [SerializeField]
    private UnityEvent <AudioManager.GameSoundEffects> soundFeedback;

    private DiffcultyFeedBackData _feedbackData;

    private readonly AudioManager.GameSoundEffects _increaseDiffSound = AudioManager.GameSoundEffects.CARNYWISE_INCREASE_DIFF;
    private readonly AudioManager.GameSoundEffects _decreaseDiffSound = AudioManager.GameSoundEffects.CARNYWISE_DECREASE_DIFF;

    private Queue<bool> _pendingRequests = new ();

    protected override void Awake()
    {
        base.Awake();

        if (increaseDiffExpression == null || decreaseDiffExpression == null)
        {
            Debug.LogError("ERROR: Carny Wise expressions are not assigned.");
            return;
        }

        if (carnyUI == null)
        {
            Debug.LogError("ERROR: Carny Wise UI GameObject is not assigned.");
            return;
        }

        carnyUI.SetActive(false);
    }


    protected override void OnDataLoaded()
    {
        if (_data is not DiffcultyFeedBackData)
        {
            Debug.LogError("ERROR: Cast to DiffcultyFeedBackData failed.");
            return;
        }

        _feedbackData = _data as DiffcultyFeedBackData;

        while (_pendingRequests.Count > 0)
        {
            bool wasIncrease = _pendingRequests.Dequeue();
            ShowNewDiffFeedback(wasIncrease); 
        }
    }

    protected override System.Type GetJSONDataType()
    {
        return typeof(DiffcultyFeedBackData);
    }

    protected override void SetJSONFileName()
    {
        _jsonFileName = "change_difficulty.json";
    }


    public void ShowNewDiffFeedback(bool isToIncrease)
    {   
        if (_feedbackData == null)
        {
            Debug.LogWarning($"[WARNING] Request received before load finished. Storing in queue... (Increase: {isToIncrease})");
            _pendingRequests.Enqueue(isToIncrease);
            return; 
        }

        carnyUI.SetActive(true);

        List<string> feedbackTexts;

        AudioManager.GameSoundEffects soundToPlay;

        increaseDiffExpression.SetActive(isToIncrease);
        decreaseDiffExpression.SetActive(!isToIncrease);

        if (isToIncrease)
        {
            feedbackTexts = _feedbackData.IncreaseDiff;
            soundToPlay = _increaseDiffSound;
        }
        else
        {
            feedbackTexts = _feedbackData.DecreaseDiff;
            soundToPlay = _decreaseDiffSound;
        }

        soundFeedback.Invoke(soundToPlay);

        if (feedbackTexts != null && feedbackTexts.Count > 0)
        {
            dialogueBoxText.text = feedbackTexts[Utils.RandomValueInRange(0, feedbackTexts.Count)];
        }
        else
        {
            dialogueBoxText.text = "...";
            Debug.LogError("List of feedback texts is null or empty.");
        }

        CancelInvoke(nameof(HideFeedback));
        Invoke(nameof(HideFeedback), displayDuration);
    }

    private  void HideFeedback()
    {
        carnyUI.SetActive(false);
    }
}