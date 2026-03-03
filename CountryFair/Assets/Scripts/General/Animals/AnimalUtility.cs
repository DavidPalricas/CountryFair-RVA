using UnityEngine;
using System.Linq;
using System.Collections.Generic;


public class AnimalUtility: MonoBehaviour
{
    public struct Stats
    {
        public float hunger;
        public float boredom;
        public float fatigue;
    }

    public Stats stats;


    private void Awake()
    {
       InitializeStats();
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


    public string DecideNextAction(Animator animator)
    {   
        Dictionary<string, float> actions = new()
        {
            { "GoEat", stats.hunger },
            { "GoIdle", stats.fatigue },
            { "GoWalk", stats.boredom }
        };

        string actionChoosen = actions.OrderByDescending(x => x.Value).First().Key;

        animator.SetTrigger(actionChoosen);


        return actionChoosen;
    }
}