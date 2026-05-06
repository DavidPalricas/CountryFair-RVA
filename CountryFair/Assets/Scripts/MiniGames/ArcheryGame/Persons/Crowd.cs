using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Manages the archery spectator crowd, forwarding score celebrations to all <see cref="IdlePerson"/> children.
/// </summary>
public class Crowd: MonoBehaviour
{
    private IdlePerson[] people;

    /// <summary>Fired when the crowd cheers — plays the crowd cheer sound effect at the crowd GameObject's position.</summary>
    public UnityEvent<AudioManager.GameSoundEffects, GameObject> cheer;

    private void Awake()
    {
        people = GetComponentsInChildren<IdlePerson>();
    }


    /// <summary>
    /// Triggers a jump animation on every crowd member and fires the cheer sound event.
    /// Called by <see cref="Arrow"/> when the player scores.
    /// </summary>
    public void Cheer()
    {
        foreach (IdlePerson person in people)
        {
            person.Jump();
        }
    }
}