using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

/// <summary>
/// Drives the mini-game tutorial flow: presents rule slides from JSON, launches a practice phase,
/// and fires <c>tutorialCompleted</c> when the player is ready to play for real.
/// Skips itself immediately if <see cref="GameManager"/> reports the tutorial was already completed.
/// </summary>
public class Tutorial : UIDialog
{
    /// <summary>Fired when the tutorial ends — wired to <see cref="MiniGameManager.TutorialCompleted"/>.</summary>
    [SerializeField]
    private UnityEvent tutorialCompleted;

    [Header("Practise Elements")]
    /// <summary>Video screen container shown during the practice phase.</summary>
    [SerializeField]
    private GameObject videoScreen;

    /// <summary>Video player object shown on the screen during practice.</summary>
    [SerializeField]
    private GameObject  miniGameVideo;

    /// <summary>Mini-game prop (bow or frisbee) revealed at the start of the practice phase.</summary>
    [SerializeField]
    private GameObject miniGameProp;

    /// <summary>Root container that holds all interactive practice score areas.</summary>
    [SerializeField]
    private GameObject practiceElements;

    [Header("Game Elements")]
    /// <summary>Button the player presses to advance through rule slides and to confirm practice completion.</summary>
    [SerializeField]
    private GameObject tutorialButton;

    /// <summary>UI elements activated after the tutorial completes (e.g. score display).</summary>
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

    /// <summary>
    /// Advances to the next rule slide; transitions to the "ready to play" confirmation once practice ends.
    /// </summary>
    /// <remarks>Invocado via Inspector pelo botão de avanço no ecrã de tutorial.</remarks>
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

    /// <summary>
    /// Activates the mini-game prop and post-tutorial UI, fires <c>tutorialCompleted</c>, marks the tutorial
    /// complete in <see cref="GameManager"/>, and destroys this tutorial object.
    /// </summary>
    /// <remarks>Invocado via Inspector pelo botão "Estou Pronto" no final do tutorial.</remarks>
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

    /// <summary>
    /// Records one completed practice task; starts the end-of-practice dialogue when all tasks are done.
    /// </summary>
    /// <remarks>Invocado via Inspector pelos eventos taskCompleted das score areas de prática.</remarks>
    public void TaskCompleted()
    {
        _currentTasksCompleted++;

        if (_currentTasksCompleted >= _numberOfTasks)
        {
            PractiseCompleted();
        }
    }
}