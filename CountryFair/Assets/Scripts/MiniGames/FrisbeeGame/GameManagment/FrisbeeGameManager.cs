using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the Frisbee mini-game, including adaptive parameters and game state tracking.
/// </summary>
/// <remarks>
/// This manager handles dynamic difficulty adjustment through adaptive parameters
/// and maintains shared state information (like target positions) that dog states can access.
/// The adaptive parameters are recalculated at game start based on the initial positions of game entities.
/// </remarks>
public class FrisbeeGameManager : MonoBehaviour
{
    /// <summary>
    /// Dictionary of adaptive parameters used to dynamically adjust game difficulty and behavior.
    /// Currently includes "DogDistance" which determines how far the dog positions itself from the player.
    /// </summary>
    /// <remarks>
    /// Parameters are set during initialization in <see cref="SetAdaptiveParameters"/>.
    /// Dog states can read these values to adapt their behavior based on game conditions.
    /// </remarks>
    /// <value>A dictionary mapping parameter names (string) to their float values.</value>
    public Dictionary<string, float> AdaptiveParameters {get; private set;} = new Dictionary<string, float>();

    /// <summary>
    /// The current target position where the dog is positioned or moving toward.
    /// Used by states like <see cref="GoToPreviousTarget"/> to return the dog to a previous location.
    /// </summary>
    /// <remarks>
    /// Hidden in the Inspector but accessible to other scripts.
    /// Updated by dog states when new positions are established.
    /// </remarks>
   [HideInInspector]
    public Vector3 currentTargetPos = Vector3.zero;

    /// <summary>
    /// Initializes the game manager at the start of the game.
    /// </summary>
    /// <remarks>
    /// Unity lifecycle callback invoked before the first frame update.
    /// Calls <see cref="SetAdaptiveParameters"/> to configure initial adaptive parameters
    /// based on the current game state.
    /// </remarks>
    private void Start()
    {
        SetAdaptiveParameters();
    }
    
    /// <summary>
    /// Configures all adaptive parameters used throughout the game.
    /// </summary>
    /// <remarks>
    /// Currently sets the "DogDistance" parameter by calculating the initial distance between
    /// the player and dog. This value is used by dog states to determine positioning relative to the player.
    /// Additional adaptive parameters can be added here as the game evolves.
    /// </remarks>
    private void SetAdaptiveParameters()
    {
        AdaptiveParameters["DogDistance"] = GetPlayerDistanceToDog();
    }

    /// <summary>
    /// Calculates the distance between the player and the dog at game start.
    /// </summary>
    /// <remarks>
    /// Finds GameObjects tagged with "Player" and "Dog" in the scene and calculates
    /// the 3D distance between their positions.
    /// This distance is used as a baseline for adaptive difficulty, determining how far
    /// the dog should position itself during gameplay.
    /// </remarks>
    /// <returns>
    /// The distance in world units between the player and dog, or 0f if either GameObject cannot be found.
    /// </returns>
    private float GetPlayerDistanceToDog()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject dog = GameObject.FindGameObjectWithTag("Dog");

        if (player == null || dog == null)
        {
            Debug.LogError("Player or Dog GameObject not found in the scene.");
            return 0f;  
        }

        return Vector3.Distance(player.transform.position, dog.transform.position);
    } 
}
