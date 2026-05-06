/// <summary>
/// Per-session metrics stored inside <see cref="MiniGameData.SessionsData"/>.
/// All values are formatted strings (e.g., "85%", "42 segundos") for human-readable JSON output.
/// </summary>
[System.Serializable]
public class SessionData
{
    /// <summary>Number of successful actions that constituted the session goal (e.g., "3 pontos").</summary>
    public string SessionGoal { get; set; }
    /// <summary>Average precision across all tasks in the session, expressed as a percentage string.</summary>
    public string AverageTaskPrecision { get; set; }
    /// <summary>Average time taken to complete each task, in seconds.</summary>
    public string AverageTaskCompletionTime { get; set; }
}