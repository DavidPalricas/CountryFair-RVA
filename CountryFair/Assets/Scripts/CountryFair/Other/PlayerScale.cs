using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class PlayerScale : MonoBehaviour
{

   [SerializeField]
   private Transform cameraRigTransform;

   [SerializeField]
   [Tooltip("Player's head (Center Eye Anchor). Used as the pivot of the scale animation, so the player does not drift sideways while growing")]
   private Transform headTransform;

   [SerializeField]
   private float giantScaleFactor = 4f;


   [Header("Scale Animation")]
   [SerializeField]
   [Tooltip("Duration of the growing animation")]
   private float scaleUpDuration = 1f;

   [SerializeField]
   [Tooltip("Duration of the shrinking animation")]
   private float scaleDownDuration = 0.8f;

   [SerializeField]
   [Tooltip("Anticipation squash before growing (Mario feel), as a fraction of the NORMAL height. 0 = no squash, the most comfortable setting")]
   [Range(0f, 0.15f)]
   private float anticipationAmount = 0.04f;

   [SerializeField]
   private float anticipationDuration = 0.12f;


   [Header("Comfort Vignette")]
   [SerializeField]
   [Tooltip("Optional OVRVignette placed on the camera. Narrows the field of view during the animation, which strongly reduces cybersickness")]
   private OVRVignette comfortVignette;

   [SerializeField]
   [Tooltip("Vignette FOV when fully open (no vignette visible)")]
   private float vignetteOpenFieldOfView = 170f;

   [SerializeField]
   [Tooltip("Vignette FOV during the animation. Lower = narrower tunnel = more comfort, less immersion")]
   private float vignetteTunnelFieldOfView = 60f;

   [SerializeField]
   [Tooltip("How long the vignette takes to close. It always opens back a bit slower, so it is not noticed")]
   private float vignetteCloseDuration = 0.25f;


   [Header("Events")]
   [SerializeField]
   private UnityEvent<AudioManager.GameSoundEffects> onScaleToGiant;

    [SerializeField]
    private UnityEvent<AudioManager.GameSoundEffects> onScaleToNormal;

   [SerializeField]
   private UnityEvent onScaleAnimationStarted;

   [SerializeField]
   private UnityEvent onScaleAnimationCompleted;

   private bool _isGiantMode = false;

   private float originalScale = 1f;

   private Sequence _scaleSequence;

   private Tween _vignetteTween;

   /// <summary>
   /// The scale being animated, stored in logarithmic space. See <see cref="AnimateScale"/> for the reason.
   /// </summary>
   private float _animatedLogScale;


   private AudioManager.GameSoundEffects scaleToGiantSoundEffect = AudioManager.GameSoundEffects.SCALE_TO_GIANT;
   private AudioManager.GameSoundEffects scaleToNormalSoundEffect = AudioManager.GameSoundEffects.SCALE_TO_NORMAL;


  private void Awake()
   {
       if (cameraRigTransform == null)
       {
           Debug.LogError("Camera Rig Transform reference is null in PlayerScale");
           return;
       }

       originalScale = cameraRigTransform.localScale.x;

       // Ensure that scale is uniform across all axes
       transform.localScale = Vector3.one * originalScale;

       if (comfortVignette != null)
       {
           comfortVignette.VignetteFieldOfView = vignetteOpenFieldOfView;
           comfortVignette.enabled = false;
       }
   }

   public void ToggleScale()
   {
       // Ignores the input while an animation is still playing, avoiding scale jumps
       if (_scaleSequence != null && _scaleSequence.IsActive() && _scaleSequence.IsPlaying())
       {
           return;
       }

       _isGiantMode = !_isGiantMode;

       if (_isGiantMode)
       {
           AnimateScale(originalScale * giantScaleFactor, scaleUpDuration);
           onScaleToGiant.Invoke(scaleToGiantSoundEffect);
           return;
       }

       AnimateScale(originalScale, scaleDownDuration);
       onScaleToNormal.Invoke(scaleToNormalSoundEffect);
   }

   /// <summary>
   /// Plays a Mario like grow/shrink animation on the camera rig.
   /// Comfort rules applied here:
   /// - the scale is interpolated in logarithmic space, so the world grows at a constant *relative* rate.
   ///   A linear interpolation from 1x to 4x sweeps the world across the eyes very fast at the beginning
   ///   (when everything is still close) and slowly at the end, and that initial burst is what makes the player dizzy;
   /// - the scale is anchored on the player's head, so only the eye height changes (no sideways swoop);
   /// - the easing is smooth and has no overshoot/bounce, so there are no direction changes at the end;
   /// - the field of view is narrowed by a vignette while the player is moving.
   /// </summary>
   private void AnimateScale(float targetScale, float duration)
   {
       _scaleSequence?.Kill();

       float currentScale = cameraRigTransform.localScale.x;

       // Ground point under the head, kept fixed during the whole animation
       Vector3 pivotWorldPosition = GetHeadPivotWorldPosition();
       Vector3 pivotLocalPosition = cameraRigTransform.InverseTransformPoint(pivotWorldPosition);

       _animatedLogScale = Mathf.Log(currentScale);

       _scaleSequence = DOTween.Sequence();

       // Anticipation: small squash before growing, like Mario crouching before he gets big.
       // It is skipped when shrinking, where it would only add an unexpected extra drop.
       bool isGrowing = targetScale > currentScale;

       if (isGrowing && anticipationAmount > 0f)
       {
           float squashScale = Mathf.Max(0.01f, currentScale - (originalScale * anticipationAmount));

           _scaleSequence.Append(CreateScaleTween(squashScale, anticipationDuration, Ease.OutSine));
       }

       _scaleSequence.Append(CreateScaleTween(targetScale, duration, Ease.InOutSine));

       _scaleSequence.OnUpdate(() => KeepPivotAnchored(pivotWorldPosition, pivotLocalPosition));

       _scaleSequence.OnComplete(() =>
       {
           OpenVignette();
           onScaleAnimationCompleted.Invoke();
       });

       _scaleSequence.SetLink(gameObject).SetUpdate(UpdateType.Normal, true);

       CloseVignette();
       onScaleAnimationStarted.Invoke();
   }

   /// <summary>
   /// Builds a tween that drives the rig scale through <see cref="_animatedLogScale"/>, keeping the growth
   /// perceptually linear instead of numerically linear.
   /// </summary>
   private Tween CreateScaleTween(float targetScale, float duration, Ease ease)
   {
       return DOTween.To(() => _animatedLogScale,
                         logScale =>
                         {
                             _animatedLogScale = logScale;
                             cameraRigTransform.localScale = Vector3.one * Mathf.Exp(logScale);
                         },
                         Mathf.Log(targetScale),
                         duration)
                     .SetEase(ease);
   }

   /// <summary>
   /// Returns the world position the scale animation grows around: the floor point under the player's head.
   /// Falls back to the rig position when there is no head reference.
   /// </summary>
   private Vector3 GetHeadPivotWorldPosition()
   {
       if (headTransform == null)
       {
           return cameraRigTransform.position;
       }

       return new Vector3(headTransform.position.x, cameraRigTransform.position.y, headTransform.position.z);
   }

   /// <summary>
   /// Compensates the horizontal offset introduced by the scale, so the player only feels the height change.
   /// </summary>
   private void KeepPivotAnchored(Vector3 pivotWorldPosition, Vector3 pivotLocalPosition)
   {
       Vector3 offset = pivotWorldPosition - cameraRigTransform.TransformPoint(pivotLocalPosition);

       cameraRigTransform.position += new Vector3(offset.x, 0f, offset.z);
   }

   private void CloseVignette()
   {
       if (comfortVignette == null)
       {
           return;
       }

       _vignetteTween?.Kill();

       comfortVignette.enabled = true;

       _vignetteTween = DOTween.To(() => comfortVignette.VignetteFieldOfView,
                                   fieldOfView => comfortVignette.VignetteFieldOfView = fieldOfView,
                                   vignetteTunnelFieldOfView,
                                   vignetteCloseDuration)
                               .SetEase(Ease.OutSine)
                               .SetLink(gameObject);
   }

   private void OpenVignette()
   {
       if (comfortVignette == null)
       {
           return;
       }

       _vignetteTween?.Kill();

       // Opens back slower than it closed, so the player does not notice the vignette leaving
       _vignetteTween = DOTween.To(() => comfortVignette.VignetteFieldOfView,
                                   fieldOfView => comfortVignette.VignetteFieldOfView = fieldOfView,
                                   vignetteOpenFieldOfView,
                                   vignetteCloseDuration * 2f)
                               .SetEase(Ease.InOutSine)
                               .SetLink(gameObject)
                               .OnComplete(() => comfortVignette.enabled = false);
   }

   private void OnDestroy()
   {
       _scaleSequence?.Kill();
       _vignetteTween?.Kill();
   }
}
