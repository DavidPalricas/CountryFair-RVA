using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the display and interaction of tent information in the Country Fair VR game.
/// 
/// This script is responsible for:
/// - Displaying tent UI panels when the user gazes at or points at a tent using the Meta Quest 3 ray interaction
/// - Showing associated mini-game preview objects for each tent
/// - Handling transitions to mini-game scenes when the user selects a tent
/// 
/// The script uses gaze/ray-based raycasting from the user's eye position to detect when they are looking at
/// a specific tent collider. When detected, it activates the corresponding UI and mini-game preview.
/// If the user performs a ray gesture (selection) while looking at the tent, the game loads the associated mini-game scene.
/// </summary>
public class ShowTentData : MonoBehaviour
{   
    /// <summary>
    /// UI panel that displays the tent name and description.
    /// This GameObject is activated/deactivated based on whether the user is looking at this tent.
    /// Expected to contain a TextMeshProUGUI component for displaying text.
    /// </summary>
    [Header("Game Tent Data")]
    [SerializeField]
    private GameObject textBox;

    /// <summary>
    /// Prefab for the mini-game preview object to instantiate and display.
    /// This prefab represents a visual preview of the mini-game associated with this tent.
    /// If null, a warning will be logged and no preview will be shown.
    /// </summary>
    [SerializeField]
    private GameObject miniGameObjectPrefab;

    /// <summary>
    /// Transform position where the instantiated mini-game preview object should be placed.
    /// The preview object is instantiated slightly above this position (0.1 units up) to ensure visibility.
    /// </summary>
    [SerializeField]
    private Transform placeHolderTransform;

    /// <summary>
    /// The name of the mini-game scene associated with this tent.
    /// This scene name is used to load the appropriate mini-game when the user selects the tent.
    /// Must match a scene name that exists in the Build Settings.
    /// </summary>
    [SerializeField]
    // Ignore spelling warning for "gameName", because if it's changed to readonly, it will not be editable in the inspector
    private string gameName = string.Empty;
    
    /// <summary>
    /// The text content to display in the UI panel for this tent.
    /// This text is shown in the TextMeshProUGUI component within the textBox GameObject.
    /// Can contain the tent name, description, or any other relevant information.
    /// </summary>
    [SerializeField]
    private string textToShow = string.Empty;

    /// <summary>
    /// Reference to the instantiated mini-game preview object in the scene.
    /// This is created in the Awake method if miniGameObjectPrefab is provided.
    /// Null if no mini-game prefab was assigned.
    /// </summary>
    private GameObject miniGameObject;

    /// <summary>
    /// Initializes the tent UI and mini-game preview during scene load.
    /// 
    /// This method:
    /// 1. Hides the tent information UI panel
    /// 2. Sets the text content that will be displayed when the user looks at this tent
    /// 3. Instantiates the mini-game preview object if a prefab is provided
    /// 4. Logs a warning if no mini-game prefab is assigned
    /// </summary>
    /// <remarks>
    /// Called automatically by Unity during scene initialization.
    /// The order of operations ensures the UI is ready before any ray interactions occur.
    /// </remarks>
    private void Awake()
    {
        textBox.SetActive(false);

        textBox.GetComponentInChildren<TextMeshProUGUI>().text = textToShow;

        if (miniGameObjectPrefab != null)
        {
            AddMiniGameObject();
            return;
        }

        Debug.LogWarning(gameObject.name + " does not have the mini game prefab to show!");
    }

    /// <summary>
    /// Called every frame after all other updates to handle ray-based tent detection and interaction.
    /// 
    /// This method:
    /// 1. Casts a ray from the user's eye position (using Meta Quest 3 gaze)
    /// 2. Checks if the ray intersects with this tent's collider
    /// 3. Activates/deactivates the tent UI and mini-game preview based on ray intersection
    /// 4. Detects ray gesture input (selection) and loads the mini-game scene if the user is looking at this tent
    /// 
    /// Ray Interaction Flow:
    /// - User looks at tent → Ray hits collider → UI and preview activate
    /// - User performs ray gesture while looking at tent → Mini-game scene loads
    /// - User looks away → Ray misses collider → UI and preview deactivate
    /// </summary>
    /// <remarks>
    /// Uses LateUpdate instead of Update to ensure all other object movements and transformations
    /// are processed before ray calculations occur, providing accurate hit detection.
    /// </remarks>
    private void LateUpdate()
    {
        Ray ray = Utils.CastRayMetaQuest();

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            bool isToShowData = hitInfo.collider == gameObject.GetComponent<Collider>();

            textBox.SetActive(isToShowData);

            miniGameObject?.SetActive(isToShowData);

            if (Gestures.GetInstance().GetGesture(Gestures.Type.RAY) && isToShowData)
            {
                GoToMiniGame();
            }
        }
    }

    /// <summary>
    /// Instantiates the mini-game preview object and positions it in the scene.
    /// 
    /// The mini-game preview is placed at the placeholder transform position,
    /// offset slightly upward (0.1 units) to ensure it's visible above the ground plane.
    /// The preview object is initially hidden and will be shown when the user looks at the tent.
    /// </summary>
    /// <remarks>
    /// This method is called during Awake if miniGameObjectPrefab is not null.
    /// If the prefab is null, this method is not called and a warning is logged instead.
    /// </remarks>
    private void AddMiniGameObject()
    {
        miniGameObject = Instantiate(miniGameObjectPrefab, placeHolderTransform.position + placeHolderTransform.up * 0.1f, miniGameObjectPrefab.transform.rotation);

        miniGameObject.SetActive(false);
    }

    /// <summary>
    /// Loads the mini-game scene associated with this tent when the user selects it.
    /// 
    /// This method:
    /// 1. Validates that the scene name corresponds to a scene in the Build Settings
    /// 2. Loads the scene additively (if needed) or as a single scene
    /// 3. Logs an error if the scene cannot be found
    /// </summary>
    /// <remarks>
    /// The scene name must exactly match the name configured in Unity's Build Settings.
    /// If the scene name is incorrect or not added to Build Settings, loading will fail with an error message.
    /// Current implementation uses SceneManager.LoadScene which unloads the current scene.
    /// </remarks>
    private void GoToMiniGame()
    {
        if (SceneManager.GetSceneByName(gameName) == null)
        {
            Debug.LogError("Scene " + gameName + " not found. Make sure the scene is added to the build settings.");
            return;
        }

        SceneManager.LoadScene(gameName); 
    }
}
