using UnityEngine;
using Oculus.Interaction.Input;

[ExecuteAlways]
public class BowHandTracking : MonoBehaviour
{
    [Header("References")]
    public Transform bowRoot;           // transform do arco (root). usado como referência local
    public OVRHand pullingHand;         // mão que puxa (direita)
    public Transform arrowSpawn;
    public GameObject arrowPrefab;

    [Header("Line Renderer / String")]
    public LineRenderer bowString;
    public Transform stringMidPoint;    // ponto que recua (DEVE ser filho de bowRoot) - se null, será criado
    public Vector3 topLocalPos = new Vector3(0f, 0.15f, 0f);   // posição local do topo relativamente a bowRoot
    public Vector3 bottomLocalPos = new Vector3(0f, -0.15f, 0f);// posição local da base relativamente a bowRoot

    [Header("Pull Settings")]
    public float maxPullDistance = 0.35f;       // distância da mão necessária para pull = 1
    public float maxStringBackward = 0.25f;     // quanto a corda recua no eixo local -Z
    [Range(0f, 1f)] public float pullSmooth = 0.2f; // suavização do movimento do ponto médio (0 instantâneo)

    [Header("Force")]
    public float minForce = 5f;
    public float maxForce = 60f;

    [Header("Finger Detection")]
    public float closeThreshold = 0.25f;
    public float openThreshold = 0.10f;

    // runtime
    private GameObject currentArrow;
    private Vector3 arrowInitialLocalPos;
    private float currentPull = 0f;
    private bool arrowReady = false;

    // internals para posições iniciais
    private Vector3 stringMidStartLocalPos;
    private Vector3 stringMidStartWorldPos;

    // created container for runtime stringMid if needed
    private Transform runtimeMid;

    void Reset()
    {
        // tentar auto-assign bowRoot para o transform actual
        if (bowRoot == null)
            bowRoot = transform;
    }

    void OnValidate()
    {
        // garantir position count
        if (bowString != null && bowString.positionCount < 3)
            bowString.positionCount = 3;
    }

    void Start()
    {
        if (bowRoot == null)
            bowRoot = transform;

        // Se não houver stringMidPoint, cria um vazio como filho do bowRoot
        if (stringMidPoint == null)
        {
            GameObject go = new GameObject("StringMidPoint_Runtime");
            go.hideFlags = HideFlags.DontSaveInBuild;
            go.transform.SetParent(bowRoot, false);
            go.transform.localPosition = Vector3.zero;
            stringMidPoint = go.transform;
            runtimeMid = stringMidPoint;
        }

        // inicializar start local/world com base em bowRoot + locais top/bottom
        stringMidStartLocalPos = (topLocalPos + bottomLocalPos) * 0.5f;
        stringMidPoint.localPosition = stringMidStartLocalPos;
        stringMidStartWorldPos = stringMidPoint.position;

        // garantir LineRenderer tem 3 posições
        if (bowString != null)
            bowString.positionCount = Mathf.Max(3, bowString.positionCount);

        // garantir arrow prefab Rigidbody configurado ok
        if (arrowPrefab != null)
        {
            Rigidbody rb = arrowPrefab.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true; // recomendamos o prefab com kinematic true
        }
    }

