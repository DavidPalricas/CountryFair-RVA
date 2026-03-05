using System;
using UnityEngine;

public class AnimalIdle: AnimalState
{   
    [Header("Recovery Stats Rates")]
    [SerializeField]
    [Range(0f, 1f)]
    private float fatigueRecoveryRate = 0.01f;

    [Header("Idle Time Settings")]
    [SerializeField]
    private float minTimeToIdle = 30f;

    [SerializeField]
    private float maxTimeToIdle = 60f;

    private float _timeIdle;

    protected override void RegisterActionInUtility()
    {
        _animalUtility.RegisterAction("GoIdle", () => _animalUtility.stats.fatigue);
    }

    public override void Enter()
    {
        base.Enter();

        _timeIdle = Utils.RandomValueInRange(minTimeToIdle, maxTimeToIdle) + Time.time;

        _fatigueStat = Mathf.Max(_fatigueStat - fatigueRecoveryRate, 0f);

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

        if (Time.time >= _timeIdle && !_readyToTransition)
        
        {   
            _readyToTransition = true;
            transitionName = _animalUtility.DecideNextAction(animator);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}