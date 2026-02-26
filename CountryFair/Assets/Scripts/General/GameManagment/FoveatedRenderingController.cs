using UnityEngine;

public class FoveatedRenderingController : MonoBehaviour
{
    [SerializeField] 
    private OVRManager.FoveatedRenderingLevel _foveatedLevel = OVRManager.FoveatedRenderingLevel.HighTop;

    [SerializeField] 
    private bool _useDynamic = true;

    private void Start()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
                OVRManager.useDynamicFoveatedRendering = _useDynamic;
                OVRManager.foveatedRenderingLevel = _foveatedLevel;
        #endif
    }
}