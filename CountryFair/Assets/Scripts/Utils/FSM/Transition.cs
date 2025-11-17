/// <summary>
/// Represents a transition between two states in the FSM.
/// Serializable to allow configuration in the Unity Inspector.
/// </summary>
[System.Serializable]
public class Transition
{
    /// <summary>
    /// Name of the transition.
    /// </summary>
    public string name;

    /// <summary>
    /// Source state from which the transition originates. Null indicates a wildcard (any state).
    /// </summary>
    public State from;

    /// <summary>
    /// Target state to which the transition leads.
    /// </summary>
    public State to;
}