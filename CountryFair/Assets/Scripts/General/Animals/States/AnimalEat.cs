using UnityEngine;

public class AnimalEat: AnimalState
{   
    [Header("Recovery Stats Rates")]
    [SerializeField]
    [Range(0f, 1f)]
    private float hungerRecoveryRate = 0.1f;
    
    [Header("Eating Time Settings")]
    [SerializeField]
    private float minTimeToEat = 30f;

    [SerializeField]
    private float maxTimeToEat = 60f;


    private float _timeToEat;

    public override void Enter()
    {
        base.Enter();

        _timeToEat = Utils.RandomValueInRange(minTimeToEat, maxTimeToEat) + Time.time;

        _hungerStat = Mathf.Max(_hungerStat - hungerRecoveryRate, 0f);

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

        if (Time.time >= _timeToEat && !_readyToTransition)
        {   
               Debug.Log("Animal STUCK: " + StateName);
            _readyToTransition = true;
            transitionName = _animalUtility.DecideNextAction(animator);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}