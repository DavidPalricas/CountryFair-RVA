using TMPro;
using UnityEngine;

public class TentPanel : MonoBehaviour
{   
    [SerializeField]
    public TextMeshProUGUI tentNameText = null;
    
    [SerializeField]
    private TextMeshProUGUI tentNumberText = null;


    public int number = 1;


    private void Awake()
    {
        if (tentNameText == null || tentNumberText == null)
        {
            Debug.LogError("One or more Texts Components are null in TentPanel");

            return;
        }

        tentNumberText.text = number.ToString();
    }


    public void UpdateTentName(MiniGameTent.MINI_GAMES miniGame)
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

        Debug.Log("Boas");
    }
}
