using DG.Tweening;
using UnityEngine;


public class TentPlaceHolder : MonoBehaviour
{   
    public Transform placeHolderForInteractors;

    [Header("Squash & Stretch")]
    [SerializeField]
    private float bounceHeight = 0.15f;
    [SerializeField]
     private float bounceDuration = 1.2f;
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


    public int tentNumber = 1;

    private void Awake()
    {    

        if (placeHolderForInteractors == null){
            Debug.LogError("Place Holder for Interactors is not assigned in the inspector.");

            return;
        }
        _originalScale = transform.localScale;
        _originalLocalPosition = transform.localPosition;
    }

    private void Start()
    {
        StartSquashStretch();
    }

    private void StartSquashStretch()
    {
        float halfDuration = bounceDuration * HALF_DURATION_RATIO;
        float recoverDuration = bounceDuration * RECOVER_DURATION_RATIO;

        Vector3 stretchScale = new Vector3(
            _originalScale.x * (1f - squashAmount * STRETCH_NARROW_FACTOR),
            _originalScale.y * (1f + squashAmount),
            _originalScale.z * (1f - squashAmount * STRETCH_NARROW_FACTOR));

        Vector3 squashScale = new Vector3(
            _originalScale.x * (1f + squashAmount),
            _originalScale.y * (1f - squashAmount * SQUASH_FLATTEN_FACTOR),
            _originalScale.z * (1f + squashAmount));

        _squashStretchSequence = DOTween.Sequence();

            // Rise: body elongates and narrows
        _squashStretchSequence.Append(
            transform.DOLocalMoveY(_originalLocalPosition.y + bounceHeight, halfDuration)
                .SetEase(Ease.OutSine));

        _squashStretchSequence.Join(
            transform.DOScale(stretchScale, halfDuration)
                .SetEase(Ease.OutSine));

            // Fall: body flattens and widens
        _squashStretchSequence.Append(
            transform.DOLocalMoveY(_originalLocalPosition.y, halfDuration)
                .SetEase(Ease.InSine));

        _squashStretchSequence.Join(
            transform.DOScale(squashScale, halfDuration)
                .SetEase(Ease.InSine));

            // Recover: body returns to original scale
        _squashStretchSequence.Append(
            transform.DOScale(_originalScale, recoverDuration)
                .SetEase(Ease.InSine));

        _squashStretchSequence.SetLoops(INFINITE_LOOPS, LoopType.Restart);
    }

    private void OnDestroy()
    {
        _squashStretchSequence?.Kill();
        transform.DOKill();
    }



    private void OnTriggerEnter(Collider other){
        if (other.gameObject.CompareTag("Tent")){
            /* The collider for the tent is a child (the model) of the tent GameObject with the script
                because this gameobject has the collider that detects the distance grabb interaction
            */
            MiniGameTent tent = other.gameObject.GetComponent<MiniGameTent>();

            tent.UpdateTentPlaceHolder(this);
        }
    }
     
    
      private void OnTriggerExit(Collider other){
        if (other.gameObject.CompareTag("Tent")){
            /* The collider for the tent is a child (the model) of the tent GameObject with the script
                because this gameobject has the collider that detects the distance grabb interaction
            */
            MiniGameTent tent = other.gameObject.GetComponent<MiniGameTent>();

            if (tent == null){
                Debug.LogError("GameObject with 'Tent' tag must have a MiniGameTent component.");

                return;
            }

            tent.UpdateTentPlaceHolder(null);
        }
    }
}
