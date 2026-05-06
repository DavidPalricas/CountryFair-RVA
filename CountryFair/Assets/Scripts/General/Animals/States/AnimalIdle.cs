using System;
using UnityEngine;

/// <summary>
/// Animal state that plays the idle animation and reduces fatigue.
/// Runs for a random duration, then calls <see cref="AnimalUtility.DecideNextAction"/> to choose the next state.
/// </summary>
public class AnimalIdle: AnimalState
{
    [Header("Recovery Stats Rates")]
    /// <summary>Amount fatigue decreases each time the animal enters the idle state (0–1).</summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float fatigueRecoveryRate = 0.01f;

    [Header("Idle Time Settings")]
    /// <summary>Minimum idle duration in seconds.</summary>
    [SerializeField]
    private float minTimeToIdle = 30f;

    /// <summary>Maximum idle duration in seconds.</summary>
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