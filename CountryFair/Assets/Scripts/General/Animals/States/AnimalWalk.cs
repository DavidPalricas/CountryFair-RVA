using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Animal state that navigates to a random nearby NavMesh position and reduces boredom.
/// Uses a <see cref="NavMeshAgent"/> with a randomized avoidance priority so herded animals don't all cluster.
/// Transitions to the next state once the destination is reached.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AnimalWalk: AnimalState
{
    [Header("Recovery Stats Rates")]
    /// <summary>Amount boredom decreases each time the animal enters the walk state (0–1).</summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float boredomRecoveryRate = 0.01f;
    
    private NavMeshAgent _agent;


    private int _agentDefaultPriority;

    protected override void Awake()
    {   
        base.Awake();
        _agent = GetComponent<NavMeshAgent>();
        _agentDefaultPriority = _agent.avoidancePriority;   
    }

    protected override void RegisterActionInUtility()
    {
        _animalUtility.RegisterAction("GoWalk", () => _animalUtility.stats.boredom);
    }

    public override void Enter()
    {
        base.Enter();

        _agent.avoidancePriority = _agentDefaultPriority;

        _agent.isStopped = false;

        SetRandomDestination();

        _boredomStat = Mathf.Max(_boredomStat - boredomRecoveryRate, 0f);

        UpdateStats();
    }

    public override void Execute()
    {
        base.Execute();

        if (IsPlayingNewAnimation())
        {
             fSM.ChangeState(transitionName);
             return;
        }

        if (AnimalStoped() && !_readyToTransition)
        {   
             _readyToTransition = true;

            transitionName = _animalUtility.DecideNextAction(animator);

        }
    }

    public override void Exit()
    {
        base.Exit();

         _agent.isStopped = true;
          
        // When is stopped has max prioty to avoid being pushed by other agents, but when is walking it has a random priority to avoid all agents having the same priority and getting stuck.
         _agent.avoidancePriority = 0;
    }


    private void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }

    private bool AnimalStoped()
    {
        return !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance;
    }
}