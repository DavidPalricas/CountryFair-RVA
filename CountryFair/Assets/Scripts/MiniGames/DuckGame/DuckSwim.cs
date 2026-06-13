using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Drives autonomous duck behaviour on a NavMesh water surface:
/// random wandering, vertical bobbing, and rotation wobble to simulate floating movement.
/// All positional offsets use local space so the effect follows a moving parent (e.g. a repositioned pond).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class DuckSwim : MonoBehaviour
{
    [Header("Wandering")]
    /// <summary>Radius around the duck's current world position in which a new NavMesh destination is sampled.</summary>
    [SerializeField] private float wanderRadius = 3f;

    /// <summary>Minimum distance the new destination must be from the previous one, preventing repeated visits to the same spot.</summary>
    [SerializeField] private float minMoveDistance = 1f;

    [Header("Bobbing Animation")]
    /// <summary>Peak vertical offset above the local rest position, in local units.</summary>
    [SerializeField] private float bobAmplitude = 0.02f;

    /// <summary>Bobbing cycles per second.</summary>
    [SerializeField] private float bobFrequency = 1.5f;

    [Header("Rotation Wobble")]
    /// <summary>Maximum tilt angle in degrees applied to Z and X axes to simulate floating sway.</summary>
    [SerializeField] private float wobbleAmplitude = 3f;

    /// <summary>Wobble cycles per second.</summary>
    [SerializeField] private float wobbleFrequency = 0.8f;

    /// <summary>How quickly the duck rotates to face its movement direction (lerp factor, degrees per second).</summary>
    [SerializeField] private float rotationSpeed = 5f;

    private NavMeshAgent _agent;

    /// <summary>Local Y at spawn; bobbing oscillates around this value so the effect stays relative to the parent.</summary>
    private float _baseLocalY;

    /// <summary>Per-instance sine phase so multiple ducks bob out of sync.</summary>
    private float _phaseOffset;

    private Vector3 _lastDestination;

    /// <summary>
    /// Captures the local rest Y, disables agent auto-rotation (handled manually in <see cref="ApplyWobble"/>),
    /// and seeds a unique phase offset for this instance.
    /// </summary>
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _baseLocalY = transform.localPosition.y;

        // Phase offset único por pato para dessincronizar o bobbing entre instâncias
        _phaseOffset = Utils.RandomValueInRange(0f, Mathf.PI * 2f);
        _lastDestination = transform.position;
    }

    /// <summary>Picks the first random NavMesh destination so the duck begins moving immediately.</summary>
    private void Start()
    {
        SetNewDestination();
    }

    /// <summary>Applies bobbing and wobble every frame, and requests a new destination once the current one is reached.</summary>
    private void Update()
    {
        ApplyBobbing();
        ApplyWobble();

        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            SetNewDestination();
        }
    }

    private void SetNewDestination()
    {
        if (TryGetRandomPoint(out Vector3 target))
        {
            _agent.SetDestination(target);
        }
    }

    /// <summary>
    /// Attempts up to 15 times to find a valid NavMesh point within <see cref="wanderRadius"/> of the duck.
    /// Uses a 2D circle sample (X/Z only) with a tight snap distance so off-surface candidates fail
    /// and retry rather than all converging on the same NavMesh edge.
    /// </summary>
    /// <param name="result">World position of the chosen destination, or the duck's current position on failure.</param>
    /// <returns>True if a suitable point was found; false if all attempts failed.</returns>
    private bool TryGetRandomPoint(out Vector3 result)
    {
        const int MAX_TRIES = 15;
        const float MAX_DISTANCE = 0.5f;

        for (int i = 0; i < MAX_TRIES; i++)
        {
            Vector2 circle = Random.insideUnitCircle * wanderRadius;
            Vector3 candidate = transform.position + new Vector3(circle.x, 0f, circle.y);

            // Snap distance pequeno: pontos fora do NavMesh falham e reentam em vez de convergirem na mesma borda
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, MAX_DISTANCE, _agent.areaMask)
                && Vector3.Distance(hit.position, _lastDestination) >= minMoveDistance)
            {
                _lastDestination = hit.position;
                result = hit.position;
                return true;
            }
        }

        result = transform.position;
        return false;
    }

    /// <summary>
    /// Oscillates the duck's local Y around <see cref="_baseLocalY"/> using a sine wave,
    /// keeping the effect parent-relative so a moving pond carries the ducks correctly.
    /// </summary>
    private void ApplyBobbing()
    {
        float time = Time.time + _phaseOffset;
        Vector3 localPos = transform.localPosition;
        localPos.y = _baseLocalY + Mathf.Sin(time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
        transform.localPosition = localPos;
    }

    /// <summary>
    /// Applies a sinusoidal tilt on X and Z to simulate floating sway.
    /// When the agent is moving, smoothly rotates Y to face the velocity direction.
    /// </summary>
    private void ApplyWobble()
    {
        float time = Time.time + _phaseOffset;
        float wobbleZ = Mathf.Sin(time * wobbleFrequency * Mathf.PI * 2f) * wobbleAmplitude;
        float wobbleX = Mathf.Cos(time * wobbleFrequency * 0.7f * Mathf.PI * 2f) * wobbleAmplitude * 0.5f;

        Vector3 euler = transform.localEulerAngles;

        if (_agent.velocity.sqrMagnitude > 0.01f)
        {
            float targetY = Quaternion.LookRotation(_agent.velocity.normalized).eulerAngles.y;
            euler.y = Mathf.LerpAngle(euler.y, targetY, Time.deltaTime * rotationSpeed);
        }

        euler.z = wobbleZ;
        euler.x = wobbleX;
        transform.localEulerAngles = euler;
    }
}
