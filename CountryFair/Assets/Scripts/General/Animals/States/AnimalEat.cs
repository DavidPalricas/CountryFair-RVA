using UnityEngine;

/// <summary>
/// Animal state that plays the eating animation and reduces hunger.
/// Runs for a random duration, then calls <see cref="AnimalUtility.DecideNextAction"/> to choose the next state.
/// </summary>
public class AnimalEat: AnimalState
{
    [Header("Recovery Stats Rates")]
    /// <summary>Amount hunger decreases each time the animal enters the eating state (0–1).</summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float hungerRecoveryRate = 0.1f;

    [Header("Eating Time Settings")]
    /// <summary>Minimum eating duration in seconds.</summary>
    [SerializeField]
    private float minTimeToEat = 30f;

    /// <summary>Maximum eating duration in seconds.</summary>
    [SerializeField]
    private float maxTimeToEat = 60f;


    private float _timeToEat;

    protected override void RegisterActionInUtility()
    {
        _animalUtility.RegisterAction("GoEat", () => _animalUtility.stats.hunger);
    }

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
            _readyToTransition = true;
            transitionName = _animalUtility.DecideNextAction(animator);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}