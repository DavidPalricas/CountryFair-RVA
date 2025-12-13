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
    [SerializeField]
    private GameObject blueBalloonPrefab = null;


    [SerializeField]
    private Transform balloonsPlaceHoldersGroup = null;
   
   [SerializeField]
    private GameObject redBalloonPrefab = null;

    [SerializeField] 
    private GameObject yellowBalloonPrefab = null;

    [SerializeField]
    private int minBaloons = 3;

   [SerializeField]
    private float minHeightToBalloonExplode = 3f;
    
    [SerializeField]
    private float maxHeightToBalloonExplode = 5f;


    [SerializeField]
    private float minflyDuration = 1f;

    [SerializeField]
    private float maxflyDuration = 2f;


    [SerializeField]
    private GameObject popBalloonEffect = null;

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

    public void ScoreAnim()
    {    
        int maxBaloons = _balloonsPlaceHolders.Length;

        // 1. Determinar quantos balões spawnar
        int baloonsNumber = (int)Utils.RandomValueInRange(minBaloons, maxBaloons);

        List<GameObject> avaiblePlaceholders = _balloonsPlaceHolders.ToList();

        for (int i = 0; i < baloonsNumber; i++)
        {
            // 2. Escolher a cor (Lógica mantida...)
            GameObject balloonType = GetBalloonType();

            Vector3 spawnPosition = GetBalloonSpawnPosition(avaiblePlaceholders);
            
            GameObject balloon = Instantiate(balloonType, spawnPosition, Quaternion.identity);

            // 5. Calcular a altura e definir a velocidade de subida
            // NOTA: Certifique-se que minflyDuration e maxflyDuration estão declarados no script
            float moveDuration = Utils.RandomValueInRange(minflyDuration, maxflyDuration); // Coloquei valores fixos caso não tenha as variáveis, ajuste conforme necessário

            float addedHeight = Utils.RandomValueInRange(minHeightToBalloonExplode, maxHeightToBalloonExplode);
            float targetY = spawnPosition.y + addedHeight;

            // 6. Animar com DOTween
            balloon.transform.DOMoveY(targetY, moveDuration)
                .SetEase(Ease.InSine)
                .OnComplete(() => 
                {  
                    PopBalloon(balloon);   
                });
        }
    }

    private GameObject GetBalloonType()
    {
        float randomValue = Random.Range(0f, 1f);

        Debug.Log($"Random value for balloon type: {randomValue}");

        if (randomValue < 0.33f)
        {
            return blueBalloonPrefab;
        }

        if (randomValue < 0.66f)
        {
            return redBalloonPrefab;
        }
            
        return yellowBalloonPrefab;
    }

    private Vector3 GetBalloonSpawnPosition(List<GameObject> avaiblePlaceholders)
    {
        GameObject placeholder = avaiblePlaceholders[Random.Range(0, avaiblePlaceholders.Count)];
        avaiblePlaceholders.Remove(placeholder);

        return placeholder.transform.position;
    }

    private void PopBalloon(GameObject balloon)
    {
        GameObject effect = Instantiate(popBalloonEffect, balloon.transform.position, Quaternion.identity);

        const float EFFECT_DURATION = 2f;
        Destroy(effect, EFFECT_DURATION); 
                    
        Destroy(balloon);
    }
}