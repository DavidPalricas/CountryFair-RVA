using DG.Tweening;
using UnityEngine;

public class TentFishAnim : MonoBehaviour
{
    [SerializeField] 
    private float amplitude = 0.3f;

    [SerializeField] 
    private float duration = 1.5f;

    [SerializeField] 
    private Ease easeType = Ease.InOutSine;

    [SerializeField] 
    private float randomOffset = 0.3f;

    private void Start()
    {
        float offset = Random.Range(-randomOffset, randomOffset);
        float actualDuration = duration + Random.Range(-0.3f, 0.3f);

        transform.DOMoveY(transform.position.y + amplitude, actualDuration)
            .SetEase(easeType)
            .SetLoops(-1, LoopType.Yoyo)
            .SetDelay(Random.Range(0f, actualDuration)); 
    }

    private void OnDestroy()
    {
        transform.DOKill(); 
    }
}