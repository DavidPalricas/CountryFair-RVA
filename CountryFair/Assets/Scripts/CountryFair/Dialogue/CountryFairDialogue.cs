using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class CountryFairDialogue : UIDialog
{   
    [Header("Country Fair Settings")]
    [SerializeField] 
    private GameObject postIntroElements;
    [SerializeField] 
    private GameObject playerArea;

    
    [Header("Characters")]
    [SerializeField] private GameObject zeca;
    [SerializeField] private GameObject carnyWise;
    [SerializeField] private TextMeshProUGUI characterNameText;

    private IntroData _introData;
    private SessionCompletedData _sessionCompleteData;
    private List<string> _currentDialogueLines = null;

    private enum DialogueState
    {
        BEGIN_INTRO,
        ZECA_INTRO_PART1,
        ZECA_INTRO_PART2,
        CARNY_WISE_INTRO,
        INTRO_COMPLETED,
    }

    private DialogueState _currentDialogueState = DialogueState.BEGIN_INTRO;

    protected override void Awake()
    {   
        GameManager gameManager = GameManager.GetInstance();

        if (gameManager.IntroCompleted)
        {   
            _currentDialogueState = DialogueState.INTRO_COMPLETED;
            
            if (!gameManager.FrisbeeSessionCompleted && !gameManager.ArcherySessionCompleted)
            {   
                Destroy(transform.parent.gameObject);
                return;
            }
        }

        base.Awake();

        if (zeca == null || carnyWise == null || characterNameText == null )
        {
            Debug.LogError("Characters missing.");
            return;
        }
        carnyWise.SetActive(false);

        if (postIntroElements == null)
        {
            Debug.LogError("Game Tents missing.");
            return;
        }

        if (playerArea == null)
        {
            Debug.LogError("Player Area missing.");

            return ;
        }

        Destroy(playerArea);

        postIntroElements.SetActive(false);
    }

    protected override void OnDataLoaded()
    {
        if (_currentDialogueState != DialogueState.INTRO_COMPLETED)
        {
             if (_data is not IntroData introData)
            {
                Debug.LogError("Error Converting data to IntroData.");
                return;
            }

            _introData = introData;

            SetIntroCurrentState();
            ShowIntroLines(); 
        }
        else
        {
            if (_data is not SessionCompletedData sessionCompleteData)
            {
                Debug.LogError("Error Converting data to SessionCompletedData.");
                return;
            }

            _sessionCompleteData = sessionCompleteData;
            ShowSessionCompletedLine();
        }
    }

    public override void NextStep()
    {   
        if (_data == null) return;

        if (_currentDialogueState == DialogueState.INTRO_COMPLETED)
        {   
            postIntroElements.SetActive(true);
            Destroy(transform.parent.gameObject);
            return;
        }
       
        if (_currentDialogueLines.Count == 0)
        {
            SetIntroCurrentState();
        }

        ShowIntroLines();
    }


    private void SetIntroCurrentState()
    {  
        switch (_currentDialogueState)
        {
            case DialogueState.BEGIN_INTRO:
                _currentDialogueLines = _introData.ZecaPart1;
                characterNameText.text = "Zeca Bigodes";
                _currentDialogueState = DialogueState.ZECA_INTRO_PART1;
                zeca.SetActive(true);
                carnyWise.SetActive(false);
                return;

            case DialogueState.ZECA_INTRO_PART1:
                _currentDialogueLines = _introData.CarnyWise;
                characterNameText.text = "Carny Wise";
                _currentDialogueState = DialogueState.CARNY_WISE_INTRO;
                zeca.SetActive(false);
                carnyWise.SetActive(true);
                return;

            case DialogueState.CARNY_WISE_INTRO:
                _currentDialogueLines = _introData.ZecaPart2;
                characterNameText.text = "Zeca Bigodes";
                _currentDialogueState = DialogueState.ZECA_INTRO_PART2;
                zeca.SetActive(true);
                carnyWise.SetActive(false);
                return;
            case DialogueState.ZECA_INTRO_PART2:
                _currentDialogueState = DialogueState.INTRO_COMPLETED;
                IntroComplete();
                return;
            default:
                Debug.LogError("Invalid dialogue state.");
                return;
        }
    }

    public void IntroComplete()
    {   
        postIntroElements.SetActive(true);
        GameManager.GetInstance().IntroCompleted = true;

        Destroy(transform.parent.gameObject);
    }

    protected override System.Type GetJSONDataType()
    {   
        if (_currentDialogueState != DialogueState.INTRO_COMPLETED)
            return typeof(IntroData);

        return typeof(SessionCompletedData);
    }

    protected override void SetJSONFileName()
    {    
        if (_currentDialogueState != DialogueState.INTRO_COMPLETED)
        {
            _jsonFileName = "intro.json";
            return;
        }

        GameManager gameManager = GameManager.GetInstance();

        if (gameManager.FrisbeeSessionCompleted)
        {
            _jsonFileName = "frisbee_session_completed.json";
            gameManager.FrisbeeSessionCompleted = false;
            return;
        }

        if (gameManager.ArcherySessionCompleted)
        {
            _jsonFileName = "archery_session_completed.json";
            gameManager.ArcherySessionCompleted = false;
            return;
        }
        Debug.LogError("No session completed to show.");
    }

    private void ShowIntroLines()
    {   
        if (_currentDialogueState != DialogueState.INTRO_COMPLETED && _currentDialogueLines != null && _currentDialogueLines.Count > 0) 
        {
             dialogueBoxText.text = _currentDialogueLines[0];
            _currentDialogueLines.RemoveAt(0);
        }
    }

    private void ShowSessionCompletedLine()
    {   
        characterNameText.text = "Zeca Bigodes";

        if(_sessionCompleteData != null && _sessionCompleteData.Congrats != null)
        {
             dialogueBoxText.text = _sessionCompleteData.Congrats[Utils.RandomValueInRange(0, _sessionCompleteData.Congrats.Count)];
        }
           
    }
}