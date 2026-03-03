using Oculus.Platform;
using UnityEngine;

[RequireComponent(typeof(AnimalUtility))]

public class AnimalState: AnimatableState
{   
    [Header("Increase Stats Rates")]
    [SerializeField]
    [Range(0f, 1f)]
    private float hungerIncreaseRate = 0.1f;
    
    [SerializeField]
    [Range(0f, 1f)]
    private float boredomIncreaseRate = 0.01f;

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
    }

    public override void Enter()
    {
        base.Enter();

        IncreaseStats();


        Debug.Log("Animal Current State: " + StateName + " | Current Animation: " + _currentAnimationHashName);
    }

    public override void Execute()
    {
        base.Execute(); 
    }

    public override void Exit()
    {
        base.Exit();

        _readyToTransition = false;

              Debug.Log(" Exit Animal Current State: " + StateName + " | Current Animation: " + _currentAnimationHashName);
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