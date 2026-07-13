using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

/// <summary>
/// Orchestrates placeholder visibility and element positioning during drag-and-drop rearrangement.
/// Maintains a map of each <see cref="OrderableElement"/> to the <see cref="PlaceHolder"/> it currently occupies,
/// and swaps entries when the player drops an element into a slot already taken by another element.
/// </summary>
public class PlaceHolderManager : MonoBehaviour
{
    [SerializeField]
    private UnityEvent<OrderableTentElement[]> updateOtherManagers;

    [SerializeField]
    private Transform elementsTransform = null;


    /// <summary>Tracks which placeholder each element currently occupies.</summary>
    private readonly Dictionary<OrderableTentElement, PlaceHolder> _elementsMap = new ();
     
     private void Awake()
     {
        if (elementsTransform == null)
        {
            Debug.LogError("Elements Transform reference is null in PlaceHolderManager");
        }
     }


    /// <summary>
    /// Discovers all scene tents by tag, seeds <see cref="_elementsMap"/> with their starting slots,
    /// and hides all placeholders until a grab begins.
    /// </summary>
    private void Start()
    {
        OrderableTentElement[] elements = GetOrderableElements();

        if (elements.Length == 0){
            Debug.LogError("No OrderableTentElement components found in children of miniGameTents.");

            return;
        }

        foreach (OrderableTentElement element in elements)
        {
            _elementsMap.Add(element, element.GetCurrentPlaceHolder());
        }

        updateOtherManagers.Invoke(_elementsMap.Keys.OrderBy(tent => _elementsMap[tent].number).ToArray());

        TogglePlaceHolders(false);
    }


    private OrderableTentElement[] GetOrderableElements()
    {
        return elementsTransform.GetComponentsInChildren<OrderableTentElement>();
    }

    /// <summary>Shows or hides all registered placeholder GameObjects.</summary>
    private void TogglePlaceHolders(bool isActive)
    {
        PlaceHolder[] placeHolders = _elementsMap.Values.ToArray();

        foreach (PlaceHolder placeHolder in placeHolders)
        {
            placeHolder.gameObject.SetActive(isActive);
        }
    }

    /// <summary>
    /// Reacts to a tent being grabbed: hides every other tent and reveals all placeholders
    /// except the one already occupied by <paramref name="selectedElement"/>.
    /// </summary>
    /// <param name="selectedElement">The tent the player just picked up.</param>
    public void ElementSelected(OrderableTentElement selectedElement)
    {
        OrderableTentElement[] elements = _elementsMap.Keys.ToArray();

        int tentIndex = System.Array.IndexOf(elements, selectedElement);

        if (tentIndex == -1){
            Debug.LogError($"Selected tent '{selectedElement.gameObject.name}' is not registered.");
            return;
        }

        for (int i = 0; i < elements.Length; i++)
        {
            if (i != tentIndex){
                elements[i].gameObject.SetActive(false);
            }
        }

        TogglePlaceHolders(true);

        _elementsMap[selectedElement].gameObject.SetActive(false);
    }

    /// <summary>
    /// Reacts to a tent being released: updates positions (swapping tents if needed),
    /// re-enables all tents, and hides all placeholders.
    /// </summary>
    /// <param name="unselectedElement">The tent the player just released.</param>
    private void ElementUnselected(OrderableTentElement unselectedElement)
    {
        OrderableTentElement[] elements = _elementsMap.Keys.ToArray();

        int elementIndex = System.Array.IndexOf(elements, unselectedElement);

        if (elementIndex == -1){
            Debug.LogError($"Selected tent '{unselectedElement.gameObject.name}' is not registered.");
            return;
        }

        UpdateElementPosition(unselectedElement);


        for (int i = 0; i < elements.Length; i++)
        {
          // Since the distance grab gameobject is the father of the tent gameobject,just need to it activate only
          // tents[i].distanceGrabRb.gameObject.SetActive(true);

            elements[i].gameObject.SetActive(true);
        }

        TogglePlaceHolders(false);
    }

    /// <summary>
    /// If <paramref name="unselectedElement"/> was dropped onto a placeholder occupied by another tent,
    /// swaps their entries in <see cref="_elementsMap"/> and teleports the displaced tent to the vacated slot.
    /// </summary>
    private void UpdateElementPosition(OrderableTentElement unselectedElement)
    {
       PlaceHolder previousUnselectedTentPlaceHolder = _elementsMap[unselectedElement];

       PlaceHolder currentUnselectedTentPlaceHolder = unselectedElement.GetCurrentPlaceHolder();

       foreach (OrderableTentElement tent in _elementsMap.Keys)
       {
            if (_elementsMap[tent] == currentUnselectedTentPlaceHolder)
            {
                _elementsMap[tent] = previousUnselectedTentPlaceHolder;
                _elementsMap[unselectedElement] = currentUnselectedTentPlaceHolder;

                tent.UpdateTentPlaceHolder(previousUnselectedTentPlaceHolder);

                tent.SnapToCurrentPlaceHolder();

                updateOtherManagers.Invoke(_elementsMap.Keys.OrderBy(tent => _elementsMap[tent].number).ToArray());

                return;
            }
       }
    }

    /// <summary>
    /// Entry point called by <see cref="OrderableElement.OnElementSelectionChanged"/>.
    /// Routes to <see cref="ElementSelected"/> or <see cref="ElementUnselected"/> based on <paramref name="isTentSelected"/>.
    /// </summary>
    /// <param name="isTentSelected">True when the tent was grabbed; false when released.</param>
    /// <param name="element">The tent that changed state.</param>
    /// <remarks>Invocado via Inspector no UnityEvent <c>OnTentSelectionChanged</c> de cada <see cref="OrderableElement"/>.</remarks>
    public void HandleTentSelection(bool isTentSelected, OrderableTentElement element)
    {
        if (isTentSelected)
        {
            ElementSelected(element);

            return;
        }

        ElementUnselected(element);
    }


    public void OnOtherManagerUpdate(OrderableTentElement[] otherElements)
    {   
        foreach (OrderableTentElement other in otherElements)
        {
            if (_elementsMap.ContainsKey(other))
            {
                UpdateElementPosition(other);
            }
        }
    }
}
