using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// Gesture input system that detects ray-based interactions from the Meta Quest 3 using the Oculus Interaction SDK.
/// This singleton class provides centralized access to hand gesture detection, specifically ray interactions
/// which are cast from the user's hands/controllers to interact with objects in the game world.
/// </summary>
public class Gestures
{
    /// <summary>
    /// Enumeration of supported gesture types that can be detected.
    /// Currently supports RAY interactions from the user's hands.
    /// </summary>
    public enum Type
    {
        /// <summary>Ray-based gesture where a ray from the user's hand selects or interacts with objects.</summary>
        RAY
    }
    
    /// <summary>
    /// The singleton instance of the Gestures class.
    /// </summary>   
    private static Gestures instance = null;
 
    /// <summary>
    /// Private constructor to enforce singleton pattern.
    /// </summary>
    private Gestures() { }

    /// <summary>
    /// Gets the singleton instance of the Gestures class.
    /// If no instance exists, a new one is created automatically.
    /// </summary>
    /// <returns>The singleton instance of the Gestures class.</returns>
    public static Gestures GetInstance()
    {
        instance ??= new Gestures();
        return instance;
    }

    /// <summary>
    /// Detects whether a specific gesture type is currently active.
    /// </summary>
    /// <param name="input">The type of gesture to detect (e.g., Type.RAY).</param>
    /// <returns>True if the specified gesture is currently active, false otherwise.</returns>
    /// <remarks>
    /// The detection automatically initializes on first call if not already initialized.
    /// For RAY gestures, this returns true if any RayInteractor is currently selecting an interactable object.
    /// </remarks>
    public bool GetGesture(Type input)
    {
        switch (input)
        {
            case Type.RAY:
                return DetectRayGesture();
            default:
                Debug.LogError("Gesture type not recognized. Please use Type.RAY for ray-based gestures.");
                return false;
        }
    }
    
    /// <summary>
    /// NOTE: The DetectRayGesture() method is still in development and may not work as expected yet.
    /// Current limitations and issues are being addressed in future updates.
    /// </summary>

    /// <summary>
    /// Detects if a ray interaction is currently active on any RayInteractor in the scene.
    /// A ray interaction is considered active when a ray from the user's hand is selecting an interactable object.
    /// This is typically triggered by pointing at objects with your hand while wearing the Meta Quest 3 headset.
    /// </summary>
    /// <returns>
    /// True if any ray interaction is currently happening (i.e., a ray is selecting an interactable).
    /// False if no ray interactions are active or no RayInteractors are found.
    /// </returns>
    /// <remarks>
    /// This method queries all RayInteractor components in the scene on each call.
    /// For performance-critical code, consider caching the results or using the Initialize() method
    /// to pre-cache RayInteractor references.
    /// </remarks>
    private bool DetectRayGesture()
    {
        RayInteractor[] rayInteractors = Object.FindObjectsByType<RayInteractor>(FindObjectsSortMode.None);

        Debug.Log("Number of RayInteractors found: " + rayInteractors.Length);

        // Check if any ray interactor has an active interactable
        foreach (RayInteractor rayInteractor in rayInteractors)
        {
            if (rayInteractor == null)
            {   
                continue;
            }

            // Check if there's an active interactable being selected by the ray
            // The Interactable property will be non-null if currently interacting with an object
            if (rayInteractor.Interactable != null)
            {
                Debug.Log($"Ray interaction detected on interactor: {rayInteractor.name}");
                return true;
            }
        }

        return false;
    }
}
