using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

/// <summary>
/// Maintains the shared behavioral stats for an animal NPC (hunger, boredom, fatigue)
/// and selects the next behavior state by comparing each registered stat value.
/// Individual <see cref="AnimalState"/> subclasses register themselves on Awake.
/// </summary>
public class AnimalUtility: MonoBehaviour
{
    /// <summary>Snapshot of the three animal needs that drive behavior decisions.</summary>
    public struct Stats
    {
        /// <summary>How hungry the animal is (0 = not hungry, 1 = starving).</summary>
        public float hunger;
        /// <summary>How bored the animal is (0 = content, 1 = restless).</summary>
        public float boredom;
        /// <summary>How tired the animal is (0 = energetic, 1 = exhausted).</summary>
        public float fatigue;
    }

    /// <summary>Current stats snapshot, read and updated by the active animal state.</summary>
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


    /// <summary>
    /// Picks the action whose registered stat is currently highest, sets the matching Animator trigger,
    /// and returns the trigger name so the calling state can store the expected transition.
    /// </summary>
    /// <param name="animator">The animal's Animator, used to set the winning trigger.</param>
    /// <returns>The Animator trigger name of the chosen action (e.g., "GoEat", "GoWalk", "GoIdle").</returns>
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