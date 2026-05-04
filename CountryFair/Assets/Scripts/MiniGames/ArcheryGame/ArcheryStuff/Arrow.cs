using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    private Rigidbody _rb;
    
    [HideInInspector]
    public bool readyToLaunch = false;

    public bool IsTutorialActive{get; set;} = true;
    public bool InAir { get ; private set; } = false;

    private Crowd _crowd;


    private Transform parentTransform = null;


    [SerializeField]
    private UnityEvent <int> playerScored;
   
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
