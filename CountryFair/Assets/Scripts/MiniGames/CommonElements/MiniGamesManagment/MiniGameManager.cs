using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MiniGameCheatCodes))]
public class MiniGameManager : MonoBehaviour
{      
    [Header("Targets Configuration")]
    [SerializeField ]
    protected int targetsCount = 3;

    [SerializeField]
    protected float targetsPerLevel = 1.5f;  

    [SerializeField]
    protected int sessionScoreGoal = 3;
   
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

    public virtual void TutorialCompleted(){
        Debug.LogError("TutorialCompleted should be overridden in derived classes.");
    }

    
    public virtual void ResetDifficulty()
    {   
        difficultyLevel = 0;
        ApplyDifficultySettings();
    }
}
