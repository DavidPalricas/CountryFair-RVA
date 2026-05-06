using UnityEngine;
using DG.Tweening;

/// <summary>
/// Dog FSM state that performs a DOTween jump animation, then transitions to <c>CatchFrisbee</c>
/// once the jump animation clip starts playing.
/// </summary>
public class Jump: DogState
{
    [Header("Jumps Settings")]
    /// <summary>Total duration of the jump sequence in seconds.</summary>
    [SerializeField]
    private float jumpDuration = 1.6f;

    /// <summary>Height of the jump arc.</summary>
    [SerializeField]
    private float jumpPower = 0.3f;
    /// <summary>Number of bounces during the jump sequence.</summary>
    [SerializeField]
    private int jumpNumbers = 2;

     protected override void Awake()
    {   
        base.Awake();

        if (animator == null)
        {
            Debug.LogError("Animator reference is null in Jump state.");
            return;
        }
    }
    
    public override void Enter()
    {
        base.Enter();

        PerformJump();
    }

    public override void Execute()
    {
        base.Execute();

        if (IsPlayingNewAnimation())
        {
            fSM.ChangeState("CatchFrisbee");

            return;
        }
    }

    private void PerformJump()
    {
        transform.DOJump(transform.position, jumpPower, jumpNumbers, jumpDuration).OnComplete( () => 
        {   
            animator.SetBool("StopAnim", false);
            animator.SetFloat("Speed", 1f);
        });
    }

    public override void Exit()
    {
       base.Exit();
    }    
}