using UnityEngine;
using System.Linq;

/// <summary>
/// Archery-specific cheat code handler. Extends <see cref="MiniGameCheatCodes"/> with miss/score
/// cheats that operate on the <see cref="Arrow"/> component — launching the arrow with zero force
/// (miss) or teleporting it onto the correct balloon (score).
/// </summary>
public class ArcheryCheatCodes : MiniGameCheatCodes
{
    [Header("Arrow Dependencies")]
    /// <summary>Transform used to reposition the arrow during the score cheat.</summary>
    [SerializeField]
    private Transform arrowTransform = null;

    /// <summary>Arrow component used to trigger launches via cheat commands.</summary>
    [SerializeField]
    private Arrow arrowComponent = null;

    /// <summary>
    /// Initializes component references and registers archery-specific cheat codes.
    /// Unity callback called when the script instance is being loaded.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        if (arrowComponent == null)
        {
            Debug.LogError("Arrow component reference is not assigned in the inspector.");
            return;
        }

        if (arrowTransform == null)
        {
            Debug.LogError("Arrow Transform reference is not assigned in the inspector.");
            return;
        }

        _tutorialCompleted = GameManager.GetInstance().ArcheryTutorialCompleted;

        _maxCheatLength = _cheatCommands.Keys.Max(c => c.Length);
    }

    /// <summary>
    /// Launches the arrow with no force, simulating a miss.
    /// </summary>
    protected override void OnMissCheat()
    {
        arrowComponent.Launch(0f);
    }

    /// <summary>
    /// Launches the arrow and teleports it to the correct balloon position, guaranteeing a score.
    /// </summary>
    protected override void OnScoreCheat()
    {
        // Random value of launch force, to ensure the arrow is not kinematic
        arrowComponent.Launch(5f);
        arrowTransform.position = GetCorrectBalloonPosition() + new Vector3(0, 0f, 0);
    }

    private Vector3 GetCorrectBalloonPosition()
    {
        string balloonColorToScore = PlayerPrefs.GetString("BalloonColorToScore", "red").ToLower();

        Transform targetBalloonTransform = GameObject.FindGameObjectsWithTag("Balloon")
            .FirstOrDefault(balloon => balloon.GetComponent<BalloonArcheryGame>().GetBalloonColorName().ToLower() == balloonColorToScore)
            .transform;

        if (targetBalloonTransform == null)
        {
            Debug.LogError("Correct balloon color not found!");
            return Vector3.zero;
        }

        return targetBalloonTransform.position;
    }
}