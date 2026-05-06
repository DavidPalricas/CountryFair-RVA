using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the Frisbee mini-game: spawns score areas around the player, applies DDA difficulty settings
/// for dog distance, target count, movement ratio, and visibility ratio.
/// Inherits target lifecycle from <see cref="MiniGameManager"/>.
/// </summary>
public class FrisbeeGameManager : MiniGameManager
{
    [Header("Dog Settings")]
    /// <summary>Distance in meters added to the baseline dog distance per difficulty level.</summary>
    [SerializeField]
    private float distanceIncrement = 2.5f;

    /// <summary>Maximum dog distance cap in meters, regardless of difficulty level.</summary>
    [SerializeField]
    private float maxDogDistance = 20f;

    [Header("Complexity Curves")]
    /// <summary>AnimationCurve controlling the fraction of score areas that move at a given difficulty saturation.</summary>
    [SerializeField]
    private AnimationCurve movingRatioCurve;
    /// <summary>AnimationCurve controlling the fraction of score areas that blink at a given difficulty saturation.</summary>
    [SerializeField]
    private AnimationCurve visibilityRatioCurve;

    [Header("Game Specific References")]
    /// <summary>Prefab instantiated for each score area target.</summary>
    [SerializeField]
    private GameObject scoreAreaPrefab;
    /// <summary>Collider defining the valid spawn region for score areas around the player.</summary>
    [SerializeField]
    private Collider dogAreaCollider;

    /// <summary>Collider marking the elevated dog-score area; score area spawns are excluded from this zone.</summary>
    [SerializeField]
    private Collider dogScoreAreaCollider;

    /// <summary>Player transform used to calculate dog distance and score-area placement relative to the player.</summary>
    [SerializeField]
    private Transform playerTransform;

    /// <summary>Fired when difficulty changes (after the first level), signaling the dog AI to reposition.</summary>
    [SerializeField]
    private UnityEvent difficultyHasChanged;
    
    // Variáveis para spawn
    private float _currentMovingRatio;
    private float _currentVisibilityRatio;

    private float _scoreAreaRadius = 0;

    private float _defaultDogDistance;


    protected override void Awake()
    {
        base.Awake();
  
        if (playerTransform == null)
        {
            Debug.LogError("Player not found. The rehabilitation system cannot start.");
            return;
        }

        if (movingRatioCurve.length == 0 || visibilityRatioCurve.length == 0)
        {
            Debug.LogError("One or more animation curves are not assigned in the inspector.");
        }

        _scoreAreaRadius = scoreAreaPrefab.GetComponent<Renderer>().bounds.extents.magnitude;

        if (!GameManager.GetInstance().FrisbeeTutorialCompleted)
        {
            PlayerPrefs.DeleteKey("FrisbeeDifficultyLevel");
        }
    }

    public override void TutorialCompleted()
    {   
        _defaultDogDistance =  GetPlayerDistanceToDog();
        
        difficultyLevel = PlayerPrefs.GetInt("FrisbeeDifficultyLevel", 0);

        Debug.Log("Current Difficulty Level: " + difficultyLevel);

        ApplyDifficultySettings();
    }

    public override void ChangeDifficulty(bool isToIncreaseDiff)
    {   
        if (difficultyLevel > 0)
        {   
            difficultyHasChanged.Invoke();
        }

        difficultyLevel = isToIncreaseDiff ? difficultyLevel + 1 : Mathf.Max(0, difficultyLevel - 1);
        // Debug.Log($"<color=blue>DDA:</color> Adjusting difficulty to Level {difficultyLevel} ({(isToIncreaseDiff ? "Increased" : "Decreased")})");
        ApplyDifficultySettings();

    }


    protected override void ApplyDifficultySettings()
    {
        float rawDistance = _defaultDogDistance + (difficultyLevel * distanceIncrement);
        float finalDogDistance = Mathf.Min(rawDistance, maxDogDistance);
        
        PlayerPrefs.SetFloat("DogDistance", finalDogDistance);

        PlayerPrefs.SetInt("FrisbeeDifficultyLevel", difficultyLevel);

        _currentDesiredCount = Mathf.RoundToInt(targetsCount + (difficultyLevel * targetsPerLevel));

        float saturationFactor = difficultyLevel / (difficultyLevel + 4f);

        _currentMovingRatio = movingRatioCurve.Evaluate(saturationFactor);
        _currentVisibilityRatio = visibilityRatioCurve.Evaluate(saturationFactor);

        // Debug.Log($"[DDA] Level {difficultyLevel} | Dist: {finalDogDistance:F1}m | Targets: {_currentDesiredCount} | Complexity: {saturationFactor:P0}");
    }

