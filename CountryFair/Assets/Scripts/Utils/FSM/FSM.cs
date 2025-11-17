using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a Finite State Machine used to manage NPC (customer) states.
/// </summary>
public class FSM : MonoBehaviour
{
    /// <summary>
    /// List of states available in the FSM.
    /// </summary>
    [SerializeField]
    private List<State> states;

    /// <summary>
    /// List of transitions between states in the FSM.
    /// </summary>
    [SerializeField] 
    private List<Transition> transitions = new();

    /// <summary>
    /// Gets the current active state of the FSM.
    /// </summary>
    public State CurrentState { get; private set; }

    /// <summary>
    /// Initializes the FSM by setting the current state to the first state in the list and calling its Enter method.
    /// Unity callback called before the first frame update.
    /// </summary>
    private void Start()
    {
        CurrentState = states[0];
        CurrentState.Enter();
    }

    /// <summary>
    /// Executes the current state's logic.
    /// Unity callback called once per frame.
    /// </summary>
    private void Update()
    {
        CurrentState.Execute();
    }

    /// <summary>
    /// Changes the current state of the FSM using the specified transition.
    /// </summary>
    /// <remarks>
    /// Iterates through the list of transitions to find a matching transition by name.
    /// A transition matches if its name equals the parameter (case-insensitive, whitespace-insensitive) and either:
    /// the transition's from state is null (wildcard transition), or the current state matches the transition's from state.
    /// If found, calls Exit on the current state, updates to the new state, and calls Enter on the new state.
    /// Logs a warning if no matching transition is found.
    /// </remarks>
    /// <param name="transitionName">Name of the transition to execute.</param>
    public void ChangeState (string transitionName)
    {   
        foreach (var transition in transitions)
        {
            // Removes whitespaces and converts to lowercase to avoid case sensitivity and whitespaces issues
            if (transition.name.Replace(" ", "").ToLower() == transitionName.Replace(" ", "").ToLower() && (transition.from == null || CurrentState == transition.from))
            {
                CurrentState.Exit();
                CurrentState = transition.to;
                CurrentState.Enter();

                return;
            }
        }

        Debug.LogWarning($"Transition {transitionName} not found for customer {gameObject.name}, from {CurrentState.StateName}");
    }
}
