using DG.Tweening;
using UnityEngine;

/// <summary>
/// Animates a fish prop on the Fish Tent with a looping vertical bob in local space.
/// Each instance adds a small random duration variation so multiple fish bob out of sync.
/// </summary>
public class TentFishAnim : MonoBehaviour
{
    /// <summary>Peak height offset above the rest position, in local units.</summary>
    [SerializeField]
    private float amplitude = 0.3f;

    /// <summary>Base duration of one up or down movement in seconds. Actual duration varies by ±0.3 s.</summary>
    [SerializeField]
    private float duration = 1.5f;

    /// <summary>Easing curve applied to each half of the bob cycle.</summary>
    [SerializeField]
    private Ease easeType = Ease.InOutSine;

    /// <summary>Maximum random delay before the bob starts, in seconds, to stagger multiple fish.</summary>
    [SerializeField]
    private float randomOffset = 0.3f;

    /// <summary>Starts the infinite yoyo tween from the local rest position to <see cref="amplitude"/> above it.</summary>
    private void Start()
    {
        float actualDuration = duration + Random.Range(-0.3f, 0.3f);

        transform.DOLocalMoveY(transform.localPosition.y + amplitude, actualDuration)
            .SetEase(easeType)
            .SetLoops(-1, LoopType.Yoyo)
            .SetDelay(Random.Range(0f, actualDuration));
    }

    /// <summary>Kills any active DOTween on this transform to prevent callbacks on a destroyed object.</summary>
    private void OnDestroy()
    {
        transform.DOKill();
    }
}
