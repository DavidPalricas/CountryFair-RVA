using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Dynamic Difficulty Adjustment (DDA) system that monitors player performance and
/// fires difficulty change events based on precision thresholds and consecutive-attempt counters.
/// </summary>
/// <remarks>
/// Precision per task = 1 / attempts_until_score. Three consecutive high-precision tasks trigger
/// an increase; three consecutive low-precision tasks trigger a decrease. If the player misses
/// <see cref="failingConsecutiveThreshold"/> times in a row, difficulty drops immediately.
/// At session end, <see cref="SessionGoalReached"/> marks the session complete in <see cref="GameManager"/>
/// and returns to the hub scene.
/// </remarks>
[RequireComponent(typeof(CarnyWiseDiffFeedback))]
public class CarnyWise : MonoBehaviour
{
    /// <summary>Wired in Inspector to <see cref="MiniGameManager.ChangeDifficulty"/>.</summary>
    [SerializeField]
    private UnityEvent <bool> changeDifficulty;

    [Header("Adaptation Configuration")]
    /// <summary>Precision ratio at or above which a task counts as excelling (triggers potential increase).</summary>
    [SerializeField, Range(0f, 1f)]
    private float precisionThresholdToIncreaseDiff = 0.7f;

    /// <summary>Precision ratio at or below which a task counts as struggling (triggers potential decrease).</summary>
    [SerializeField, Range(0f, 1f)]
    private float precisionThresholdToDecreaseDiff = 0.3f;

    /// <summary>Number of consecutive excelling or struggling tasks required to change difficulty.</summary>
    [SerializeField]
    private int thresholdToChangeDiff = 3;

    /// <summary>Number of consecutive misses that immediately trigger a difficulty decrease.</summary>
    [SerializeField]
    private int failingConsecutiveThreshold = 4;

    // Variáveis de Estado
    private float _taskStartTime = 0f;
    private int _currentAttemptsForTask = 0; 
    
    // Contadores de buffer
    private int _struggleCounter = 0;
    private int _excelCounter = 0;

    private readonly string _sessionID = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

    private readonly List<float> _tasksTimes = new ();
    private readonly List<float> _tasksPrecisions = new ();

    private bool _is_frisbeeMiniGame = false;

    private CarnyWiseDiffFeedback _diffFeedback;



    private void Awake()
    {
        SetMiniGameName();
        _diffFeedback = GetComponent<CarnyWiseDiffFeedback>();
    }


    private void SetMiniGameName()
    {
        string sceneName = SceneManager.GetActiveScene().name.ToLower();

        if (sceneName.Contains("frisbee"))
        {
            _is_frisbeeMiniGame = true;

             return;
        }

        if (sceneName.Contains("archery"))
        {
            _is_frisbeeMiniGame = false;

            return;
        }
      
        Debug.LogError("Invalid scene"); 
    }

    /// <summary>
    /// Starts the per-task completion timer. Safe to call multiple times; only the first call sets the start time.
    /// </summary>
    /// <remarks>Invocado via Inspector quando o jogador começa uma tarefa (ex: ao ativar uma score area).</remarks>
    public void StartTaskTimer()
    {   // If the timer already started, do not reset
        if (_taskStartTime <= 0f)
        {
             _taskStartTime = Time.time;
        }
    }

    /// <summary>
    /// Records a miss attempt. If consecutive misses exceed <see cref="failingConsecutiveThreshold"/>, immediately decreases difficulty.
    /// </summary>
    /// <remarks>Invocado via Inspector pelo evento playerMissed do mini-jogo.</remarks>
    public void PlayerMissed()
    {
        _currentAttemptsForTask++;

        if (_currentAttemptsForTask >= failingConsecutiveThreshold)
        {
            DecreaseDifficulty();

            ResetTask();
        }
    }

