using UnityEngine;

/// <summary>
/// Abstract base class representing a state within a Finite State Machine (FSM).
/// All custom states should inherit from this class and override the Enter, Execute, and Exit methods
/// to implement state-specific behavior.
/// </summary>
/// <remarks>
/// <para>
/// This class is marked as Serializable to allow configuration in the Unity Inspector.
/// The RequireComponent attribute ensures that any GameObject with a State component also has an FSM component.
/// </para>
/// <para>
/// State Lifecycle:
/// <list type="bullet">
/// <item><description><see cref="LateStart"/>: Called once during FSM initialization for all states</description></item>
/// <item><description><see cref="Enter"/>: Called when transitioning into this state</description></item>
/// <item><description><see cref="Execute"/>: Called every frame while this state is active</description></item>
/// <item><description><see cref="Exit"/>: Called when transitioning out of this state</description></item>
/// </list>
/// </para>
/// </remarks>
[RequireComponent(typeof(FSM))]
[System.Serializable]
public abstract class State : MonoBehaviour
{
    /// <summary>
    /// Reference to the parent FSM component that manages this state.
    /// Automatically set by <see cref="SetStateProprieties"/> and used to trigger state transitions.
    /// </summary>
    protected FSM fSM;

    /// <summary>
    /// Gets the name of this state, automatically set to the class type name.
    /// Used for debugging and identifying states in log messages.
    /// </summary>
    /// <value>The name of the state's type (e.g., "IdleState", "WalkingState").</value>
    public string StateName {get; protected set; }
    
    /// <summary>
    /// Initializes core state properties including the FSM reference and state name.
    /// Should be called in derived state classes during initialization.
    /// </summary>
    /// <remarks>
    /// This method retrieves the FSM component and sets the StateName to the derived class's type name.
    /// </remarks>
    protected void SetStateProprieties()
    {
        fSM = GetComponent<FSM>();

        StateName = GetType().Name;
    }

    /// <summary>
    /// Called once for each state during FSM initialization, before the first state's Enter method.
    /// Override this method to perform state-specific initialization that needs to happen before any state becomes active.
    /// </summary>
    /// <remarks>
    /// This method exists because Unity's Start() execution order across components is unpredictable.
    /// Since the FSM requires its Start() to run first to activate the initial state, this method
    /// provides a guaranteed initialization point that runs during the FSM's startup sequence.
    /// </remarks>
    public virtual void LateStart(){}
    
    /// <summary>
    /// Called when the FSM transitions into this state.
    /// Override this method to implement state-specific entry behavior such as starting animations,
    /// playing sounds, or initializing state variables.
    /// </summary>
    /// <remarks>
    /// This method is invoked by <see cref="FSM.ChangeState"/> after the previous state's Exit method.
    /// The base implementation logs the state entry to the console.
    /// </remarks>
    public virtual void Enter() {
      Debug.Log($"Entering {StateName} State");
    }

    /// <summary>
    /// Called every frame while this state is active to perform ongoing state logic.
    /// Override this method to implement the core behavior of the state, such as movement logic,
    /// decision-making, or condition checking that may trigger state transitions.
    /// </summary>
    /// <remarks>
    /// This method is invoked by the FSM's Update loop as long as this state remains the current state.
    /// The base implementation logs the state execution to the console each frame.
    /// </remarks>
    public virtual void Execute() { 
     Debug.Log($"Executing {StateName} State");
    }

    /// <summary>
    /// Called when the FSM transitions out of this state to another state.
    /// Override this method to implement state-specific cleanup behavior such as stopping animations,
    /// resetting variables, or releasing resources.
    /// </summary>
    /// <remarks>
    /// This method is invoked by <see cref="FSM.ChangeState"/> before the new state's Enter method.
    /// The base implementation logs the state exit to the console.
    /// </remarks>
    public virtual void Exit() { 
      // Debug.Log($"Exiting {StateName} State");
    }
}