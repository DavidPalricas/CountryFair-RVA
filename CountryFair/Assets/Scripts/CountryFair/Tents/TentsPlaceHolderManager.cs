using UnityEngine;
using System.Collections.Generic;

public class TentsPlaceHolderManager : MonoBehaviour
{
    [SerializeField]
    private Transform firstTentPlaceHolderTransform;

    [SerializeField]
    private Transform secondTentPlaceHolderTransform;

    [SerializeField]
    private Transform miniGameTents;


    private ShowTentData[] tents;

    private Dictionary<ShowTentData, Transform> tentToPlaceHolderMap = new Dictionary<ShowTentData, Transform>();
    
    private void Awake(){

        if (firstTentPlaceHolderTransform == null || secondTentPlaceHolderTransform == null){
            Debug.LogError("One or more tent placeholder transforms are not assigned in the inspector.");
            return;
        }

        if (miniGameTents == null){
            Debug.LogError("Mini Game Tents parent transform is not assigned in the inspector.");
            return;
        }

        tents = miniGameTents.GetComponentsInChildren<ShowTentData>();

        if (tents == null){
            Debug.LogError("No ShowTentData components found in children of miniGameTents.");
            return;
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


    private Transform[] ShufflePlaceHolders(){

        Transform[] placeHolders = new Transform[] { firstTentPlaceHolderTransform, secondTentPlaceHolderTransform };

        for (int i = placeHolders.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Transform temp = placeHolders[i];
            placeHolders[i] = placeHolders[j];
            placeHolders[j] = temp;
        }

        return placeHolders;
    }
}