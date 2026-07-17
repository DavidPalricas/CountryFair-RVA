using DG.Tweening;
using UnityEngine;

/// <summary>
/// Marks a slot where a <see cref="MiniGameTent"/> can be placed and plays a looping
/// squash-and-stretch bounce animation to make the empty slot visible to the player.
/// Trigger collider callbacks inform the active tent which placeholder it is hovering over.
/// </summary>
public class TentPlaceHolder : PlaceHolder
{
    /// <summary>Transform used to position the tent's play button when a tent snaps to this slot.</summary>
    public Transform miniGameButtonPlaceHolderTransform;

    [Header("Squash & Stretch")]
    /// <summary>Child transform that carries the visual model being animated.</summary>
    [SerializeField]
    private Transform modelTransform;

    /// <summary>How high the model rises above its rest position during the bounce, in local units.</summary>
    [SerializeField]
    private float bounceHeight = 0.15f;

    /// <summary>Total duration of one full bounce cycle (rise + fall + recover), in seconds.</summary>
    [SerializeField]
    private float bounceDuration = 1.2f;

    /// <summary>Intensity of the squash and stretch deformation (0 = none, 0.5 = maximum).</summary>
    [Range(0f, 0.5f)]
    [SerializeField]
    private float squashAmount = 0.12f;

    private const float HALF_DURATION_RATIO = 0.45f;
    private const float RECOVER_DURATION_RATIO = 0.1f;
    private const float STRETCH_NARROW_FACTOR = 0.7f;
    private const float SQUASH_FLATTEN_FACTOR = 0.5f;
    private const int INFINITE_LOOPS = -1;

    private Vector3 _originalScale;
    private Vector3 _originalLocalPosition;
    private Sequence _squashStretchSequence;

    /// <summary>Caches the model's original scale and local position as the animation baseline.</summary>
    private void Awake()
    {
        if (miniGameButtonPlaceHolderTransform == null || modelTransform == null)
        {
            Debug.LogError("One or more required transforms are not assigned in TentPlaceHolder script.");

            return;
        }

        _originalScale = modelTransform.localScale;
        _originalLocalPosition = modelTransform.localPosition;
    }

    /// <summary>Starts the looping squash-and-stretch bounce sequence.</summary>
    private void Start()
    {
        StartSquashStretch();
    }

    /// <summary>
    /// Builds and plays the DOTween Sequence that cycles through rise (stretch), fall (squash),
    /// and recover phases on an infinite loop.
    /// </summary>
    private void StartSquashStretch()
    {
        float halfDuration = bounceDuration * HALF_DURATION_RATIO;
        float recoverDuration = bounceDuration * RECOVER_DURATION_RATIO;

        Vector3 stretchScale = new (
            _originalScale.x * (1f - squashAmount * STRETCH_NARROW_FACTOR),
            _originalScale.y * (1f + squashAmount),
            _originalScale.z * (1f - squashAmount * STRETCH_NARROW_FACTOR));

        Vector3 squashScale = new(
            _originalScale.x * (1f + squashAmount),
            _originalScale.y * (1f - squashAmount * SQUASH_FLATTEN_FACTOR),
            _originalScale.z * (1f + squashAmount));

        _squashStretchSequence = DOTween.Sequence();

            // Rise: body elongates and narrows
        _squashStretchSequence.Append(
            modelTransform.DOLocalMoveY(_originalLocalPosition.y + bounceHeight, halfDuration)
                .SetEase(Ease.OutSine));

        _squashStretchSequence.Join(
            modelTransform.DOScale(stretchScale, halfDuration)
                .SetEase(Ease.OutSine));

            // Fall: body flattens and widens
        _squashStretchSequence.Append(
            modelTransform.DOLocalMoveY(_originalLocalPosition.y, halfDuration)
                .SetEase(Ease.InSine));

        _squashStretchSequence.Join(
            modelTransform.DOScale(squashScale, halfDuration)
                .SetEase(Ease.InSine));

            // Recover: body returns to original scale
        _squashStretchSequence.Append(
            modelTransform.DOScale(_originalScale, recoverDuration)
                .SetEase(Ease.InSine));

        _squashStretchSequence.SetLoops(INFINITE_LOOPS, LoopType.Restart);
    }

    /// <summary>Kills the bounce sequence and any stray tweens on the model to prevent callbacks after destruction.</summary>
    private void OnDestroy()
    {
        _squashStretchSequence?.Kill();
        modelTransform.DOKill();
    }
}
