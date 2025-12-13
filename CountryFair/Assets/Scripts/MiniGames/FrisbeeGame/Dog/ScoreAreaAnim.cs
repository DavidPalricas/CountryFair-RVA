using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Handles the animation of the score area including floating, rotating, and pulsing effects.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ScoreAreaAnim : MonoBehaviour
{
    [Header("Rotate Animation Settings")]
    /// <summary>
    /// Duration of one cycle of the floating animation.
    /// </summary>
    [SerializeField]
     private float floatDuration = 2f;

    /// <summary>
    /// Duration of one full rotation cycle.
    /// </summary>
    [SerializeField]
     private float rotationDuration = 3f;

    /// <summary>
    /// Duration of one cycle of the pulse scaling animation.
    /// </summary>
    [SerializeField]
     private float pulseDuration = 1.5f;

    /// <summary>
    /// Maximum scale multiplier for the pulse animation.
    /// </summary>
    [SerializeField]
     private float pulseScale = 1.2f;

    /// <summary>
    /// Cached starting position used as baseline for floating animation.
    /// </summary>
    private Vector3 _startPosition;

    /// <summary>
    /// Cached starting scale used as baseline for pulse animation.
    /// </summary>
    private Vector3 _startScale;

    /// <summary>
    /// DOTween sequence controlling all active animations.
    /// </summary>
    private Sequence _animSequence;


    [Header("Score Animation Settings")]
    /// <summary>
    /// Prefab for the blue balloon used in the score animation.
    /// </summary>
    [SerializeField]
    private GameObject blueBalloonPrefab = null;

    /// <summary>
    /// Prefab for the red balloon used in the score animation.
    /// </summary>
   [SerializeField]
    private GameObject redBalloonPrefab = null;

    /// <summary>
    /// Prefab for the yellow balloon used in the score animation.
    /// </summary>
    [SerializeField] 
    private GameObject yellowBalloonPrefab = null;

    /// <summary>
    /// Minimum number of balloons to spawn during a score animation.
    /// </summary>
    [SerializeField]
    private int minBaloons = 3;

    /// <summary>
    /// Minimum height (in units) that balloons will rise before exploding.
    /// </summary>
   [SerializeField]
    private float minHeightToBalloonExplode = 3f;
    
    /// <summary>
    /// Maximum height (in units) that balloons will rise before exploding.
    /// </summary>
    [SerializeField]
    private float maxHeightToBalloonExplode = 5f;

    /// <summary>
    /// Minimum duration (in seconds) for balloons to fly upward before exploding.
    /// </summary>
    [SerializeField]
    private float minflyDuration = 1f;

    /// <summary>
    /// Maximum duration (in seconds) for balloons to fly upward before exploding.
    /// </summary>
    [SerializeField]
    private float maxflyDuration = 2f;

    /// <summary>
    /// Visual effect prefab instantiated when a balloon pops.
    /// </summary>
    [SerializeField]
    private GameObject popBalloonEffect = null;

    /// <summary>
    /// Transform containing child GameObjects that serve as spawn positions for balloons.
    /// </summary>
    [SerializeField]
    private Transform balloonsPlaceHoldersGroup = null;

    /// <summary>
    /// Cached array of placeholder GameObjects used as spawn positions for balloons.
    /// Populated during Awake from the children of balloonsPlaceHoldersGroup.
    /// </summary>
    private GameObject[] _balloonsPlaceHolders = null;

    /// <summary>
    /// Caches the initial position and scale for animation reference.
    /// Unity callback called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        _startPosition = transform.position;
        _startScale = transform.localScale;

        if (blueBalloonPrefab == null || redBalloonPrefab == null || yellowBalloonPrefab == null)
        {
            Debug.LogError("One or more balloon prefabs are not assigned in ScoreAreaAnim script.");

            return;
        }

        if (popBalloonEffect == null)
        {
            Debug.LogError("Pop Balloon Effect is not assigned in ScoreAreaAnim script.");

            return;
        }

        if (balloonsPlaceHoldersGroup == null)
        {
            Debug.LogError("Balloons PlaceHolders Group is not assigned in ScoreAreaAnim script.");

            return; 
        }

        _balloonsPlaceHolders = Utils.GetChildren(balloonsPlaceHoldersGroup);
    }

    /// <summary>
    /// Begins the animation sequence.
    /// Unity callback called before the first frame update.
    /// </summary>
    private void Start()
    {
        StartAnimation();
    }

    /// <summary>
    /// Initializes and starts the floating, rotating, and pulsing animations using DOTween.
    /// </summary>
    private void StartAnimation()
    {
        _animSequence = DOTween.Sequence();

        const float FLOAT_HEIGHT = 0f;

        transform.DOMoveY(_startPosition.y + FLOAT_HEIGHT, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        transform.DORotate(new Vector3(0, 360, 0), rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);

        transform.DOScale(_startScale * pulseScale, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    /// <summary>
    /// Cleans up DOTween animations when the object is destroyed.
    /// Unity callback called when the MonoBehaviour will be destroyed.
    /// </summary>
    private void OnDestroy()
    {
        transform.DOKill();
        _animSequence?.Kill();
    }

    /// <summary>
    /// Triggers the score animation by spawning and animating balloons.
    /// Randomly spawns between minBaloons and maxBaloons, ensuring balanced distribution of colors.
    /// Each balloon flies upward and pops at a random height.
    /// </summary>
    public void ScoreAnim()
    {    
        int maxBaloons = _balloonsPlaceHolders.Length;

        int baloonsNumber = Utils.RandomValueInRange(minBaloons, maxBaloons);

        List<GameObject> avaiblePlaceholders = _balloonsPlaceHolders.ToList();

        Dictionary<GameObject, int> balloonTypesCount = new()
        {
            { blueBalloonPrefab, 0 },
            { redBalloonPrefab, 0 },
            { yellowBalloonPrefab, 0 }
        };

        for (int i = 0; i < baloonsNumber; i++)
        {
            GameObject balloonType = GetBalloonType(balloonTypesCount);

            balloonTypesCount[balloonType]++;

            Vector3 spawnPosition = GetBalloonSpawnPosition(avaiblePlaceholders);
            
            GameObject balloon = Instantiate(balloonType, spawnPosition, Quaternion.identity);

            float moveDuration = Utils.RandomValueInRange(minflyDuration, maxflyDuration); 

            float addedHeight = Utils.RandomValueInRange(minHeightToBalloonExplode, maxHeightToBalloonExplode);
            float targetY = spawnPosition.y + addedHeight;

            balloon.transform.DOMoveY(targetY, moveDuration)
                .SetEase(Ease.InSine)
                .OnComplete(() => 
                {  
                    PopBalloon(balloon);   
                });
        }
    }

    /// <summary>
    /// Selects a balloon type (color) using a balanced distribution algorithm.
    /// Prioritizes balloon types that have been spawned the least to maintain color balance.
    /// </summary>
    /// <param name="balloonTypesCount">Dictionary tracking the count of each balloon type spawned.</param>
    /// <returns>A GameObject reference to the selected balloon prefab.</returns>
    private GameObject GetBalloonType(Dictionary<GameObject, int> balloonTypesCount)
    {   
        int minCount = balloonTypesCount.Min(typeCount => typeCount.Value);

        GameObject[] candidates = balloonTypesCount
            .Where(typeCount => typeCount.Value == minCount)
            .Select(typeCount => typeCount.Key)
            .ToArray();

        return candidates[Utils.RandomValueInRange(0, candidates.Length)];
    }

    /// <summary>
    /// Selects a random spawn position from the available placeholders and removes it from the list.
    /// Ensures each balloon spawns at a unique position during a single score animation.
    /// </summary>
    /// <param name="avaiblePlaceholders">List of available placeholder GameObjects for balloon spawning.</param>
    /// <returns>The world position of the selected placeholder.</returns>
    private Vector3 GetBalloonSpawnPosition(List<GameObject> avaiblePlaceholders)
    {
        GameObject placeholder = avaiblePlaceholders[Utils.RandomValueInRange(0, avaiblePlaceholders.Count)];
        avaiblePlaceholders.Remove(placeholder);

        return placeholder.transform.position;
    }

    /// <summary>
    /// Instantiates a pop effect at the balloon's position and destroys both the effect and balloon.
    /// The pop effect is automatically cleaned up after a fixed duration.
    /// </summary>
    /// <param name="balloon">The balloon GameObject to be popped and destroyed.</param>
    private void PopBalloon(GameObject balloon)
    {
        GameObject effect = Instantiate(popBalloonEffect, balloon.transform.position, Quaternion.identity);

        const float EFFECT_DURATION = 2f;
        Destroy(effect, EFFECT_DURATION); 
                    
        Destroy(balloon);
    }
}