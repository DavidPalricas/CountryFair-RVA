using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AnimalWalk: AnimalState
{   
    [Header("Recovery Stats Rates")]
    [SerializeField]
    [Range(0f, 1f)]
    private float boredomRecoveryRate = 0.01f;
    
    private NavMeshAgent _agent;


    private int _agentDefaultPriority;

    protected override void Awake()
    {   
        base.Awake();
        _agent = GetComponent<NavMeshAgent>();
        
        const int AVOIDANCE_PRIORITY_MIN_VALUE = 1;
        const int AVOIDANCE_PRIORITY_MAX_VALUE = 99;
        _agentDefaultPriority = Random.Range(AVOIDANCE_PRIORITY_MIN_VALUE , AVOIDANCE_PRIORITY_MAX_VALUE);
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

        if (AnimalStoped())
        {
            string transitionName = _animalUtility.DecideNextAction();
            fSM.ChangeState(transitionName);
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