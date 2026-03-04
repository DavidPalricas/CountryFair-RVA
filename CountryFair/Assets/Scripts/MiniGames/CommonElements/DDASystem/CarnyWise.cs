using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CarnyWiseDiffFeedback))]
public class CarnyWise : MonoBehaviour
{
    [SerializeField]
    private UnityEvent <bool> changeDifficulty;

    [Header("Adaptation Configuration")] 
    [SerializeField, Range(0f, 1f)]
    private float precisionThresholdToIncreaseDiff = 0.7f; 

    [SerializeField, Range(0f, 1f)]
    private float precisionThresholdToDecreaseDiff = 0.3f; 

    [SerializeField]
    private int thresholdToChangeDiff = 3; 

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

    public void StartTaskTimer()
    {   // If the timer already started, do not reset
        if (_taskStartTime <= 0f)
        {
             _taskStartTime = Time.time;
        }
    }

    public void PlayerMissed()
    {
        _currentAttemptsForTask++;

        if (_currentAttemptsForTask >= failingConsecutiveThreshold)
        {
            DecreaseDifficulty();

            ResetTask();
        }
    }

    public void PlayerScored()
    {   
        _currentAttemptsForTask++;

        Debug.Log($"Player Scored! Current Attempts: {_currentAttemptsForTask}");
        float timeTaken = Time.time - _taskStartTime;

        const int SCORE_ATTEMPT = 1; 

        float taskPrecision = SCORE_ATTEMPT / (float)_currentAttemptsForTask; 

        Debug.Log("Task Precision: " + taskPrecision);

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

            _struggleCounter = Mathf.Max(0, _struggleCounter--);

            CheckToChangeDifficulty();
            return;
        }

        if (precision <= precisionThresholdToDecreaseDiff)
        {
            _struggleCounter++;

            _excelCounter = Mathf.Max(0, _excelCounter--);

            CheckToChangeDifficulty();

            return;
        }

        MantainFlowState();
    }


    private void MantainFlowState()
    {   
        Debug.Log("Mantaining Flow State");

        _struggleCounter = Mathf.Max(0, _struggleCounter--);
        _excelCounter = Mathf.Max(0, _excelCounter--);
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