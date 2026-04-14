using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System;


public class SliderDisplay: EmotionDisplay
{   [Header("Slider Configuration")]
    [SerializeField]
    private Slider slider = null;
    
    [SerializeField]
    private Image fillImage = null;
    
    [SerializeField]
    private Image handleImage = null;

    
    [Header("Slider Colors")]
    [SerializeField]
    private Color positiveColor = Color.green;


    [SerializeField]
    private Color neutralColor = Color.yellow;


    [SerializeField]
    private Color negativeColor = Color.red;

    [Header("Emoji Sprites")]
    [SerializeField]
    private Sprite neutralEmoji = null;

    [SerializeField]
    private Sprite happyEmoji = null;

    [SerializeField]
    private Sprite angryEmoji = null;


    [Header("Emojis State Thresholds")]
    [SerializeField]
    [Range(0, 1)]
    private float positiveThreshold = 0.6f;

    [SerializeField]
    [Range(0, 1)]
    private float negativeThreshold = 0.4f;

    [SerializeField]
    [Range(0, 1)]
    private float sliderChangeAmountThreshold = 0.05f;

     private readonly NetworkVariable<EMOJI_CATEGORY> _netSliderState = new(
        EMOJI_CATEGORY.NEUTRAL,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public enum EMOJI_CATEGORY
    {
        NEUTRAL,
        POSITIVE,
        NEGATIVE,
    }

    private void Awake()
    {  
        if (slider == null ||  fillImage  == null)
        {
            Debug.LogError("Slider and/or its fill image is not reference is not assigned in EmojiSlider.");
            return;
        }

        if (handleImage == null)
        {
            Debug.LogError("Handle Image reference is not assigned in EmojiSlider.");
            return;
        }

        if (neutralEmoji == null || happyEmoji == null || angryEmoji == null)
        {
            Debug.LogError("One or more emoji GameObjects are not assigned in EmojiSlider.");
            return;
        }

       

        StartAsNeutral();

    }

    
    public override void OnNetworkSpawn()
    {
        _netSliderState.OnValueChanged += OnCategoryChanged;
        
        // Force initial visual update without triggering send logic
        _isUpdatingFromNetwork = true;
        UpdateSlider(_netSliderState.Value);
        _isUpdatingFromNetwork = false;
    }

      /// <summary>
    /// Called when the object is despawned from the network.
    /// Unsubscribes from state changes.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        _netSliderState.OnValueChanged -= OnCategoryChanged;
    }

    
    private void OnCategoryChanged(EMOJI_CATEGORY previous, EMOJI_CATEGORY current)
    {
        _isUpdatingFromNetwork = true;
       UpdateSlider(current);
        _isUpdatingFromNetwork = false;
    }


    public void UpdateSlider(EMOJI_CATEGORY newCategory)
    {   
       string categoryName = newCategory.ToString();

        if (!Enum.TryParse(categoryName.ToUpper(), out EMOJI_CATEGORY category))
        {
            Debug.LogError($"Invalid emoji category: {categoryName}");
            return;
        }

        Debug.Log($"Updating slider with category: {category}");

        if (category == EMOJI_CATEGORY.POSITIVE)
        {
             slider.value = Mathf.Min(slider.value + sliderChangeAmountThreshold, 1);

        }else if(category == EMOJI_CATEGORY.NEGATIVE){
             slider.value = Mathf.Max(slider.value - sliderChangeAmountThreshold, 0);
        }

        UpdateEmojiVisual();


         // 2. NETWORK LOGIC (Only executes if NOT updating via network callback)
        if (!_isUpdatingFromNetwork)
        {
            SyncToNetwork(category);
        }
    }
   
    private void StartAsNeutral()
    {
        handleImage.sprite = neutralEmoji;
        slider.value = 0.5f;
        fillImage.color = neutralColor;

    }

    private void UpdateEmojiVisual()
    {  
        if (slider.value >= positiveThreshold)
        {
            handleImage.sprite = happyEmoji;
         
            fillImage.color = positiveColor;
         

            return;
        }

        if (slider.value <= negativeThreshold)
        {
            handleImage.sprite = angryEmoji;
            fillImage.color = negativeColor;

            return;
        }

            handleImage.sprite = neutralEmoji;
        fillImage.color = neutralColor;
    }


    public override void ProcessServerString(string message)
    {
        string emotionCategory = message.Split(_messageSeparator)[1];
        
        if (Enum.TryParse(emotionCategory, true, out EMOJI_CATEGORY result))
        {
           UpdateSlider(result); 
        }
    }


    private void SyncToNetwork(EMOJI_CATEGORY category)
    {
        // Checks if NetworkManager exists and is running
        if (NetworkManager.Singleton == null || (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer))
        {
            return; // We are offline (test mode), do nothing on network
        }

        // Avoid redundant sending if the value is already the same
        if (_netSliderState.Value == category) return;

        if (IsServer)
        {
            _netSliderState.Value = category;

            return;
        }

        if (IsOwner) // If it's a local cheat by the owner
        {
            RequestDisplayChangeServerRpc(category);
        }
    }

   [ServerRpc]
   private void RequestDisplayChangeServerRpc(EMOJI_CATEGORY category)
    {
        _netSliderState.Value = category;
    }
}