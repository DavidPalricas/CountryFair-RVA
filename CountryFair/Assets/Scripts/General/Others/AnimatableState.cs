using  UnityEngine;

public class AnimatableState : State
{   
   [SerializeField]
    protected Animator animator;

    protected int _currentAnimationHashName = 0;

    protected virtual void Awake()
    {
        if (animator == null)
        {
            Debug.LogError("Animator reference is null in AnimatableState.");
            return;
        }

         SetStateProprieties();
    }

    public override void Enter()
    {
        base.Enter();

        _currentAnimationHashName = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
    }

    public override void Execute()
    {
        base.Execute();
    }

    public override void Exit()
    {
        base.Exit();
    }

 

    protected bool IsPlayingNewAnimation()
    {
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash != _currentAnimationHashName;
    }
}

