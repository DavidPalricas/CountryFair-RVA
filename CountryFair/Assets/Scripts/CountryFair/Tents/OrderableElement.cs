using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class OrderableTentElement : MonoBehaviour
{
     /// <summary>
    /// Raised when the tent grab state changes.
    /// Passes <c>true</c> when grabbed, <c>false</c> when released, along with this tent instance.
    /// </summary>
    [SerializeField]
    protected UnityEvent<bool, OrderableTentElement> OnElementSelectionChanged;

    /// <summary>The placeholder slot this tent currently occupies; determines snap position and badge number.</summary>
    [SerializeField]
    protected PlaceHolder currentPlaceHolder;

    protected Collider _collider  = null;


    protected PlaceHolder _previousPlaceHolder;

    public MINI_GAMES miniGame = MINI_GAMES.ARCHERY;

    public enum MINI_GAMES
    {
        ARCHERY,
        DUCKGAME,
        FISHING,
        FRISBEE,
    }


    protected virtual void Awake()
    {   
        if (currentPlaceHolder == null)
        {
            Debug.LogError("Current Tent PlaceHolder reference is null in OrderableElement");

            return;
        }

        _collider = GetComponent<Collider>();

        _previousPlaceHolder = currentPlaceHolder;
    }

    
    protected IEnumerator SnapToPlaceHolderNextFixedUpdate()
    {
        // Wait one fixed step so the Grabbable/Interactable finishes its release logic
        // (which writes velocity to the Rigidbody for throw inertia).
        yield return new WaitForFixedUpdate();

        SnapToCurrentPlaceHolder();

        OnElementSelectionChanged.Invoke(false, this);
    }

    
    /// <summary>Returns the placeholder slot this tent currently occupies.</summary>
    public PlaceHolder GetCurrentPlaceHolder()
    {
        return currentPlaceHolder;
    }

    
    /// <summary>
    /// Sets the target placeholder for this tent.
    /// If <paramref name="newPlaceHolder"/> is null, reverts to the last valid placeholder —
    /// covers the case where the tent exits all trigger zones mid-drag.
    /// Called by <see cref="TentPlaceHolder"/> trigger enter/exit callbacks.
    /// </summary>
    public void UpdateTentPlaceHolder(PlaceHolder newPlaceHolder)
    {
        if (newPlaceHolder == null){
            currentPlaceHolder = _previousPlaceHolder;

            return;
        }

        currentPlaceHolder = newPlaceHolder;
    }

        /// <summary>
    /// Responds to a grab start or release event, toggling between selected and unselected states.
    /// </summary>
    /// <param name="isGrabbed">True when the grab begins; false when the player releases the tent.</param>
    /// <remarks>Invocado via Inspector nos eventos OnSelectEntered/OnSelectExited do componente XR Grab Interactable.</remarks>
    public virtual void HandleGrab(bool isGrabbed)
    {
        Debug.LogError("HandleGrab method must be implemented in a derived class.");
    }

     /// <summary>
    /// Teleports this tent and its play button to the current placeholder's position and rotation,
    /// and refreshes the ribbon number.
    /// </summary>
    public virtual void SnapToCurrentPlaceHolder()
    {
        Transform PlaceHolderTransform = currentPlaceHolder.transform;

        transform.SetPositionAndRotation(PlaceHolderTransform.position, PlaceHolderTransform.rotation);
    }
}