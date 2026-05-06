using System.Collections.Generic;

/// <summary>
/// JSON structure for the post-session congratulations dialogue, loaded from
/// <c>frisbee_session_completed.json</c> or <c>archery_session_completed.json</c>.
/// </summary>
public class SessionCompletedData : JSONData
{
    /// <summary>Pool of congratulatory lines; one is selected at random to display each session.</summary>
    public List<string> Congrats { get; set; }
}