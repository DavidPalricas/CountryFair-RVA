using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class ScoreAreaProperties : MonoBehaviour
{   
    [SerializeField]
    private AreaType areaType = AreaType.NORMAL;

    [SerializeField]
    private UnityEvent tutorialTaskCompleted;


    [Header("Movement Settings")]
    [SerializeField] 
    private float moveSpeed = 2f;
    [SerializeField] 
    private float radius = 0.5f;

    [Header("Visibility Settings")]
    [SerializeField] 
    private float timeVisible = 3f;  
    [SerializeField] 
    private float timeInvisible = 2f; 

    private Vector3 _initialPosition = Vector3.zero;
    private Renderer _renderer;
    private Collider _collider;

    // Constante para loops infinitos
    private const int _INFINITE_LOOPS = -1;

    // Unique ID to differentiate Blinking from Moving
    private string _blinkId; 


    private FrisbeeGameManager _frisbeeGameManager;

    public int ScorePoints  { get; private set; } = 1;

    public enum AreaType
    {
        NORMAL,
        DOG,
        TUTORIAL
    }

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();
        _initialPosition = transform.position;
        
        _blinkId = "blink_" + GetInstanceID();


        if (areaType == AreaType.DOG)
        {
            ScorePoints = 8;
        }

        _frisbeeGameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<FrisbeeGameManager>();

        if (_frisbeeGameManager == null)
        {
            Debug.LogError("FrisbeeGameManager not found in the scene.");
        }
    }

    public void AdjustMovement(bool isToMove)
    {   
        if (areaType != AreaType.NORMAL)
        {
            Debug.LogWarning("Movement adjustment is only applicable for NORMAL area type.");
            return;
        }

        if (areaType == AreaType.NORMAL && isToMove)
        {   
            ScorePoints *= 2;
            StartMoving();
            return;
        }
        StopMoving();
    }

    public void AdjustVisibility(bool canChangeVisibility)
    {  if (areaType != AreaType.NORMAL)
        {
            Debug.LogWarning("Visibility adjustment is only applicable for NORMAL area type.");
            return;
        }
        
        if (canChangeVisibility)
        { 
            ScorePoints *= 3;
            StartBlinking();
            return;
        }
        StopBlinking();
    }
    
    private void StartMoving()
    {   
        transform.DOKill(false);

        const int STEPS = 8;
        Vector3[] circularPath = GetCircularWaypoints(_initialPosition, radius, STEPS);

        transform.DOPath(circularPath, moveSpeed, PathType.CatmullRom)
            .SetOptions(true)
            .SetEase(Ease.Linear)
            .SetLoops(_INFINITE_LOOPS);
    }

    private void StopMoving()
    {
        transform.DOKill(false);

        const float DURATION_TO_INITIAL_POS = 0.5f;
        
        transform.DOMove(_initialPosition, DURATION_TO_INITIAL_POS); 
    }

   private void StartBlinking()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.SetId(_blinkId); // <--- O SEGREDO: Dá um nome a esta animação

        sequence.AppendInterval(timeVisible);
        sequence.AppendCallback(() => ToggleVisuals(false));
        sequence.AppendInterval(timeInvisible);
        sequence.AppendCallback(() => ToggleVisuals(true));
       
        sequence.SetLoops(_INFINITE_LOOPS);
    }

    private void StopBlinking()
    {
        DOTween.Kill(_blinkId);
        
        ToggleVisuals(true); 
    }


    private void ToggleVisuals(bool visualState)
    {
        _renderer.enabled = visualState;
        _collider.enabled = visualState;
    }

    private Vector3[] GetCircularWaypoints(Vector3 centerPos, float r, int steps)
    {
        Vector3[] points = new Vector3[steps];
        
        // 360 degrees in radians
        const float FULL_CIRCLE_IN_RADIANS = 2 * Mathf.PI;

        float angleStep = FULL_CIRCLE_IN_RADIANS  / steps;
        
        Vector3 centerOfCircle = centerPos + (transform.right * r); 
        
        for (int i = 0; i < steps; i++)
        {
            float angle = i * angleStep + Mathf.PI; 
            float x = centerOfCircle.x + r * Mathf.Cos(angle);
            float z = centerOfCircle.z + r * Mathf.Sin(angle);
            
            points[i] = new Vector3(x, centerPos.y, z);
        }

        return points;
    }

    private void OnDestroy()
    {   
        if (areaType == AreaType.NORMAL)
        {
              transform.DOKill(); 

              return;
        }
    }

    public void OnPlayerScore(){
        if (areaType == AreaType.TUTORIAL)
        {
            tutorialTaskCompleted.Invoke();

            Destroy(gameObject);

            return;
        }

        if (areaType == AreaType.NORMAL)
        {
             _frisbeeGameManager.DestroyTarget(gameObject);

             return;
        }

        gameObject.SetActive(false);
    }
}