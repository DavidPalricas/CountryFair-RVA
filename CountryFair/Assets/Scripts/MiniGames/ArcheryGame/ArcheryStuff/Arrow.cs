using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Physical arrow projectile for the Archery mini-game. Handles launch physics, trigger detection for
/// balloon hits and ground/out-of-bounds misses, and fires <c>playerScored</c> or <c>playerMissed</c>
/// events accordingly. Resets to its bow-attachment position after each shot.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    private Rigidbody _rb;

    /// <summary>Set by <see cref="BowHandTracking"/> when the player grabs the string; allows the arrow to be fired.</summary>
    [HideInInspector]
    public bool readyToLaunch = false;

    /// <summary>When false (tutorial mode), hitting a balloon does not fire scoring events.</summary>
    public bool IsTutorialActive{get; set;} = true;
    /// <summary>True while the arrow is in flight; prevents re-grabbing before it lands.</summary>
    public bool InAir { get ; private set; } = false;

    private Crowd _crowd;

    private Transform parentTransform = null;

    /// <summary>Fired with the balloon score value when the arrow hits the scoring-color balloon outside tutorial mode.</summary>
    [SerializeField]
    private UnityEvent <int> playerScored;

    /// <summary>Fired when the arrow hits a non-scoring balloon, the ground, or goes out of bounds.</summary>
    [SerializeField]
    private UnityEvent playerMissed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    
        _crowd = GameObject.FindGameObjectWithTag("CrowdArcheryGame").GetComponent<Crowd>();

        if (_crowd == null)
        {
            Debug.LogError("Crowd GameObject not found in the scene or its crowd component is missing.");
        }

        _rb.isKinematic = true;
    }

    /// <summary>
    /// Detaches the arrow from the bow, enables physics, and applies <paramref name="launchForce"/> in the forward direction.
    /// </summary>
    /// <param name="launchForce">Force magnitude via <see cref="ForceMode.VelocityChange"/>; pass 0 for a drop-miss cheat.</param>
    public void Launch(float launchForce)
    {
        readyToLaunch = false;
        InAir = true;

        parentTransform = transform.parent;


        _rb.isKinematic = false;

        transform.parent = null;

        _rb.AddForce(transform.forward * launchForce, ForceMode.VelocityChange);
    }

    private void OnTriggerEnter(Collider col)
    {
        // --- SE BATER NO BALÃO ---
        if (col.gameObject.CompareTag("Balloon"))
        {   
            BalloonArcheryGame balloon = col.gameObject.GetComponent<BalloonArcheryGame>();

            balloon.Pop();

            SetArrowToOrginalPosition();


            if (!IsTutorialActive)
            {
               int scoreValue = balloon.GetScoreValue();

                if (scoreValue > 0)
                {
                    playerScored.Invoke(scoreValue);

                    _crowd.Cheer();

                    return;
                }
               
                playerMissed.Invoke();

                return;   
            }

            return;
        }

        // --- SE BATER NO CHÃO ---
        if (col.gameObject.CompareTag("Ground") || col.gameObject.CompareTag("OutOfBounds"))
        {
            playerMissed.Invoke();
            SetArrowToOrginalPosition();
            return;
        }
    }


    private void SetArrowToOrginalPosition()
    {  
        _rb.isKinematic = true;
        InAir = false;
       
        transform.parent = parentTransform;
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
}
