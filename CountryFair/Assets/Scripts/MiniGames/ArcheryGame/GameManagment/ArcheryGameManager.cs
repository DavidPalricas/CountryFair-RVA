using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

/// <summary>
/// Manages the Archery mini-game: spawns colored balloons inside the spawn area, applies DDA difficulty
/// curves for movement and transparency ratios, and randomly selects the scoring balloon color each round.
/// Inherits target lifecycle from <see cref="MiniGameManager"/>.
/// </summary>
public class ArcheryGameManager : MiniGameManager
{
    [Header("Game Specific References")]
    /// <summary>TMP text displaying the balloon color that scores; its text and color are set to match the target balloon.</summary>
    [SerializeField] private TextMeshProUGUI balloonColorToScoreText;
    /// <summary>Collider defining the 3D volume within which balloons are spawned.</summary>
    [SerializeField] private Collider balloonSpawnArea;

    [Header("Balloons Prefabs")]
    /// <summary>Prefab for a blue balloon.</summary>
    [SerializeField]
     private GameObject blueBalloonPrefab;
    /// <summary>Prefab for a red balloon.</summary>
    [SerializeField]
    private GameObject redBalloonPrefab;
    /// <summary>Prefab for a yellow balloon.</summary>
    [SerializeField]
    private GameObject yellowBalloonPrefab;

    [Header("Complexity Curves")]
    /// <summary>AnimationCurve controlling the fraction of balloons that move at a given difficulty saturation.</summary>
    [SerializeField]
    private AnimationCurve movingRatioCurve;
    /// <summary>AnimationCurve controlling the fraction of balloons that become transparent at a given difficulty saturation.</summary>
    [SerializeField]
    private AnimationCurve transparencyRatioCurve;

    [Header("Speed Progression")]
    /// <summary>Balloon movement duration in seconds at the easiest difficulty — larger value means slower movement.</summary>
    [Tooltip("Movement duration in lowest difficulty (Bigger is slower). Ex: 4s")]
    [SerializeField]
    private float slowMoveDuration = 6.0f;

    /// <summary>Balloon movement duration in seconds at the hardest difficulty — smaller value means faster movement.</summary>
    [Tooltip("Movement duration in highest difficulty (Smaller is faster). Ex: 1s" )]
    [SerializeField]
    private float fastMoveDuration = 1.5f;

    [Header("Color to Score Animation Settings")]
    /// <summary>Punch scale multiplier applied to the balloon-color text on each color-change animation.</summary>
    [SerializeField]
    private float punchScale = 1.2f;
    /// <summary>Duration in seconds of the DOPunchScale animation on the balloon-color text.</summary>
    [SerializeField]
    private float punchDuration = 0.5f;

    private Dictionary<GameObject, int> balloonTypesCount;
    private GameObject _balloonPrefabToScore;
    
    // Variáveis de Controlo
    private float _currentMovingRatio;
    private float _currentTransparencyRatio;
    private float _currentMoveDuration; 


    protected override void Awake()
    {   
        base.Awake(); 

        if (balloonColorToScoreText == null)
        {
            Debug.LogError("Balloon Color To Score Text reference is missing.");
            return;
        }

        if (balloonSpawnArea == null)
        {
            Debug.LogError("Balloon Spawn Area reference is missing.");
            return;
        }

        if (blueBalloonPrefab == null || yellowBalloonPrefab == null || redBalloonPrefab == null)
        {
            Debug.LogError("One or more balloon prefab references are missing.");
            return;
        }

        if (movingRatioCurve.length == 0 || transparencyRatioCurve.length == 0)
        {
            Debug.LogError("One or more difficulty curves are not set.");
            return;
        }

        balloonTypesCount = new Dictionary<GameObject, int>
        {
            { blueBalloonPrefab, 0 },
            { redBalloonPrefab, 0 },
            { yellowBalloonPrefab, 0 }
        };

          if (!GameManager.GetInstance().ArcheryTutorialCompleted)
        {
            PlayerPrefs.DeleteKey("ArcheryDifficultyLevel");
        }

        SetBalloonColorToScore();
    }

    public override void TutorialCompleted()
    {   
        difficultyLevel = PlayerPrefs.GetInt("ArcheryDifficultyLevel", 0);
        ApplyDifficultySettings();
    }

    public override void ChangeDifficulty(bool isToIncreaseDiff)
    {
        difficultyLevel = isToIncreaseDiff ? difficultyLevel + 1 : Mathf.Max(0, difficultyLevel - 1);
        // Debug.Log($"<color=orange>ARCHERY DDA:</color> Level {difficultyLevel}");
        ApplyDifficultySettings();
    }

