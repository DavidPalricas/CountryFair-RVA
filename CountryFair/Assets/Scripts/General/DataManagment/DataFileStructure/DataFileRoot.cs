
/// <summary>
/// Root JSON structure of <c>survivorData.json</c>.
/// Contains persistent session history and adaptive parameters for each mini-game.
/// </summary>
[System.Serializable]
public class DataFileRoot
{
    /// <summary>Frisbee mini-game session history and adaptive parameters.</summary>
    public MiniGameData frisbeeGame = new ();
    /// <summary>Archery mini-game session history and adaptive parameters.</summary>
    public MiniGameData archeryGame = new ();
}