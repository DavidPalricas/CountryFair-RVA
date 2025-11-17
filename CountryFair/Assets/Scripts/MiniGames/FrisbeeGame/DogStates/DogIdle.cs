using UnityEngine;

/// <summary>
/// Represents the idle state of the dog in the Frisbee game.
/// Activates the catch area and transitions to catch state when the frisbee lands.
/// </summary>
public class DogIdle : State
{   
    /// <summary>
    /// Reference to the area where the frisbee can be caught.
    /// </summary>
    [SerializeField]
    private AreaToCatchFrisbee areaToCatchFrisbee;

    /// <summary>
    /// Initializes the state by deactivating the catch area and setting the state name.
    /// Unity callback called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        areaToCatchFrisbee.gameObject.SetActive(false);
        StateName = "DogIdle";
    }

    /// <summary>
    /// Activates the frisbee catch area when entering the idle state.
    /// </summary>
    public override void Enter()
    {
        base.Enter();
        areaToCatchFrisbee.gameObject.SetActive(true);
    }

    /// <summary>
    /// Monitors the catch area for a landed frisbee and transitions to catch state when detected.
    /// </summary>
    public override void Execute()
    {
        base.Execute();

        if (areaToCatchFrisbee.frisbeeLanded)
        {   
            areaToCatchFrisbee.frisbeeLanded = false;
            fSM.ChangeState("CatchFrisbee");
        }
    }

    /// <summary>
    /// Deactivates the frisbee catch area when exiting the idle state.
    /// </summary>
    public override void Exit()
    {
        base.Exit();
        areaToCatchFrisbee.gameObject.SetActive(false);
    }
}
