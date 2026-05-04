using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class Tutorial : UIDialog
{
    [SerializeField]
    private UnityEvent tutorialCompleted;

    [Header("Practise Elements")]
    [SerializeField]
    private GameObject videoScreen;
   
   [SerializeField]
    private GameObject  miniGameVideo;

    [SerializeField]
    private GameObject miniGameProp;


    [SerializeField]
    private GameObject practiceElements;

    [Header("Game Elements")]
    [SerializeField]
    private GameObject tutorialButton;

    [SerializeField]
    private GameObject postTutorialElements;

    private int _numberOfTasks;
    private TutorialData _tutorialData;
    private int _currentTasksCompleted = 0;
    private bool _finishedPracticing = false;
    private bool _isFromFrisbeGame = false;

    protected override void Awake()
    {
        if (TutorialWasCompleted())
        {
            tutorialCompleted.Invoke();
            Destroy(gameObject);
            return;
        }

        base.Awake();

        if (practiceElements == null || postTutorialElements == null)
        {
            Debug.LogError("Practice or Post elements are not assigned in the inspector.");
            return;
        }

        if (tutorialButton == null)
        {
            Debug.LogError("Tutorial button is not assigned in the inspector.");
            return;
        }

        if (miniGameProp == null)
        {
            Debug.LogError("Mini game prop is not assigned in the inspector.");
            return;
        }

        if (videoScreen == null || miniGameVideo == null)
        {
            Debug.LogError("Video screen or mini game video is not assigned in the inspector.");
            return;
        }

        _numberOfTasks = Utils.GetChildren(practiceElements.transform).Length;

        practiceElements.SetActive(false);
        miniGameProp.SetActive(false);
        postTutorialElements.SetActive(false);
        videoScreen.SetActive(false);
    }

    protected override void OnDataLoaded()
    {
        if (_data is not TutorialData tutorialData)
        {
            Debug.LogError("Error Converting data to TutorialData.");
            return;
        }

        _tutorialData = tutorialData;

        ShowGameRule();
    }

    private void CheckCurrenMiniGame()
    {
        string sceneName = SceneManager.GetActiveScene().name.ToLower();

        if (sceneName.Contains("frisbee"))
        {
            _isFromFrisbeGame = true;
            return;
        }

        if (sceneName.Contains("archery"))
        {
            _isFromFrisbeGame = false;
            return;
        }

        Debug.LogError("Invalid scene for tutorial detection.");
    }

    private bool TutorialWasCompleted()
    {
        CheckCurrenMiniGame();

        GameManager gameManager = GameManager.GetInstance();

        // If in the development the developer starts testing on a mini game scene without going through the fair intro, to make sure when return to te fair does not have the intro dialogue this flag is set to true.
        gameManager.IntroCompleted = true;

        if (_isFromFrisbeGame && gameManager.FrisbeeTutorialCompleted)
        {
            return true;
        }

        if (!_isFromFrisbeGame && gameManager.ArcheryTutorialCompleted)
        {
            return true;
        }

        return false;
    }

    protected override System.Type GetJSONDataType()
    {
        return typeof(TutorialData);
    }

    protected override void SetJSONFileName()
    {
        // Usa o bool definido no CheckCurrenMiniGame (chamado no início do Awake)
        _jsonFileName = _isFromFrisbeGame ? "frisbee_tutorial.json" : "archery_tutorial.json";
    }

    public override void NextStep()
    {
        if (_tutorialData == null){
             return;
        }

        if (_finishedPracticing)
        {
            ReadyToPlay();
            return;
        }

        ShowGameRule();
    }

    public void ReadyToPlay()
    {
        miniGameProp.SetActive(true);
        postTutorialElements.SetActive(true);
        tutorialCompleted.Invoke();

        if (_isFromFrisbeGame)
        {
            GameManager.GetInstance().FrisbeeTutorialCompleted = true;
        }
        else
        {
            GameManager.GetInstance().ArcheryTutorialCompleted = true;
        }

        Destroy(gameObject);
    }

    private void ShowGameRule()
    {
        List<string> rules = _tutorialData.Rules;

        if (rules.Count == 0)
        {
            StartPractice();
            return;
        }

        dialogueBoxText.text = rules[0];
        _tutorialData.Rules.RemoveAt(0);
    }

    private void StartPractice()
    {
        tutorialButton.SetActive(false);
        practiceElements.SetActive(true);
        miniGameProp.SetActive(true);
        videoScreen.SetActive(true);
        miniGameVideo.SetActive(true);

        dialogueBoxText.text = _tutorialData.Guide;
    }

    private void PractiseCompleted()
    {
        dialogueBoxText.text = _tutorialData.End;

        tutorialButton.SetActive(true);
        miniGameProp.SetActive(false);
        videoScreen.SetActive(false);
        _finishedPracticing = true;
    }

    public void TaskCompleted()
    {
        _currentTasksCompleted++;

        if (_currentTasksCompleted >= _numberOfTasks)
        {
            PractiseCompleted();
        }
    }
}