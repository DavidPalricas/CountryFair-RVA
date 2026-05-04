using UnityEngine;

[DefaultExecutionOrder(1000)]
public class FollowPlayerHead : MonoBehaviour
{   
    [SerializeField] 
    private Transform centerEyeTransform;
    
    [Header("Positioning Settings")]
    [SerializeField]
     private float distanceFromPlayer = 2.0f; // Ajustado para valor realista
    
    [SerializeField] 
    private float heightOffset = -0.5f;      // Ajustado para valor realista
    
    [SerializeField] 
    private float horizontalOffset = 0f;
    
    private void Awake()
    {
           if (centerEyeTransform == null)
        {
            Debug.LogError("Center Eye Transform is not assigned in the inspector.");
            return;
        }

        transform.SetPositionAndRotation(CalculateBaseTarget(), Quaternion.identity);
    }

    private void LateUpdate()
    {
        HandlePosition();
    }

    private void HandlePosition()
    {
        transform.position = CalculateBaseTarget();
    }

    private Vector3 CalculateBaseTarget()
    {
        Vector3 target = centerEyeTransform.position;
        target += centerEyeTransform.right * horizontalOffset;
        target += centerEyeTransform.forward * distanceFromPlayer;
        target += centerEyeTransform.up * heightOffset;
        return target;
    }
}