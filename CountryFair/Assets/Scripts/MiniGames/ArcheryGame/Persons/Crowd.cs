using UnityEngine;
using UnityEngine.Events;
public class Crowd: MonoBehaviour
{
    private IdlePerson[] people;

    public UnityEvent<AudioManager.GameSoundEffects, GameObject> cheer;

    private void Awake()
    {
        people = GetComponentsInChildren<IdlePerson>();
    }


    public void Cheer()
    {   
        foreach (IdlePerson person in people)
        {
            person.Jump();
        }
    }
}