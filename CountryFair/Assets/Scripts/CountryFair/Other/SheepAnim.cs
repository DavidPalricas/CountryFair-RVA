using UnityEngine;
using DG.Tweening;

/// <summary>
/// Animates a sheep with a continuous bouncing motion including squashing and stretching effects.
/// </summary>
public class SheepAnimation : MonoBehaviour
{
    /// <summary>Maximum height the sheep bounces upward.</summary>
    [Header("Animation Settings")]
    [SerializeField]
    private readonly float bounceHeight = 0.5f;

    /// <summary>Duration in seconds for one complete bounce cycle.</summary>
    [SerializeField]
    private readonly float bounceDuration = 0.5f;
    
    /// <summary>Amount of squashing and stretching deformation as a ratio of original scale.</summary>
    [SerializeField]
    private readonly float squashAmount = 0.3f;

    /// <summary>Original scale of the sheep stored on Start.</summary>
    private Vector3 originalScale;

    /// <summary>Sequence controlling the bounce animation.</summary>
    private Sequence bounceSequence;

    /// <summary>
    /// Stores the original scale and initiates the bounce animation.
    /// </summary>
    private void Start()
    {
        originalScale = transform.localScale;
        StartBounceAnimation();
    }

    /// <summary>
    /// Creates and configures the bounce animation sequence with up/down motion and squash/stretch effects.
    /// Loops infinitely with a restart loop type.
    /// </summary>
    private void StartBounceAnimation()
    {
        bounceSequence = DOTween.Sequence();
        
        bounceSequence.Append(transform.DOMoveY(transform.position.y + bounceHeight, bounceDuration / 2)
            .SetEase(Ease.OutQuad));

        bounceSequence.Join(transform.DOScale(new Vector3(
            originalScale.x * (1f + squashAmount),
            originalScale.y * (1f - squashAmount),
            originalScale.z), bounceDuration / 4));

        bounceSequence.Append(transform.DOMoveY(transform.position.y, bounceDuration / 2)
            .SetEase(Ease.InQuad));

        bounceSequence.Join(transform.DOScale(originalScale, bounceDuration / 4)
            .SetDelay(bounceDuration / 4));

        bounceSequence.Insert(bounceDuration / 4, transform.DOScale(new Vector3(
            originalScale.x * (1f - squashAmount / 2),
            originalScale.y * (1f + squashAmount / 2),
            originalScale.z), bounceDuration / 4));

        bounceSequence.SetLoops(-1, LoopType.Restart);
        bounceSequence.SetEase(Ease.OutQuad);
    }
}