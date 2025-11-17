using UnityEngine;

/// <summary>
/// Abstract base class representing a state in the FSM.
/// Serializable to allow configuration in the Unity Inspector.
/// </summary>
[System.Serializable]
public abstract class State : MonoBehaviour
{
    /// <summary>
    /// Reference to the FSM that owns this state.
    /// </summary>
    protected FSM fSM;

    /// <summary>
    /// Gets or sets the name of this state.
    /// </summary>
    public string StateName {get; protected set; }

    /// <summary>
    /// Called when entering this state to perform initialization actions.
    /// Override in derived classes to implement state-specific entry behavior.
    /// </summary>
    public virtual void Enter() {
       // Debug.Log($"Entering {StateName} State");
    }

    /// <summary>
    /// Called every frame while this state is active to perform ongoing state logic.
    /// Override in derived classes to implement state-specific execution behavior.
    /// </summary>
    public virtual void Execute() { 
     // Debug.Log($"Executing {StateName} State");
    }

    /// <summary>
    /// Called when exiting this state to perform cleanup actions.
    /// Override in derived classes to implement state-specific exit behavior.
    /// </summary>
    public virtual void Exit() { 
       // Debug.Log($"Exiting {StateName} State");
    }
}