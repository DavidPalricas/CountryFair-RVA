
using UnityEngine.AI;
using UnityEngine;

/// <summary>
/// Makes an NPC wander the NavMesh by repeatedly picking random destinations within a fixed radius.
/// Used for ambient crowd NPCs in the CountryFair hub world.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class WanderingPerson: MonoBehaviour
{
    /// <summary>Radius in world units within which random walk destinations are sampled.</summary>
    [SerializeField]
    private float walkRadius = 10f;

    /// <summary>Animator component; avoidance priority is randomized on Awake to prevent agents from clustering.</summary>
    [SerializeField]
    private Animator animator;


    private NavMeshAgent _agent;


    private void Awake()
    {   
        if (animator == null)
        {
            Debug.LogError("Animator reference is missing in Walkable Person.");
            return;
        }

        _agent = GetComponent<NavMeshAgent>();

        const int AVOIDANCE_PRIORITY_MAX_VALUE = 99;
        _agent.avoidancePriority = Random.Range(0, AVOIDANCE_PRIORITY_MAX_VALUE);
    }

    private void Start()
    {
        ChooseRandomDestination();
    }

    private void Update()
    {  
        if (DestinationReached())
        {   
            ChooseRandomDestination();
        }
    }

    private bool DestinationReached()
    {
        return !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance;
    }

    private void ChooseRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;
        NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, walkRadius, NavMesh.AllAreas);
        _agent.SetDestination(navHit.position);
    }
}