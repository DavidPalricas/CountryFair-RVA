using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Implements hand-tracking bow mechanics for the Archery mini-game.
/// Detects pinch gestures (or right-controller trigger as fallback) to grab, pull, and release the arrow;
/// deforms the bow string via a three-point <see cref="LineRenderer"/>; and shows a parabolic trajectory
/// preview while the string is drawn back.
/// </summary>
[ExecuteAlways]
public class BowHandTracking : MonoBehaviour
{
    /// <summary>Fired when the player first closes their hand on the string — used to start the CarnyWise task timer.</summary>
    [SerializeField]
    private UnityEvent playerStartedPulling;

    [Header("References")]
    /// <summary>Root transform of the bow used as the reference frame for pull-distance calculations.</summary>
    [SerializeField]
    private Transform bowRoot;

    /// <summary>Bow mesh renderer source for reading and writing the bow material color during aim.</summary>
    [SerializeField]
    private Transform bowModel;

    /// <summary>Arrow component managed by this bow.</summary>
    [SerializeField]
    private Arrow arrow;

    /// <summary>Trajectory visualizer shown when <see cref="Arrow.readyToLaunch"/> is true.</summary>
    [SerializeField]
    private TrajectoryLine trajectoryLine;

    [Header("Right Hand Override")]
    /// <summary>OVRHand for the right (pulling) hand; used for finger pinch strength queries when hand tracking is active.</summary>
    [SerializeField]
    private OVRHand pullingHand;

    [Header("Line Renderer / String")]
    /// <summary>LineRenderer with three points (top, mid, bottom) that deforms as the string is pulled back.</summary>
    [SerializeField]
    private LineRenderer bowString;

    /// <summary>Dynamically repositioned midpoint of the string that follows the pulling hand.</summary>
    [SerializeField]
    private Transform stringMidPoint;

    /// <summary>Local-space anchor for the top of the bow string.</summary>
    [SerializeField]
    private Vector3 topLocalPos = new (0f, 0.15f, 0f);

    /// <summary>Local-space anchor for the bottom of the bow string.</summary>
    [SerializeField]
    private Vector3 bottomLocalPos = new (0f, -0.15f, 0f);

    [Header("Pull Settings")]
    /// <summary>Maximum hand-backward distance mapped to a pull value of 1.0.</summary>
    [SerializeField]
    private float maxPullDistance = 0.35f;

    /// <summary>Maximum world-space backward offset of the string midpoint at full pull.</summary>
    [SerializeField]
    private float maxStringBackward = 0.25f;

    /// <summary>Exponential smoothing factor (0–1) for the pull value.</summary>
    [Range(0f, 1f)]
     [SerializeField]
     private float pullSmooth = 0.2f;

    [Header("Force")]
    /// <summary>Minimum launch force applied at zero pull.</summary>
    [SerializeField]
    private float minForce = 5f;
    /// <summary>Maximum launch force applied at full pull.</summary>
    [SerializeField]
    private float maxForce = 60f;

    [Header("Finger Detection")]
    /// <summary>Pinch strength above which the hand is considered closed (grabbing).</summary>
    [SerializeField]
    private float closeThreshold = 0.25f;
    /// <summary>Pinch strength below which the hand is considered open (releasing).</summary>
    [SerializeField]
    private float openThreshold = 0.10f;

    [Header("Arrow Grab Detection")]
    /// <summary>World-space point on the bow that the hand must be near to initiate pulling.</summary>
    [SerializeField]
    private Transform arrowGrabPoint;
    /// <summary>Radius around <see cref="arrowGrabPoint"/> within which the hand counts as grabbing.</summary>
    [SerializeField]
    private float grabRadius = 0.05f;

    /// <summary>Fired on arrow release with the shot sound effect and the bow GameObject as the spatial audio source.</summary>
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
