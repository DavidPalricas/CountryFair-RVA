using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TentsPlaceHolderManager : MonoBehaviour
{
    [SerializeField]
    private Transform miniGameTentsTransform;

    private Dictionary<MiniGameTent, TentPlaceHolder>  _tentsMap = new Dictionary<MiniGameTent, TentPlaceHolder>();

    private void Awake(){
        if (miniGameTentsTransform == null){
            Debug.LogError("Mini Game Tents parent transform is not assigned in the inspector.");
            return;
        }
    }

     private void Start(){
        MiniGameTent[] tents = miniGameTentsTransform.GetComponentsInChildren<MiniGameTent>();

        if (tents.Length == 0){
            Debug.LogError("No MiniGameTent components found in children of miniGameTents.");
        }

        foreach (MiniGameTent tent in tents)
        {
            _tentsMap.Add(tent, tent.GetCurrentPlaceHolder());
        }

        TogglePlaceHolders(false);
    }

    private void TogglePlaceHolders(bool isActive){
        TentPlaceHolder[] tentsPlaceHolders = _tentsMap.Values.ToArray();

        foreach (TentPlaceHolder placeHolder in tentsPlaceHolders)
        {
            placeHolder.gameObject.SetActive(isActive);
        }
    }

    public void TentSelected(MiniGameTent selectedTent)
    {   
        MiniGameTent[] tents = _tentsMap.Keys.ToArray();

        int tentIndex = System.Array.IndexOf(tents, selectedTent);

        if (tentIndex == -1){
            Debug.LogError($"Selected tent '{selectedTent.gameObject.name}' is not registered.");
            return;
        }

        for (int i = 0; i < tents.Length; i++)
        {
            if (i != tentIndex){
                tents[i].gameObject.SetActive(false);
            } 
        }

        TogglePlaceHolders(true);

        _tentsMap[selectedTent].gameObject.SetActive(false);
    }

    private void TentUnselected(MiniGameTent unselectedTent)
    {   
        MiniGameTent[] tents = _tentsMap.Keys.ToArray();

        int tentIndex = System.Array.IndexOf(tents, unselectedTent);

        if (tentIndex == -1){
            Debug.LogError($"Selected tent '{unselectedTent.gameObject.name}' is not registered.");
            return;
        }

        UpdateTentsPosition(unselectedTent);

        for (int i = 0; i < tents.Length; i++)
        {
            tents[i].gameObject.SetActive(true);       
        }

        TogglePlaceHolders(false);
    }

    
    private void UpdateTentsPosition(MiniGameTent unselectedTent)
    {
       TentPlaceHolder previousUnselectedTentPlaceHolder = _tentsMap[unselectedTent];

       TentPlaceHolder currentUnselectedTentPlaceHolder = unselectedTent.GetCurrentPlaceHolder();

       foreach (MiniGameTent tent in _tentsMap.Keys)
       {
            if (_tentsMap[tent] == currentUnselectedTentPlaceHolder)
            {    
                _tentsMap[tent] = previousUnselectedTentPlaceHolder;
                _tentsMap[unselectedTent] = currentUnselectedTentPlaceHolder;


                Debug.Log($"Tent '{tent.gameObject.name}' updated its place holder to '{previousUnselectedTentPlaceHolder.gameObject.name}'");

                tent.UpdateTentPlaceHolder(previousUnselectedTentPlaceHolder);

                tent.UpdateTentPosition();

                return;
            }
       }   
    }

    public void HandleTentSelection(bool isTentSelected, MiniGameTent tent)
    {
        if (isTentSelected)
        {
            TentSelected(tent);

            return;
        }

        TentUnselected(tent);
    }
}
