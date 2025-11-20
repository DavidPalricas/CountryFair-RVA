using UnityEngine;

/// <summary>
/// Represents the idle state of the dog in the Frisbee game.
/// Activates the catch area and transitions to the <see cref="CatchFrisbee"/> state when the frisbee lands.
/// </summary>

public class DogIdle : DogState
{   
    /// <summary>
    /// Reference to the area where the frisbee can be caught.
    /// </summary>
    [SerializeField]
    private GameObject scoreArea;

    /// <summary>
    /// Initializes the state by deactivating the catch area and setting the state name.
    /// Unity callback called when the script instance is being loaded.
    /// </summary>
    protected override void Awake()
    {   
        base.Awake();
        
        scoreArea.SetActive(false);
    }

    /// <summary>
    /// Initializes references to the player Transform and game manager by calling the base LateStart method.
    /// </summary>
    public override void LateStart()
    {
        base.LateStart();
    }
    
    /// <summary>
    /// Activates the frisbee catch area when entering the idle state.
    /// </summary>
    public override void Enter()
    {
        base.Enter();

        if (_gameManager == null)
        {
            Debug.LogError("GameManager reference is null in DogIdle state.");
            return;
        }

        _gameManager.currentTargetPos = transform.position;

        scoreArea.SetActive(true);
    }

    /// <summary>
    /// Turns the dog to face the player while idle.
    /// </summary>
    public override void Execute()
    {
        base.Execute();

        RotateDogTowardsTarget(_playerTransform);
    }

    /// <summary>
    /// Triggers the transition to the "FrisbeeLanded" state when the frisbee has landed  to the <see cref="CatchFrisbee"/> state.
    /// </summary>
    public void FrisbeeLanded()
    {
        fSM.ChangeState("FrisbeeLanded");
    }

    /// <summary>
    /// Deactivates the frisbee catch area when exiting the idle state.
    /// </summary>
    public override void Exit()
    {
        base.Exit();
        scoreArea.SetActive(false);
    }
}
