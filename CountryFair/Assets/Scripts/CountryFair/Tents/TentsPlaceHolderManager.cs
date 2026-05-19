using UnityEngine;
using System.Collections.Generic;

public class TentsPlaceHolderManager : MonoBehaviour
{
    [SerializeField]
    private Transform tentsPlaceHolderTransform;

    [SerializeField]
    private Transform miniGameTentsTransform;

    private ShowTentData[] tents;

    private TentPlaceHolder[] tentsPlaceHolders;

    private Dictionary<ShowTentData, Transform> tentToPlaceHolderMap = new Dictionary<ShowTentData, Transform>();
    
    private void Awake(){

        if (tentsPlaceHolderTransform == null){
            Debug.LogError("Tents placeholder transform is not assigned in the inspector.");
            return;
        }

        if (miniGameTentsTransform == null){
            Debug.LogError("Mini Game Tents parent transform is not assigned in the inspector.");
            return;
        }

        tentsPlaceHolders = tentsPlaceHolderTransform.GetComponentsInChildren<TentPlaceHolder>();

        if (tentsPlaceHolders == null){
            Debug.LogError("No TentPlaceHolder components found in children of tentsPlaceHolderTransform.");
            return;
        }

        TogglePlaceHolders(false);

        tents = miniGameTentsTransform.GetComponentsInChildren<ShowTentData>();

        if (tents == null){
            Debug.LogError("No ShowTentData components found in children of miniGameTents.");
        }
    }

    public void SetPlaceHolderForTent(){

        int miniGamesCount = tents.Length;

        Transform[] placeHolders = ShufflePlaceHolders();

        if (miniGamesCount != placeHolders.Length){
            Debug.LogError($"The number of mini-game tents ({miniGamesCount}) does not match the number of placeholders ({placeHolders.Length}). Please ensure they are equal.");
            return;
        }

        for (int i = 0; i < miniGamesCount; i++)
        {
            ShowTentData tent = tents[i];

            tentToPlaceHolderMap[tent] = placeHolders[i];

            tent.transform.SetPositionAndRotation(placeHolders[i].position, placeHolders[i].rotation);

            Debug.Log($"Assigned tent '{tent.gameObject.name}' to placeholder '{placeHolders[i].name}' at position {placeHolders[i].position}."); 
        }

    }

    private Transform[] ShufflePlaceHolders()
    {
        for (int i = tentsPlaceHolders.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (tentsPlaceHolders[i], tentsPlaceHolders[j]) = (tentsPlaceHolders[j], tentsPlaceHolders[i]);
        }

        Transform[] transforms = new Transform[tentsPlaceHolders.Length];

        for (int i = 0; i < tentsPlaceHolders.Length; i++){
            transforms[i] = tentsPlaceHolders[i].transform;
        }
            
        return transforms;
    }


    private void TogglePlaceHolders(bool isActive){
        foreach (TentPlaceHolder placeHolder in  tentsPlaceHolders)
        {
            placeHolder.gameObject.SetActive(isActive);
        }
    }
}