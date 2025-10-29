using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Displays tent information and mini-game objects based on raycast detection from the crosshair.
/// </summary>
public class ShowTentData : MonoBehaviour
{   
    /// <summary>UI panel containing the tent name text.</summary>
    [Header("Game Tent Data")]
    [SerializeField]
    private GameObject textBox;

    /// <summary>Prefab for the mini-game object to display.</summary>
    [SerializeField]
    private GameObject miniGameObjectPrefab;

    /// <summary>Transform position where the mini-game object should be placed.</summary>
    [SerializeField]
    private Transform placeHolderTransform;

    /// <summary>Name of the game/tent to display in the UI.</summary>
    [SerializeField]
    // Ignore spelling warning for "gameName", becuase if its changed to readonly, it will not be editable in the inspector
    private string gameName = string.Empty;
    
    [SerializeField]
    private string textToShow = string.Empty;

    /// <summary>Instantiated mini-game object in the scene.</summary>
    private GameObject miniGameObject;

    /// <summary>
    /// Initializes the tent UI by finding the crosshair, setting text, and instantiating the mini-game object if provided.
    /// </summary>
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
    /// Casts a ray from the crosshair each frame and activates/deactivates the tent UI and mini-game 
    /// when the ray hits this tent's collider.
    /// </summary>
    private void LateUpdate()
    {
        Ray ray = Utils.CastRayMetaQuest();

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            bool isToShowData = hitInfo.collider == gameObject.GetComponent<Collider>();

            textBox.SetActive(isToShowData);

            miniGameObject?.SetActive(isToShowData);

            if (Input.GetInstance().GetInput(Input.InputType.SIMPLE_TOUCH) && isToShowData)
            {
                GoToMiniGame();
            }
        }
    }

    /// <summary>
    /// Instantiates the mini-game object at the placeholder position and initially hides it.
    /// </summary>
    private void AddMiniGameObject()
    {
        miniGameObject = Instantiate(miniGameObjectPrefab, placeHolderTransform.position + placeHolderTransform.up * 0.1f, miniGameObjectPrefab.transform.rotation);

        miniGameObject.SetActive(false);
    }

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
