using System.Collections.Generic;

/// <summary>
/// Persisted data for a single mini-game: session records keyed by ISO timestamp and
/// a flexible dictionary of adaptive parameters (e.g., difficulty level, dog distance).
/// </summary>
[System.Serializable]
public class MiniGameData
{
    /// <summary>Session records keyed by ISO timestamp strings (e.g., "2024-01-01_10-00-00").</summary>
    public Dictionary<string, SessionData> SessionsData { get; set; } = new();

    /// <summary>Adaptive algorithm parameters stored as name-value string pairs for JSON flexibility.</summary>
    public Dictionary<string, string> AdadaptiveParameters { get; set; } = new();

    // Construtor para inicializar os dicionários e evitar erros de null
    public MiniGameData()
    {
        SessionsData = new Dictionary<string, SessionData>();
        AdadaptiveParameters = new Dictionary<string, string>();
    }
}