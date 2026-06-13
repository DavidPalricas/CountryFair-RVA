using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Orchestrates placeholder visibility and tent positioning during drag-and-drop tent rearrangement.
/// Maintains a map of each <see cref="MiniGameTent"/> to the <see cref="TentPlaceHolder"/> it currently occupies,
/// and swaps entries when the player drops a tent into a slot already taken by another tent.
/// </summary>
public class TentsPlaceHolderManager : MonoBehaviour
{
    /// <summary>Tracks which placeholder each tent currently occupies.</summary>
    private Dictionary<MiniGameTent, TentPlaceHolder> _tentsMap = new Dictionary<MiniGameTent, TentPlaceHolder>();

    /// <summary>
    /// Discovers all scene tents by tag, seeds <see cref="_tentsMap"/> with their starting slots,
    /// and hides all placeholders until a grab begins.
    /// </summary>
    private void Start()
    {
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

    /// <summary>Shows or hides all registered placeholder GameObjects.</summary>
    private void TogglePlaceHolders(bool isActive)
    {
        TentPlaceHolder[] tentsPlaceHolders = _tentsMap.Values.ToArray();

        foreach (TentPlaceHolder placeHolder in tentsPlaceHolders)
        {
            placeHolder.gameObject.SetActive(isActive);
        }
    }

    /// <summary>
    /// Reacts to a tent being grabbed: hides every other tent and reveals all placeholders
    /// except the one already occupied by <paramref name="selectedTent"/>.
    /// </summary>
    /// <param name="selectedTent">The tent the player just picked up.</param>
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
                // tents[i].distanceGrabRb.gameObject.SetActive(false);
                tents[i].gameObject.SetActive(false);
            }
        }

        TogglePlaceHolders(true);

        _tentsMap[selectedTent].gameObject.SetActive(false);
    }

    /// <summary>
    /// Reacts to a tent being released: updates positions (swapping tents if needed),
    /// re-enables all tents, and hides all placeholders.
    /// </summary>
    /// <param name="unselectedTent">The tent the player just released.</param>
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
          // tents[i].distanceGrabRb.gameObject.SetActive(true);

            tents[i].gameObject.SetActive(true);
        }

        TogglePlaceHolders(false);
    }

    /// <summary>
    /// If <paramref name="unselectedTent"/> was dropped onto a placeholder occupied by another tent,
    /// swaps their entries in <see cref="_tentsMap"/> and teleports the displaced tent to the vacated slot.
    /// </summary>
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

    /// <summary>
    /// Entry point called by <see cref="MiniGameTent.OnTentSelectionChanged"/>.
    /// Routes to <see cref="TentSelected"/> or <see cref="TentUnselected"/> based on <paramref name="isTentSelected"/>.
    /// </summary>
    /// <param name="isTentSelected">True when the tent was grabbed; false when released.</param>
    /// <param name="tent">The tent that changed state.</param>
    /// <remarks>Invocado via Inspector no UnityEvent <c>OnTentSelectionChanged</c> de cada <see cref="MiniGameTent"/>.</remarks>
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