    void Update()
    {

        // evita erros se falta de refs
        if (bowRoot == null) return;

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

    // -------------------------
    // PREPARAR SETA
    // -------------------------
    void PrepareArrow()
    {
        if (arrowPrefab == null || arrowSpawn == null)
        {
            Debug.LogWarning("[Bow] arrowPrefab ou arrowSpawn não definidos.");
            return;
        }

        arrowReady = true;
        currentArrow = Instantiate(arrowPrefab, arrowSpawn.position, arrowSpawn.rotation, arrowSpawn);
        arrowInitialLocalPos = currentArrow.transform.localPosition;

        Rigidbody rb = currentArrow.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        // reset pull
        currentPull = 0f;

        // garantir midpoint inicial
        stringMidStartLocalPos = (topLocalPos + bottomLocalPos) * 0.5f;
        stringMidPoint.localPosition = stringMidStartLocalPos;
        stringMidStartWorldPos = stringMidPoint.position;
    }

    // -------------------------
    // UPDATE PULL - apenas recua ao longo do eixo local Z do bowRoot
    // -------------------------
void UpdatePull()
{
    if (pullingHand == null || stringMidPoint == null) return;
    if (!pullingHand.IsTracked) return;

    Vector3 handPos = pullingHand.transform.position;

    // projetar a mão no eixo -Z do arco
    Vector3 localHand = bowRoot.InverseTransformPoint(handPos);

    // quanto recuou no eixo Z (valor negativo)
    float pullAmount = Mathf.Clamp01( Mathf.Abs(localHand.z) / maxPullDistance );

    // suavizar
    currentPull = Mathf.Lerp(currentPull, pullAmount, 1f - Mathf.Exp(-pullSmooth * 30f * Time.deltaTime));

    // mover corda localmente
    Vector3 newLocal = stringMidStartLocalPos;
    newLocal.z = -currentPull * Mathf.Abs(maxStringBackward);
    stringMidPoint.localPosition = newLocal;

    // colar seta
    if (currentArrow != null)
    {
        currentArrow.transform.position = stringMidPoint.position;
        currentArrow.transform.rotation = arrowSpawn.rotation;
    }
}


    // -------------------------
    // FIRE
    // -------------------------
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
                if (arrowSpawn != null)
                    rb.AddForce(arrowSpawn.forward * force, ForceMode.VelocityChange);
                else
                    rb.AddForce(bowRoot.forward * force, ForceMode.VelocityChange);
            }
        }

        // reset midpoint local
        stringMidPoint.localPosition = stringMidStartLocalPos;
        currentPull = 0f;
        arrowReady = false;
        currentArrow = null;
    }

    // -------------------------
    // LINE RENDERER UPDATE - usa topLocalPos / bottomLocalPos relativos ao bowRoot
    // -------------------------
    void UpdateBowString()
    {
        if (bowString == null) return;

        if (bowString.positionCount < 3)
            bowString.positionCount = 3;

        // calcular posições em world a partir de locais relativos ao bowRoot
        Vector3 topWorld = bowRoot.TransformPoint(topLocalPos);
        Vector3 bottomWorld = bowRoot.TransformPoint(bottomLocalPos);
        Vector3 midWorld = stringMidPoint != null ? stringMidPoint.position : (topWorld + bottomWorld) * 0.5f;

        bowString.SetPosition(0, topWorld);
        bowString.SetPosition(1, midWorld);
        bowString.SetPosition(2, bottomWorld);
    }

    // -------------------------
    // DETEÇÃO DE MÃO
    // -------------------------
    bool IsHandClosed()
    {
        if (pullingHand == null) return false;

        float i = pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        float m = pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
        float r = pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);

        return i > closeThreshold || m > closeThreshold || r > closeThreshold;
    }

    bool IsHandOpen()
    {
        if (pullingHand == null) return true;

        float i = pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        float m = pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
        float r = pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);

        return i < openThreshold && m < openThreshold && r < openThreshold;
    }

    // -------------------------
    // GIZMOS para auxiliar no Editor
    // -------------------------
    void OnDrawGizmosSelected()
    {
        if (bowRoot == null) return;

        Gizmos.color = Color.green;
        Vector3 topWorld = bowRoot.TransformPoint(topLocalPos);
        Vector3 bottomWorld = bowRoot.TransformPoint(bottomLocalPos);
        Gizmos.DrawSphere(topWorld, 0.01f);
        Gizmos.DrawSphere(bottomWorld, 0.01f);

        Gizmos.color = Color.yellow;
        Vector3 midWorld = bowRoot.TransformPoint((topLocalPos + bottomLocalPos) * 0.5f);
        Gizmos.DrawSphere(midWorld, 0.01f);
    }
}
