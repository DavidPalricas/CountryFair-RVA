using UnityEngine;
using UnityEngine.UI;

public class EmojiSlider: MonoBehaviour
{       [SerializeField]
    private Slider slider;

    [Header("Emoji GameObjects")]
    [SerializeField]
    private GameObject neutralEmoji;

    [SerializeField]
    private GameObject happyEmoji;

    [SerializeField]
    private GameObject angryEmoji;


    [Header("Emojis State Thresholds")]
    [SerializeField]
    private float positiveThreshold = 0.6f;

    [SerializeField]
    private float negativeThreshold = 0.4f;


    private GameObject _currentEmojiActive = null;

    enum EMOJI_CATEGORIES
    {
        NEUTRAL,
        POSITIVE,
        NEGATIVE,
    }

    private void Awake()
    {  
        if (slider == null)
        {
            Debug.LogError("Slider reference is not assigned in EmojiSlider.");
            return;
        }


        if (neutralEmoji == null || happyEmoji == null || angryEmoji == null)
        {
            Debug.LogError("One or more emoji GameObjects are not assigned in EmojiSlider.");
            return;
        }

        StartAsNeutral();

        _currentEmojiActive = neutralEmoji;

        slider.value = 0.5f; 
    }


    public void UpdateEmojiSlider(string emojiCategory)
    {
        if (!Enum.TryParse(emojiCategory.ToUpper(), out EMOJI_CATEGORIES category))
        {
            Debug.LogError($"Invalid emoji category: {emojiCategory}");
            return;
        }


        if (category == EMOJI_CATEGORIES.POSITIVE)
        {
             slider.value = MathF.Min(slider.value + 0.1f, 1);

             UpdateEmojiVisual();

             return;
        }

        if (category == EMOJI_CATEGORIES.NEGATIVE)
        {
             slider.value = MathF.Max(slider.value - 0.1f, 0);

            UpdateEmojiVisual();
        }
    }
   
    private void StartAsNeutral()
    {
        _currentEmojiActive = neutralEmoji;
        neutralEmoji.SetActive(true);
        slider.value = 0.5f;

        happyEmoji.SetActive(false);
        angryEmoji.SetActive(false);
    }


    private void UpdateEmojiVisual()
    {  
        _currentEmojiActive.SetActive(false);

        if (slider.value >= positiveThreshold)
        {
            _currentEmojiActive = happyEmoji;
            happyEmoji.SetActive(true);

            return;
        }

        if (slider.value <= negativeThreshold)
        {
            _currentEmojiActive = angryEmoji;
            angryEmoji.SetActive(true);

            return;
        }

        _currentEmojiActive = neutralEmoji;
        neutralEmoji.SetActive(true);
    }
}