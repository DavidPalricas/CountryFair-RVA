using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

public class AnimalUtility: MonoBehaviour
{
    public struct Stats
    {
        public float hunger;
        public float boredom;
        public float fatigue;
    }

    public Stats stats;

    // Maps trigger name -> stat getter. States register themselves on Awake.
    private readonly Dictionary<string, Func<float>> _registeredActions = new();


    private void Awake()
    {
       InitializeStats();
    }

    private void InitializeStats()
    {
        stats = new Stats()
        {
            hunger = UnityEngine.Random.value,
            boredom = UnityEngine.Random.value,
            fatigue = UnityEngine.Random.value
        };
    }

    /// <summary>
    /// Called by each AnimalState subclass on Awake to register itself.
    /// triggerName must match the Animator trigger (e.g. "GoEat", "GoIdle", "GoWalk").
    /// statGetter is a lambda that returns the relevant stat value at decision time.
    /// </summary>
    public void RegisterAction(string triggerName, Func<float> statGetter)
    {
        _registeredActions[triggerName] = statGetter;
    }


    public string DecideNextAction(Animator animator)
    {   
        Dictionary<string, float> actions = new();

        foreach (KeyValuePair<string, Func<float>> entry in _registeredActions)
        {
             actions[entry.Key] = entry.Value();
        }
           
        string actionChoosen = actions.OrderByDescending(x => x.Value).First().Key;

        animator.SetTrigger(actionChoosen);

        return actionChoosen;
    }
}