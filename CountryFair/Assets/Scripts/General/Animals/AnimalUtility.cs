using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class AnimalUtility: MonoBehaviour
{
    public struct Stats
    {
        public float hunger;
        public float boredom;
        public float fatigue;
    }

    public Stats stats;


    public Animator Animator { get; private set; }


    private void Awake()
    {
       InitializeStats();
        Animator = GetComponent<Animator>();
    }

    private void InitializeStats()
    {
        stats = new Stats()
        {
            hunger = Random.value,
            boredom = Random.value,
            fatigue = Random.value
        };
    }


    public string DecideNextAction()
    {   
        Dictionary<string, float> actions = new()
        {
            { "GoEat", stats.hunger },
            { "GoIdle", stats.fatigue },
            { "GoWalk", stats.boredom }
        };

        string actionChoosen = actions.OrderByDescending(x => x.Value).First().Key;

        Animator.SetTrigger(actionChoosen);

        return actionChoosen;
    }
}