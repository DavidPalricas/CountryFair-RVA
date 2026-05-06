
using System.Collections.Generic;

/// <summary>
/// JSON structure for Carny Wise difficulty-change feedback lines, loaded from <c>change_difficulty.json</c>.
/// </summary>
public class DiffcultyFeedBackData : JSONData
{
    /// <summary>Lines displayed when the difficulty increases.</summary>
    public List<string> IncreaseDiff{ get; set;}
    /// <summary>Lines displayed when the difficulty decreases.</summary>
    public List<string> DecreaseDiff{ get; set;}
}