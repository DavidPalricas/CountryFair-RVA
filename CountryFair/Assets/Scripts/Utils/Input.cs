using UnityEngine;

public class Input
{
    public enum InputType
    {
        SIMPLE_TOUCH
    }
    
    private static Input instance = null;

    private Input() { }

    public static Input GetInstance()
    {
        instance ??= new Input();
        return instance;
    }

    public bool GetInput(InputType input)
    {
        switch (input)
        {
            case InputType.SIMPLE_TOUCH:
                return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);
            default:
                Debug.LogError("Input type not recognized.");
                return false;
        }
    }
}
