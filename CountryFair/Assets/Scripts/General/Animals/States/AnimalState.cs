using Oculus.Platform;
using UnityEngine;

/// <summary>
/// Abstract base class for animal behavior states (Eat, Walk, Idle).
/// On each entry it increments the stats that this activity worsens, then lets
/// <see cref="AnimalUtility.DecideNextAction"/> pick the next state when activity ends.
/// </summary>
[RequireComponent(typeof(AnimalUtility))]
public class AnimalState: AnimatableState
{
    [Header("Increase Stats Rates")]
    /// <summary>Rate at which hunger increases every time this state is entered (0–1).</summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float hungerIncreaseRate = 0.1f;

    /// <summary>Rate at which boredom increases every time this state is entered (0–1).</summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float boredomIncreaseRate = 0.01f;

    /// <summary>Rate at which fatigue increases every time this state is entered (0–1).</summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float fatigueIncreaseRate = 0.01f;

    protected Animator _animator;

    protected AnimalUtility _animalUtility;

    protected float _hungerStat = 0f;

    protected float _boredomStat = 0f;

    protected float _fatigueStat = 0f;

   

    protected string transitionName = string.Empty;


    protected bool _readyToTransition = false;

    protected override void Awake()
    {   
        base.Awake();

        _animalUtility = GetComponent<AnimalUtility>();

        AnimalUtility.Stats animalStats = _animalUtility.stats;

        _hungerStat = animalStats.hunger;
        _boredomStat = animalStats.boredom;
        _fatigueStat = animalStats.fatigue;

        SetStateProprieties();

        // Each subclass registers its own trigger + stat so AnimalUtility
        // knows which actions are available without any hardcoded list.
        RegisterActionInUtility();
    }

    /// <summary>
    /// Override in subclasses to register the Animator trigger and the stat
    /// this state is responsible for recovering.
    /// Example:  _animalUtility.RegisterAction("GoEat", () => _animalUtility.stats.hunger);
    /// </summary>
    protected virtual void RegisterActionInUtility() { }

    public override void Enter()
    {
        base.Enter();

        IncreaseStats();
    }

    public override void Execute()
    {
        base.Execute(); 
    }

    public override void Exit()
    {
        base.Exit();

        _readyToTransition = false;
    }


    private void IncreaseStats()
    {    
        _hungerStat = Mathf.Min(_hungerStat + hungerIncreaseRate, 1f);
        _boredomStat = Mathf.Min(_boredomStat + boredomIncreaseRate, 1f);
        _fatigueStat = Mathf.Min(_fatigueStat + fatigueIncreaseRate, 1f);
    }

    protected void UpdateStats()
    {
        _animalUtility.stats.hunger = _hungerStat;
        _animalUtility.stats.boredom = _boredomStat; 
        _animalUtility.stats.fatigue = _fatigueStat;

        // Debug.Log("Animal Current State: " + GetType().Name + " | Hunger: " + _animalUtility.stats.hunger + " | Boredom: " + _animalUtility.stats.boredom + " | Fatigue: " + _animalUtility.stats.fatigue);
    }
}