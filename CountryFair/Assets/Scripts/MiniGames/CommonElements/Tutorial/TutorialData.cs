using System.Collections.Generic;

/// <summary>
/// JSON structure for mini-game tutorial content, loaded from
/// <c>frisbee_tutorial.json</c> or <c>archery_tutorial.json</c>.
/// </summary>
public class TutorialData : JSONData
{
    /// <summary>Ordered list of game-rule lines displayed before practice begins. Consumed sequentially and removed.</summary>
    public List<string> Rules{ get; set;}
    /// <summary>Instruction text shown on screen during the practice phase.</summary>
    public string Guide{ get; set;}
    /// <summary>Closing message displayed after the player completes all practice tasks.</summary>
    public string End{ get; set; }
}