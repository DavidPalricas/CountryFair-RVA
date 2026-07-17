using UnityEngine;

public class PlaceHolder: MonoBehaviour
{
    /// <summary>Slot number displayed on the tent ribbon when a tent occupies this placeholder.</summary>
    public int number = 1;


        /// <summary>
    /// Notifies the entering tent that it is now hovering over this placeholder,
    /// allowing it to snap here when released.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("TentElement")){
            
            if (!other.gameObject.TryGetComponent<OrderableTentElement>(out var element))
            {
                Debug.LogError("GameObject with 'TentElement' tag must have a OrderableTentElement component.");

                return;
            }

            element.UpdateTentPlaceHolder(this);
        }
    }

    /// <summary>
    /// Notifies the exiting tent that it has left this placeholder's zone,
    /// causing it to revert to its previous valid placeholder.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("TentElement")){
            
            if (!other.gameObject.TryGetComponent<OrderableTentElement>(out var element))
            {
                Debug.LogError("GameObject with 'TentElement' tag must have a OrderableTentElement component.");

                return;
            }

            element.UpdateTentPlaceHolder(null);
        }
    }
}