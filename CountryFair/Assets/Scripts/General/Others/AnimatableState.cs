using  UnityEngine;

/// <summary>
/// Extends <see cref="State"/> with an Animator reference and a helper to detect when the
/// Animator has transitioned to a new state since <see cref="Enter"/> was called.
/// Base class for all states that drive character animations.
/// </summary>
public class AnimatableState : State
{
    /// <summary>Animator component driven by this state. Assigned in the Inspector.</summary>
    [SerializeField]
    protected Animator animator;

    /// <summary>
    /// Hash of the Animator state that was active when <see cref="Enter"/> was last called.
    /// Used by <see cref="IsPlayingNewAnimation"/> to detect animation transitions.
    /// </summary>
    protected int _currentAnimationHashName = 0;

    /// <summary>
    /// Validates that an Animator is assigned, then calls <see cref="State.SetStateProprieties"/>.
    /// </summary>
    protected virtual void Awake()
    {
        if (animator == null)
        {
            Debug.LogError("Animator reference is null in AnimatableState.");
            return;
        }

         SetStateProprieties();
    }

    /// <summary>Caches the current Animator state hash so <see cref="IsPlayingNewAnimation"/> can detect transitions.</summary>
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

    /// <summary>
    /// Returns true when the Animator has transitioned to a different state since the last <see cref="Enter"/> call.
    /// Used by derived states to detect when a triggered animation has finished playing.
    /// </summary>
    protected bool IsPlayingNewAnimation()
    {
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash != _currentAnimationHashName;
    }
}

