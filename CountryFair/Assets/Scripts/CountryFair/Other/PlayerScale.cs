using UnityEngine;
using UnityEngine.Events;

public class PlayerScale : MonoBehaviour
{
   
   [SerializeField]
   private Transform cameraRigTransform;

   [SerializeField]
   private float giantScaleFactor = 4f;


   [Header("Events")]
   [SerializeField]
   private UnityEvent<AudioManager.GameSoundEffects> onScaleToGiant;

    [SerializeField]
    private UnityEvent<AudioManager.GameSoundEffects> onScaleToNormal;

   private bool _isGiantMode = false;

   private float originalScale = 1f;


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
   }

   public void ToggleScale()
   {
       _isGiantMode = !_isGiantMode;

       if (_isGiantMode)
       {
          
        cameraRigTransform.localScale = Vector3.one * (originalScale * giantScaleFactor);

        onScaleToGiant.Invoke(scaleToGiantSoundEffect);
        return;
       }
     
       cameraRigTransform.localScale = Vector3.one * originalScale;
       onScaleToNormal.Invoke(scaleToNormalSoundEffect);
   }
}