using System.Collections.Generic;

/// <summary>
/// JSON structure for the hub-world intro dialogue, loaded from <c>intro.json</c>.
/// </summary>
public class IntroData : JSONData
{
    /// <summary>Lines spoken by Zeca Bigodes in the first part of the introduction.</summary>
    public List<string> ZecaPart1 { get; set; }
    /// <summary>Lines spoken by Carny Wise during the introduction.</summary>
    public List<string> CarnyWise { get; set; }
    /// <summary>Lines spoken by Zeca Bigodes in the second part of the introduction.</summary>
    public List<string> ZecaPart2 { get; set; }
}