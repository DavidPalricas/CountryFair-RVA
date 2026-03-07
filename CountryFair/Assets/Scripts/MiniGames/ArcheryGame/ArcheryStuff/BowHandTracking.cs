using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class BowHandTracking : MonoBehaviour
{   

    [SerializeField]
    private UnityEvent playerStartedPulling;

    [Header("References")]
    [SerializeField]
    private Transform bowRoot;  

    [SerializeField]
    private Transform bowModel;                

    [SerializeField]   
    private Arrow arrow;

    [SerializeField]
    private TrajectoryLine trajectoryLine;

    [Header("Right Hand Override")]
    [SerializeField]
    private OVRHand pullingHand;    
    
               // opcional (se existir hand tracking)
         // mão OU controlador (fallback automático)

    [Header("Line Renderer / String")]
    [SerializeField]
    private LineRenderer bowString;

    [SerializeField]
    private Transform stringMidPoint;

    [SerializeField]
    private Vector3 topLocalPos = new (0f, 0.15f, 0f);

    [SerializeField]
    private Vector3 bottomLocalPos = new (0f, -0.15f, 0f);

    [Header("Pull Settings")]
    [SerializeField]
    private float maxPullDistance = 0.35f;
    
    [SerializeField]
    private float maxStringBackward = 0.25f;

    [Range(0f, 1f)]
     [SerializeField] 
     private float pullSmooth = 0.2f;

    [Header("Force")]
    [SerializeField]
    private float minForce = 5f;
    [SerializeField]
    private float maxForce = 60f;

    [Header("Finger Detection")]
    [SerializeField]
    private float closeThreshold = 0.25f;
    [SerializeField]
    private float openThreshold = 0.10f;


    [Header("Arrow Grab Detection")]
    [SerializeField]
    private Transform arrowGrabPoint;     // ponto que a mão tem de tocar
    [SerializeField]
    private float grabRadius = 0.05f;     // raio da zona de deteção

    public UnityEvent<AudioManager.GameSoundEffects, GameObject> arrowShot;
    

    private Transform _handSource;    


    private float _currentPull = 0f;
  
    private Vector3 _stringMidStartLocalPos;
    private Vector3 _stringMidStartWorldPos;

    private Vector3 _shootDirection;
    private float _shootForce;
    private MeshRenderer _bowRenderer;
    private Color _originalColor;


    private readonly AudioManager.GameSoundEffects _shootSoundEffect = AudioManager.GameSoundEffects.ARROW_SHOT;

    private void Start()
    {
        if (bowRoot == null)
        {
             bowRoot = transform;
        }
           
        // --- HAND SOURCE SETUP (VERY IMPORTANT) ---
        AssignRightHandSource();

        // --- STRING MIDPOINT ---
        if (stringMidPoint == null)
        {
            GameObject go = new("StringMidPoint_Runtime")
            {
                hideFlags = HideFlags.DontSaveInBuild
            };

            go.transform.SetParent(bowRoot, false);
            go.transform.localPosition = Vector3.zero;
            stringMidPoint = go.transform;
  
        }

        _stringMidStartLocalPos = (topLocalPos + bottomLocalPos) * 0.5f;
        stringMidPoint.localPosition = _stringMidStartLocalPos;
        _stringMidStartWorldPos = stringMidPoint.position;

        if (bowString != null)
        {
            bowString.positionCount = 3;
        }
        
        _bowRenderer = bowModel.GetComponent<MeshRenderer>();

        if (_bowRenderer != null)
        {
            _originalColor = _bowRenderer.sharedMaterial.color;  
        }

    }

    private void AssignRightHandSource()
    {
        // 2. Otherwise → use RightControllerAnchor
        OVRCameraRig rig = FindFirstObjectByType<OVRCameraRig>();

        if (rig != null)
        {
            _handSource = rig.rightControllerAnchor;
        }
    }

    private void Update()
    {
        if (arrow.readyToLaunch)
        {
            Vector3 startVel = _shootDirection * _shootForce;
            trajectoryLine.ShowTrajectory(arrow.transform.position, startVel);

            SetBowTransparency(0.35f);
        }
        else
        {
            trajectoryLine.HideTrajectory();
            SetBowTransparency(_originalColor.a);
        }

        if (_handSource == null)
        {
            AssignRightHandSource();
            return;
        }

        bool handClosed = IsHandClosed();
        bool handOpen = IsHandOpen();

        if (handClosed && !arrow.InAir && !arrow.readyToLaunch && IsHandAtGrabPoint())
        {
            PrepareArrow();
        }
           
        if (arrow.readyToLaunch)
        {
            UpdatePull();
        }
          
        if (arrow.readyToLaunch && handOpen)
        {
             FireArrow();
        }
           

        UpdateBowString();
    }

    // PREPARAR SETA -------------------------------
    private void PrepareArrow()
    {   
        arrow.readyToLaunch = true;
        _currentPull = 0f;

        _stringMidStartLocalPos = (topLocalPos + bottomLocalPos) * 0.5f;
        stringMidPoint.localPosition = _stringMidStartLocalPos;
        _stringMidStartWorldPos = stringMidPoint.position;

        playerStartedPulling.Invoke();
    }

    // PULL -----------------------------------------
    private void UpdatePull()
    {
        Vector3 handPos = _handSource.position;

        // SEMPRE recalcular posição inicial da corda
        _stringMidStartWorldPos = bowRoot.TransformPoint(_stringMidStartLocalPos);

        Vector3 pullDir = bowRoot.forward;

        float rawDist = Vector3.Dot(handPos - _stringMidStartWorldPos, pullDir);

        float backwardDist = Mathf.Max(0f, -rawDist);

        float pullAmount = Mathf.Clamp01(backwardDist / maxPullDistance);

        _currentPull = Mathf.Lerp(_currentPull, pullAmount,
            1f - Mathf.Exp(-pullSmooth * 30f * Time.deltaTime));

        Vector3 targetPos = _stringMidStartWorldPos - bowRoot.forward * (_currentPull * maxStringBackward);

        stringMidPoint.position = targetPos;

        Vector3 offset = bowRoot.forward * 0.12f; // ajusta o valor
        arrow.transform.SetPositionAndRotation(
            stringMidPoint.position + offset,
            bowRoot.rotation
        );
        
        // Direção do tiro = direção em que o arco aponta
        _shootDirection = bowRoot.forward;

        // Força calculada com base no pull (mesmo que o FireArrow usa)
        _shootForce = Mathf.Lerp(minForce, maxForce, _currentPull);
    }


    // FIRE ------------------------------------------
    private void FireArrow()
    {       
        float launchForce = Mathf.Lerp(minForce, maxForce, _currentPull);

        arrow.Launch(launchForce);

        arrowShot?.Invoke(_shootSoundEffect, gameObject);
        
        stringMidPoint.localPosition = _stringMidStartLocalPos;
        _currentPull = 0f;
    }

    // STRING -----------------------------------------
    private void UpdateBowString()
    {
        Vector3 topWorld = bowRoot.TransformPoint(topLocalPos);
        Vector3 bottomWorld = bowRoot.TransformPoint(bottomLocalPos);

        bowString.SetPosition(0, topWorld);
        bowString.SetPosition(1, stringMidPoint.position);
        bowString.SetPosition(2, bottomWorld);
    }

    // HAND STATE --------------------------------------
    private bool IsHandClosed()
    {
        // If hand tracking exists
        if (pullingHand.IsTracked)
        {
            float i = pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
            float m = pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
            float r = pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);
            return i > closeThreshold || m > closeThreshold || r > closeThreshold;
        }

        // Controller fallback
        return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.2f ||
               OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.2f;
    }

    private bool IsHandOpen()
    {
        if (pullingHand.IsTracked)
        {
            float i = pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
            float m = pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
            float r = pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);
            return i < openThreshold && m < openThreshold && r < openThreshold;
        }

        return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) < 0.1f &&
               OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) < 0.1f;
    }

    private void SetBowTransparency(float alpha)
    {
        Color c = _bowRenderer.sharedMaterial.color;
        c.a = alpha;
        _bowRenderer.sharedMaterial.color = c;
    }

    private bool IsHandAtGrabPoint()
    {
        float dist = Vector3.Distance(_handSource.position, arrowGrabPoint.position);

        return dist <= grabRadius;
    }
}
