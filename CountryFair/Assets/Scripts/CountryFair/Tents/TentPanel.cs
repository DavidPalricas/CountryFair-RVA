using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TentPanel : OrderableTentElement
{   
    [SerializeField]
    public TextMeshProUGUI tentNameText = null;
    
    [SerializeField]
    private TextMeshProUGUI tentNumberText = null;

    [SerializeField]
    private Image tentImage = null;

    [SerializeField]
    private Sprite miniGameSprite = null;

    protected override void Awake()
    {   
        base.Awake();

        if (tentNameText == null || tentNumberText == null)
        {
            Debug.LogError("One or more Texts Components are null in TentPanel");

            return;
        }

        if (tentImage == null)
        {
            Debug.LogError("Tent Image Component is null in TentPanel");

            return;
        }

        if (miniGameSprite == null)
        {
            Debug.LogError("MiniGame Sprite is null in TentPanel");

            return;
        }

        tentImage.sprite = miniGameSprite;

        tentNumberText.text = currentPlaceHolder.number.ToString();

        SetTentName();
    }


    private void SetTentName()
    {   
        string tentName = "Tenda de ";

        switch (miniGame)
        {   
            case MINI_GAMES.FISHING:
                tentName += " pesca";
                break;

            case MINI_GAMES.ARCHERY:
                tentName += " arco e flecha";
                break;

            case MINI_GAMES.FRISBEE:
                tentName += "frisbee";
                break;

            case MINI_GAMES.DUCKGAME:
                tentName += "jogo do pato";
                break;

            default:
                Debug.LogError("Invalide MiniGame");
                return;
        }

        tentNameText.text = tentName;
    }

     public override void SnapToCurrentPlaceHolder()
    {
        base.SnapToCurrentPlaceHolder();

        tentNumberText.text = currentPlaceHolder.number.ToString();
    }

    public override void HandleGrab(bool isGrabbed)
    { 
        if (!isGrabbed)
        {
            StartCoroutine(SnapToPlaceHolderNextFixedUpdate());

            return;
        }

       OnElementSelectionChanged.Invoke(true, this);
  }
}