    protected override void SyncTargets(int desiredCount)
    {
        while (_spawnedTargets.Count < desiredCount)
        {
            AddTarget();
        }
        
        while (_spawnedTargets.Count > desiredCount)
        {
            RemoveTarget();
        }

        UpdateTargetsProperties();
    }

    protected override void AddTarget(GameObject prefabToSpawn = null)
    {
        Vector3 pos = GetRandomTargetPosition();
        GameObject newScoreArea = Instantiate(scoreAreaPrefab, pos, Quaternion.identity);
        _spawnedTargets.Add(newScoreArea);
    }

    protected override void RemoveTarget()
    {
        if (_spawnedTargets.Count > 0)
        {
            GameObject target = _spawnedTargets[0];
            _spawnedTargets.RemoveAt(0);
            Destroy(target);
        }
    }

    protected override void UpdateTargetsProperties()
    {
        int total = _spawnedTargets.Count;
        int targetMovingCount = Mathf.RoundToInt(total * _currentMovingRatio);
        int targetVisibleCount = Mathf.RoundToInt(total * _currentVisibilityRatio);

        List<GameObject> shuffled = _spawnedTargets.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < total; i++)
        {   
            if (shuffled[i].TryGetComponent<ScoreAreaProperties>(out var scoreArea))
            {
                scoreArea.AdjustMovement( i < targetMovingCount);

                scoreArea.AdjustVisibility(i < targetVisibleCount); 
            }
        }
    }

    protected override Vector3 GetRandomTargetPosition()
    {
        float currentDogDist = PlayerPrefs.GetFloat("DogDistance", _defaultDogDistance);
        
        const float MIN_OFFSET = 0.2f;
        const float MAX_OFFSET = 1f;

        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        float scoreAreaDistance = currentDogDist * Random.Range(MIN_OFFSET, MAX_OFFSET);

        Vector3 scoreAreaPosition = playerTransform.position + new Vector3(randomDirection.x, 0, randomDirection.y) * scoreAreaDistance;

        scoreAreaPosition.y = dogAreaCollider.bounds.center.y;

        int layerMask = LayerMask.GetMask("ScoreArea");
        
        bool collidesWithOtherAreas = Physics.CheckSphere(scoreAreaPosition, _scoreAreaRadius, layerMask);

        if (!dogAreaCollider.bounds.Contains(scoreAreaPosition) || collidesWithOtherAreas)
        {
            return GetRandomTargetPosition();
        }

        return scoreAreaPosition;
    }
    

    public override void DestroyTarget(GameObject target, GameObject targetPrefab = null)
    {
        _spawnedTargets.Remove(target);
        Destroy(target);

        UpdateTargetsProperties();
    }


    /// <summary>
    /// Spawns or removes score areas to match the current desired count for the active difficulty level.
    /// </summary>
    /// <remarks>Invocado via Inspector pelo evento frisbeeScored do Dog AI quando uma nova ronda começa.</remarks>
    public void SpawnScoreAreas()
    {
        SyncTargets(_currentDesiredCount);
    }

    /// <summary>
    /// Destroys all active score areas and clears the spawned-targets list.
    /// </summary>
    /// <remarks>Invocado via Inspector pelo evento difficultyHasChanged para limpar áreas antes de um respawn.</remarks>
    public void RemoveScoreAreas()
    {
       if (_spawnedTargets.Count > 0)
        {
            foreach (GameObject target in _spawnedTargets)
            {
                Destroy(target);
            }

            _spawnedTargets.Clear();
        }
    }

    private float GetPlayerDistanceToDog()
    {
        GameObject dog = GameObject.FindGameObjectWithTag("Dog");

        if (dog == null)
        {
            Debug.LogError("Dog not found in the scene.");
            return 0f;
        }

        return Vector3.Distance(playerTransform.position, dog.transform.position);
    }


    public override void ResetDifficulty()
    {      
        difficultyHasChanged.Invoke();
        base.ResetDifficulty();
    }
}