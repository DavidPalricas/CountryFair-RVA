using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class DuckSwim : MonoBehaviour
{
    [Header("Wandering")]
    [SerializeField] private float wanderRadius = 3f;
    [Header("Bobbing Animation")]
    [SerializeField] private float bobAmplitude = 0.02f;
    [SerializeField] private float bobFrequency = 1.5f;

    [Header("Rotation Wobble")]
    [SerializeField] private float wobbleAmplitude = 3f;
    [SerializeField] private float wobbleFrequency = 0.8f;
    [SerializeField] private float rotationSpeed = 5f;

    private NavMeshAgent _agent;
    private float _baseY;
    private float _phaseOffset;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _baseY = transform.position.y;

        // Phase offset único por pato para dessincronizar o bobbing entre instâncias
        _phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Start()
    {
        SetNewDestination();
    }

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

    private bool TryGetRandomPoint(out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, _agent.areaMask))
            {
                result = hit.position;
                return true;
            }
        }

        result = transform.position;
        return false;
    }

    private void ApplyBobbing()
    {
        float time = Time.time + _phaseOffset;
        Vector3 pos = transform.position;
        pos.y = _baseY + Mathf.Sin(time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
        transform.position = pos;
    }

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