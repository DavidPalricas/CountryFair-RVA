using UnityEngine;

/// <summary>
/// Configures Meta Quest foveated rendering settings on device builds.
/// No-op in the Unity Editor to avoid OVRManager dependency errors.
/// </summary>
public class FoveatedRenderingController : MonoBehaviour
{
    /// <summary>Level of foveated rendering to apply. Defaults to HighTop for Quest 3 performance.</summary>
    [SerializeField]
    private OVRManager.FoveatedRenderingLevel _foveatedLevel = OVRManager.FoveatedRenderingLevel.HighTop;

    /// <summary>When true, enables dynamic foveated rendering that adjusts quality based on GPU load.</summary>
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