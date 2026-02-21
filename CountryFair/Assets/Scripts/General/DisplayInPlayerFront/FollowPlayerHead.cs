using UnityEngine;

[DefaultExecutionOrder(1000)]
public class FollowPlayerHead : DisplayInPlayerFront
{
    protected override void Awake()
    {
        base.Awake();
        transform.SetPositionAndRotation(CalculateBaseTarget(), Quaternion.identity);
    }

    private void LateUpdate()
    {
        HandlePosition();
    }

    private void HandlePosition()
    {
        // Posição instantânea — sem lag. O ATW do Quest trata da suavidade visual.
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