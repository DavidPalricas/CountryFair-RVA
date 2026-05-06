using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Bridges XR interactable button presses to the audio system via a UnityEvent.
/// Attach to any pressable prop and wire <see cref="buttonPressed"/> to an AudioManager in the Inspector.
/// </summary>
public class ButtonPressed : MonoBehaviour
{
    /// <summary>Wired in Inspector to the scene AudioManager's PlaySpatialSoundEffect method.</summary>
    public UnityEvent<AudioManager.GameSoundEffects, GameObject> buttonPressed;
    private AudioManager.GameSoundEffects _buttonPressSoundEffect = AudioManager.GameSoundEffects.BUTTON_PRESSED;

    /// <summary>
    /// Invokes the button-pressed sound effect event, using this GameObject as the spatial audio source.
    /// </summary>
    /// <remarks>Invocado via Inspector pelo XR Interactable (OnSelectEntered) no botão.</remarks>
    public void Pressed()
    {
        buttonPressed.Invoke(_buttonPressSoundEffect, gameObject);
    }
}