using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TentsPlaceHolderManager : MonoBehaviour
{
    private Dictionary<MiniGameTent, TentPlaceHolder>  _tentsMap = new Dictionary<MiniGameTent, TentPlaceHolder>();

     private void Start(){
        MiniGameTent[] tents = GameObject.FindGameObjectsWithTag("Tent")
            .Select(tentObj => tentObj.GetComponent<MiniGameTent>())
            .Where(tent => tent != null)
            .ToArray();
         
        Debug.Log($"Found {tents.Length} tents with 'Tent' tag in the scene.");
        if (tents.Length == 0){
            Debug.LogError("No MiniGameTent components found in children of miniGameTents.");

            return;
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
                // Since the distance grab gameobject is the father of the tent gameobject,just need to it deactivate only  
                tents[i].distanceGrabRb.gameObject.SetActive(false);
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
          // Since the distance grab gameobject is the father of the tent gameobject,just need to it activate only
          tents[i].distanceGrabRb.gameObject.SetActive(true);      
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

                tent.UpdateTentPlaceHolder(previousUnselectedTentPlaceHolder);

                tent.SnapToCurrentPlaceHolder();

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
