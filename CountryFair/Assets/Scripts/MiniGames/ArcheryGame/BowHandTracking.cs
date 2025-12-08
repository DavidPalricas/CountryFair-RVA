using UnityEngine;
using Oculus.Interaction.Input;

[ExecuteAlways]
public class BowHandTracking : MonoBehaviour
{
    [Header("References")]
    public Transform bowRoot;                
    public Transform arrowSpawn;
    public GameObject arrowPrefab;

    [Header("Right Hand Override")]
    public OVRHand pullingHand;               // opcional (se existir hand tracking)
    private Transform handSource;             // mão OU controlador (fallback automático)

    [Header("Line Renderer / String")]
    public LineRenderer bowString;
    public Transform stringMidPoint;
    public Vector3 topLocalPos = new Vector3(0f, 0.15f, 0f);
    public Vector3 bottomLocalPos = new Vector3(0f, -0.15f, 0f);

    [Header("Pull Settings")]
    public float maxPullDistance = 0.35f;
    public float maxStringBackward = 0.25f;
    [Range(0f, 1f)] public float pullSmooth = 0.2f;

    [Header("Force")]
    public float minForce = 5f;
    public float maxForce = 60f;

    [Header("Finger Detection")]
    public float closeThreshold = 0.25f;
    public float openThreshold = 0.10f;

    // Runtime
    private GameObject currentArrow;
    private float currentPull = 0f;
    private bool arrowReady = false;

    private Vector3 stringMidStartLocalPos;
    private Vector3 stringMidStartWorldPos;
    private Transform runtimeMid;

    void Start()
    {
        if (bowRoot == null)
            bowRoot = transform;

        // --- HAND SOURCE SETUP (VERY IMPORTANT) ---
        AssignRightHandSource();

        // --- STRING MIDPOINT ---
        if (stringMidPoint == null)
        {
            GameObject go = new GameObject("StringMidPoint_Runtime");
            go.hideFlags = HideFlags.DontSaveInBuild;
            go.transform.SetParent(bowRoot, false);
            go.transform.localPosition = Vector3.zero;
            stringMidPoint = go.transform;
            runtimeMid = stringMidPoint;
        }

        stringMidStartLocalPos = (topLocalPos + bottomLocalPos) * 0.5f;
        stringMidPoint.localPosition = stringMidStartLocalPos;
        stringMidStartWorldPos = stringMidPoint.position;

        if (bowString != null)
            bowString.positionCount = 3;
    }

    void AssignRightHandSource()
    {
        // 1. If OVRHand exists and is tracked → use it
        if (pullingHand != null && pullingHand.IsTracked)
        {
            handSource = pullingHand.transform;
            Debug.Log("[Bow] Using real RIGHT HAND for pulling.");
            return;
        }

        // 2. Otherwise → use RightControllerAnchor
        var rig = FindObjectOfType<OVRCameraRig>();
        if (rig != null)
        {
            handSource = rig.rightControllerAnchor;
            Debug.Log("[Bow] Using RIGHT CONTROLLER as pulling hand fallback.");
        }
    }

    void Update()
    {
        if (handSource == null)
        {
            AssignRightHandSource();
            return;
        }

        bool handClosed = IsHandClosed();
        bool handOpen = IsHandOpen();

        if (handClosed && !arrowReady)
            PrepareArrow();

        if (arrowReady)
            UpdatePull();

        if (arrowReady && handOpen)
            FireArrow();

        UpdateBowString();
    }

    // PREPARAR SETA -------------------------------
    void PrepareArrow()
    {
        if (arrowPrefab == null || arrowSpawn == null)
            return;

        arrowReady = true;

        currentArrow = Instantiate(arrowPrefab, arrowSpawn.position, arrowSpawn.rotation, arrowSpawn);

        Rigidbody rb = currentArrow.GetComponentInChildren<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        currentPull = 0f;

        stringMidStartLocalPos = (topLocalPos + bottomLocalPos) * 0.5f;
        stringMidPoint.localPosition = stringMidStartLocalPos;
        stringMidStartWorldPos = stringMidPoint.position;
    }

    // PULL -----------------------------------------
    void UpdatePull()
    {
        Vector3 handPos = handSource.position;

        // SEMPRE recalcular posição inicial da corda
        stringMidStartWorldPos = bowRoot.TransformPoint(stringMidStartLocalPos);

        Vector3 pullDir = bowRoot.forward;

        float rawDist = Vector3.Dot(handPos - stringMidStartWorldPos, pullDir);

        float backwardDist = Mathf.Max(0f, -rawDist);

        float pullAmount = Mathf.Clamp01(backwardDist / maxPullDistance);

        currentPull = Mathf.Lerp(currentPull, pullAmount,
            1f - Mathf.Exp(-pullSmooth * 30f * Time.deltaTime));

        Vector3 targetPos = stringMidStartWorldPos - bowRoot.forward * (currentPull * maxStringBackward);

        stringMidPoint.position = targetPos;

        if (currentArrow != null)
        {
            currentArrow.transform.SetPositionAndRotation(
                stringMidPoint.position,
                bowRoot.rotation
            );
        }
    }






    // FIRE ------------------------------------------
    void FireArrow()
    {
        if (currentArrow != null)
        {
            currentArrow.transform.parent = null;
            Rigidbody rb = currentArrow.GetComponentInChildren<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                float force = Mathf.Lerp(minForce, maxForce, currentPull);
                rb.AddForce(arrowSpawn.forward * force, ForceMode.VelocityChange);
            }
        }

        stringMidPoint.localPosition = stringMidStartLocalPos;
        currentPull = 0f;
        arrowReady = false;
        currentArrow = null;
    }

    // STRING -----------------------------------------
    void UpdateBowString()
    {
        if (bowString == null) return;

        Vector3 topWorld = bowRoot.TransformPoint(topLocalPos);
        Vector3 bottomWorld = bowRoot.TransformPoint(bottomLocalPos);

        bowString.SetPosition(0, topWorld);
        bowString.SetPosition(1, stringMidPoint.position);
        bowString.SetPosition(2, bottomWorld);
    }

    // HAND STATE --------------------------------------
    bool IsHandClosed()
    {
        // If hand tracking exists
        if (pullingHand != null && pullingHand.IsTracked)
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

    bool IsHandOpen()
    {
        if (pullingHand != null && pullingHand.IsTracked)
        {
            float i = pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
            float m = pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
            float r = pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);
            return i < openThreshold && m < openThreshold && r < openThreshold;
        }

        return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) < 0.1f &&
               OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) < 0.1f;
    }
}
