using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TentPanel : MonoBehaviour
{   
    [SerializeField]
    public TextMeshProUGUI tentNameText = null;
    
    [SerializeField]
    private TextMeshProUGUI tentNumberText = null;

    [SerializeField]
    private Image tentImage = null;


    public int placeHoldernumber = 1;


    private void Awake()
    {
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

        tentNumberText.text = placeHoldernumber.ToString();
    }


    public void UpdateTentAtributes(MiniGameTent.MINI_GAMES miniGame, Sprite tentImage)
    {   
        string tentName = "Tenda de ";

        switch (miniGame)
        {   
            case MiniGameTent.MINI_GAMES.FISHING:
                tentName += " pesca";
                break;

            case MiniGameTent.MINI_GAMES.ARCHERY:
                tentName += " arco e flecha";
                break;

            case MiniGameTent.MINI_GAMES.FRISBEE:
                tentName += "frisbee";
                break;

            case MiniGameTent.MINI_GAMES.DUCK:
                tentName += "jogo do pato";
                break;

            default:
                Debug.LogError("Invalide MiniGame");
                return;
        }

        tentNameText.text = tentName;

       this.tentImage.sprite = tentImage;

        Debug.Log("Boas");
    }
}