    /// <summary>
    /// Records a successful score, calculates task precision, and evaluates whether to change difficulty.
    /// </summary>
    /// <remarks>Invocado via Inspector pelo evento playerScored do mini-jogo.</remarks>
    public void PlayerScored()
    {
        _currentAttemptsForTask++;

        // Debug.Log($"Player Scored! Current Attempts: {_currentAttemptsForTask}");
        float timeTaken = Time.time - _taskStartTime;

        const int SCORE_ATTEMPT = 1; 

        float taskPrecision = SCORE_ATTEMPT / (float)_currentAttemptsForTask; 

        // Debug.Log("Task Precision: " + taskPrecision);

        _tasksPrecisions.Add(taskPrecision);
        _tasksTimes.Add(timeTaken);

        ResetTask();

        EvaluatePerformance(taskPrecision, timeTaken);
    }


    private void ResetTask()
    {
        _taskStartTime = 0f;
        _currentAttemptsForTask = 0; 
    }

    private void EvaluatePerformance(float precision, float timeTaken)
    {
        if (precision >= precisionThresholdToIncreaseDiff)
        {
            _excelCounter++;

            _struggleCounter = Mathf.Max(0, _struggleCounter - 1);

            CheckToChangeDifficulty();
            return;
        }

        if (precision <= precisionThresholdToDecreaseDiff)
        {
            _struggleCounter++;

            _excelCounter = Mathf.Max(0, _excelCounter - 1);

            CheckToChangeDifficulty();

            return;
        }

        MantainFlowState();
    }


    private void MantainFlowState()
    {   
       // Debug.Log("Mantaining Flow State");

        _struggleCounter = Mathf.Max(0, _struggleCounter - 1);
        _excelCounter = Mathf.Max(0, _excelCounter - 1);
    }

    private void CheckToChangeDifficulty()
    {  
        if (_excelCounter >= thresholdToChangeDiff)
        {
            IncreaseDifficulty();
            return;
        }

        if (_struggleCounter >= thresholdToChangeDiff)
        {
            DecreaseDifficulty();
        }
    }

    public void IncreaseDifficulty()
    {
        const bool IS_TO_INCREASE_DIFF = true;


        changeDifficulty.Invoke(IS_TO_INCREASE_DIFF);
       

        _diffFeedback.ShowNewDiffFeedback(IS_TO_INCREASE_DIFF);

        _excelCounter = 0; 
    }

    public void DecreaseDifficulty()
    {   
        string playerPrefsDiffKey = _is_frisbeeMiniGame ? "FrisbeeDifficultyLevel" : "ArcheryDifficultyLevel";
        
        int currentDifficulty = PlayerPrefs.GetInt(playerPrefsDiffKey, 0);

        const bool IS_TO_INCREASE_DIFF = false;
        
        if (currentDifficulty > 0)
        {
            _diffFeedback.ShowNewDiffFeedback(IS_TO_INCREASE_DIFF);
        }

        changeDifficulty.Invoke(IS_TO_INCREASE_DIFF);

        _struggleCounter = 0; 
    }


    /// <summary>
    /// Called when the player reaches the session score goal.
    /// Flags the session as complete in <see cref="GameManager"/> and loads the hub scene after a short delay.
    /// </summary>
    /// <remarks>Invocado via Inspector pelo evento sessionGoalReached do <see cref="ScoreAndStreakSystem"/>.</remarks>
    public void SessionGoalReached()
    {
       // SaveSessionData();

        Invoke(nameof(ReturnToFair), 1f);     
    }

    private void ReturnToFair()
    {   
        GameManager gameManager = GameManager.GetInstance();

        if (_is_frisbeeMiniGame)
        {
            gameManager.FrisbeeSessionCompleted = true;
        }
        else
        {
            gameManager.ArcherySessionCompleted = true;
        }

        SceneManager.LoadScene("CountryFair");
    }
     

     /*
    private void SaveSessionData()
    {
       int averageTaskPrecision = (int)System.Math.Round(_tasksPrecisions.Average() * 100);

       int averageTaskTime = (int)System.Math.Round(_tasksTimes.Average());

       SessionData sessionData = new()
       {
           SessionGoal = $"{_tasksPrecisions.Count} pontos",
           AverageTaskPrecision = $"{averageTaskPrecision} %",
           AverageTaskCompletionTime = $"{averageTaskTime} segundos"
       };

       DataFileManager.GetInstance().SaveSessionData(sessionData, _sessionID, _miniGameName);
    }
    */
}