    protected override void ApplyDifficultySettings()
    {    
        _currentDesiredCount = Mathf.RoundToInt(targetsCount + (difficultyLevel * targetsPerLevel));

        float saturationFactor = difficultyLevel / (difficultyLevel + 4f); 
        _currentMovingRatio = movingRatioCurve.Evaluate(saturationFactor);
        _currentTransparencyRatio = transparencyRatioCurve.Evaluate(saturationFactor);

        _currentMoveDuration = Mathf.Lerp(slowMoveDuration, fastMoveDuration, saturationFactor);

        PlayerPrefs.SetInt("ArcheryDifficultyLevel", difficultyLevel);
        
        // Debug.Log($"[Archery Stats] Total Balloons: {_currentDesiredCount} | Movimento Ratio: {_currentMovingRatio:P0} | Move Duration: {_currentMoveDuration:F1}s");
        
        SetBalloonColorToScore();
        SyncTargets(_currentDesiredCount);
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
        
        if (prefabToSpawn == null)
        {
            prefabToSpawn = GetBalloonType();
        }

        GameObject newBalloon = Instantiate(prefabToSpawn, pos, Quaternion.identity);

        BalloonArcheryGame balloonComponent = newBalloon.GetComponentInChildren<BalloonArcheryGame>();
        
        balloonComponent.OriginalPrefab = prefabToSpawn;
            
        _spawnedTargets.Add(newBalloon);
    }

    protected override void RemoveTarget()
    {
        if (_spawnedTargets.Count > 0)
        {
            GameObject target = _spawnedTargets[0];
 
            BalloonArcheryGame balloonComponent = target.GetComponentInChildren<BalloonArcheryGame>();

            DestroyTarget(target, balloonComponent.OriginalPrefab);
        }  
    }

    public override void DestroyTarget(GameObject target, GameObject targetPrefab)
    {   
        balloonTypesCount[targetPrefab] = Mathf.Max(0, balloonTypesCount[targetPrefab] - 1);
        
        _spawnedTargets.Remove(target);

        Destroy(target);

        if (_spawnedTargets.Count < _currentDesiredCount)
        {
            if (balloonTypesCount[_balloonPrefabToScore] == 0)
            {
                AddTarget(_balloonPrefabToScore);
            }
            else
            {
                AddTarget();
            }
            
            UpdateTargetsProperties();
        }
    }

    protected override void UpdateTargetsProperties()
    {
        int total = _spawnedTargets.Count;
        int targetMovingCount = Mathf.RoundToInt(total * _currentMovingRatio);
        int targetTransparentCount = Mathf.RoundToInt(total * _currentTransparencyRatio);

        List<GameObject> shuffled = _spawnedTargets.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < total; i++)
        {    
            BalloonArcheryGame balloonComponent = shuffled[i].GetComponentInChildren<BalloonArcheryGame>();

            balloonComponent.SetMoveDuration(_currentMoveDuration);

            balloonComponent.AdjustMovement(i < targetMovingCount);
            balloonComponent.AdjustTransparency(i < targetTransparentCount);
        }
    }

    protected override Vector3 GetRandomTargetPosition()
    {
        Bounds bounds = balloonSpawnArea.bounds;
        
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        float z = Random.Range(bounds.min.z, bounds.max.z);

        Vector3 candidatePos = new (x, y, z);

        const float SAFETEY_RADIUS = 0.3f;
            
        Collider[] hitColliders = Physics.OverlapSphere(candidatePos, SAFETEY_RADIUS);
        bool hitBalloon = false;

        foreach(Collider hit in hitColliders)
        {
            if(hit.CompareTag("Balloon")) 
            {
                hitBalloon = true; 
                break;
            }
        }

        return !hitBalloon ? candidatePos : GetRandomTargetPosition();
    }

    private void SetBalloonColorToScore()
    {   
        int randomColorIndex = Utils.RandomValueInRange(0, Enum.GetValues(typeof(BalloonArcheryGame.Colors)).Length);
        string colorToScore = ((BalloonArcheryGame.Colors)randomColorIndex).ToString().ToLower();

        switch (colorToScore)
        {
            case "red": 
                balloonColorToScoreText.text = "Vermelho"; 
                balloonColorToScoreText.color = Color.red;
                _balloonPrefabToScore = redBalloonPrefab; 
                break;

            case "blue": 
                balloonColorToScoreText.text = "Azul"; 
                balloonColorToScoreText.color = Color.blue;
                _balloonPrefabToScore = blueBalloonPrefab; 
                break;

            case "yellow": 
                balloonColorToScoreText.text = "Amarelo"; 
                balloonColorToScoreText.color = Color.yellow;
                _balloonPrefabToScore = yellowBalloonPrefab; 
                break;

             default:
                Debug.LogError("Invalid balloon color selected for scoring.");
                return;
        }

        balloonColorToScoreText.transform.DOKill();

        
        balloonColorToScoreText.transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 5, 0.5f);


        PlayerPrefs.SetString("BalloonColorToScore", colorToScore); 
    }

    private GameObject GetBalloonType()
    {   
        int minCount = balloonTypesCount.Min(typeCount => typeCount.Value);

        GameObject[] candidates = balloonTypesCount
            .Where(element => element.Value == minCount)
            .Select(element => element.Key)
            .ToArray();

        GameObject selectedBalloon;

        if (candidates.Contains(_balloonPrefabToScore))
        {
            selectedBalloon = _balloonPrefabToScore;
        }
        else
        {
            selectedBalloon = candidates[Utils.RandomValueInRange(0, candidates.Length)];
        }

        balloonTypesCount[selectedBalloon]++;
        return selectedBalloon;
    }

    public override void ResetDifficulty()
    {   
        if (difficultyLevel > 0)
        {
            difficultyLevel = 0;
            ApplyDifficultySettings();
        }
    }
}