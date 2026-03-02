using UnityEngine;
using DG.Tweening;

public class Jump: DogState
{  
    [Header("Jumps Settings")]
    [SerializeField]
    private float jumpDuration = 1.6f;

    [SerializeField]
    private float jumpPower = 0.3f;
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

        Invoke(nameof(JumpDog), animationsCooldown);

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

    private void JumpDog()
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