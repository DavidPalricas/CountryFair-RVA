using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Abstract base class for all mini-game managers.
/// Provides the contract for spawning/removing targets, applying difficulty settings,
/// and tracking the current difficulty level via PlayerPrefs.
/// Concrete implementations: <see cref="FrisbeeGameManager"/>, <see cref="ArcheryGameManager"/>.
/// </summary>
[RequireComponent(typeof(MiniGameCheatCodes))]
public class MiniGameManager : MonoBehaviour
{
    [Header("Targets Configuration")]
    /// <summary>Base number of targets present at difficulty level 0.</summary>
    [SerializeField]
    protected int targetsCount = 3;

    /// <summary>Fractional number of extra targets added per difficulty level.</summary>
    [SerializeField]
    protected float targetsPerLevel = 1.5f;

    /// <summary>Number of successful actions the player must complete to finish the session.</summary>
    [SerializeField]
    protected int sessionScoreGoal = 3;

    /// <summary>Current difficulty level, persisted in PlayerPrefs under a game-specific key.</summary>
    public int difficultyLevel = 0;

    protected readonly List<GameObject> _spawnedTargets = new();

    protected int _currentDesiredCount; // CRÍTICO: Para saber quantos devemos ter

    protected virtual void Awake()
    {
       PlayerPrefs.SetInt("SessionGoal", sessionScoreGoal);
    }


    public virtual void ChangeDifficulty(bool isToIncreaseDiff){
        Debug.LogError("ChangeDifficulty should be overridden in derived classes.");
    }

    protected virtual void ApplyDifficultySettings(){
        Debug.LogError("ApplyDifficultySettings should be overridden in derived classes.");
    }

    protected virtual Vector3 GetRandomTargetPosition()
    {
        Debug.LogError("GetRandomTargetPosition should be overridden in derived classes.");
        return Vector3.zero;
    }

    protected virtual void SyncTargets(int desiredCount){
        Debug.LogError("SyncTargets should be overridden in derived classes.");
    }

    protected virtual void UpdateTargetsProperties()
    {
        Debug.LogError("UpdateTargetsProperties should be overridden in derived classes.");
    }

    protected virtual void AddTarget(GameObject targetPrefab = null)
    {
        Debug.LogError("AddTarget should be overridden in derived classes.");
    }

    protected virtual void RemoveTarget()
    {
        Debug.LogError("RemoveTarget should be overridden in derived classes.");
    }

    public virtual void DestroyTarget(GameObject target, GameObject targetPrefab = null)
    {
        Debug.LogError("DestroyTarget should be overridden in derived classes.");
    }

    /// <summary>
    /// Called by the Tutorial system when the tutorial is fully complete.
    /// Implementations should read the persisted difficulty level and apply initial settings.
    /// </summary>
    /// <remarks>Invocado via Inspector pelo evento tutorialCompleted do <see cref="Tutorial"/>.</remarks>
    public virtual void TutorialCompleted(){
        Debug.LogError("TutorialCompleted should be overridden in derived classes.");
    }

    
    public virtual void ResetDifficulty()
    {   
        difficultyLevel = 0;
        ApplyDifficultySettings();
    }
}
