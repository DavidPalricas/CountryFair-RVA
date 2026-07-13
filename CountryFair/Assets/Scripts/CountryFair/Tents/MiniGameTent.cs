using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages the display and interaction of tent information in the Country Fair VR game.
///
/// This script is responsible for:
/// - Displaying tent UI panels when the user gazes at or points at a tent using the Meta Quest 3 ray interaction
/// - Showing associated mini-game preview objects for each tent
/// - Handling transitions to mini-game scenes when the user selects a tent
/// - Snapping the tent to a placeholder position when released after a Distance Grab
/// </summary>

public class MiniGameTent : OrderableTentElement
{
    [Header("Text elements")]
    /// <summary>Displays the tent's descriptive text label.</summary>
    [SerializeField]
    private TextMeshProUGUI tentText;

    /// <summary>Displays the numbered ribbon badge assigned by <see cref="TentPlaceHolder.tentNumber"/>.</summary>
    [SerializeField]
    private TextMeshProUGUI tentNumber;

    [SerializeField]
    private GameObject dropdownItem;

    [Header("PlaceHolders")]
    
    /// <summary>Anchor used when spawning <see cref="miniGamePropPrefab"/> above the tent.</summary>
    [SerializeField]
    private Transform miniGamePropPlaceHolderTransform;

    [Header("Tent Objects")]
    /// <summary>Prefab for the decorative mini-game prop spawned above this tent on Start.</summary>
    [SerializeField]
    private GameObject miniGamePropPrefab;

    /// <summary>Play button shown when the player's ray targets this tent.</summary>
    [SerializeField]
    private GameObject buttonToPlayMiniGame;

    /// <summary>Ribbon decoration that hosts the tent number badge.</summary>
    [SerializeField]
    private GameObject ribbon;

    [Header("Data")]
    /// <summary>Text displayed on the tent panel; applied to <see cref="tentText"/> on Awake.</summary>
    [SerializeField]
    private string textToShow = string.Empty;

    private GameObject _miniGameProp;

     private bool _isSelected = false;

    /// <summary>
    /// Initialises UI text, validates required references, snaps the tent to its starting placeholder,
    /// and hides the play button until the player aims at the tent.
    /// </summary>
    protected override void Awake()
    {   
        base.Awake();
        tentText.text = textToShow;

        if (buttonToPlayMiniGame == null)
        {
            Debug.LogError("Red Button is not assigned in MiniGameTent script.");
            return;
        }

        buttonToPlayMiniGame.SetActive(false);

        if (miniGamePropPrefab == null)
        {
            Debug.LogError("Mini Game Object is not assigned in MiniGameTent script.");
            return;
        }

        if (tentNumber == null)
        {
            Debug.LogError("Ribbon Number Text is not assigned in MiniGameTent script.");
            return;
        }

        if (miniGamePropPlaceHolderTransform == null)
        {
            Debug.LogError("One or more required transforms are not assigned in MiniGameTent script.");

            return;
        }

        SetTentNumber(currentPlaceHolder.number);

        SnapToCurrentPlaceHolder();
    }

    /// <summary>Spawns the mini-game decorative prop above <see cref="miniGamePropPlaceHolderTransform"/>.</summary>
    private void Start()
    {
        AddMiniGameObject();
    }

    /// <summary>
    /// Casts a Meta Quest ray each frame and shows or hides <see cref="buttonToPlayMiniGame"/>
    /// based on whether the ray hits this tent. Skipped while the tent is grabbed.
    /// </summary>
    private void LateUpdate()
    {
        if (!_isSelected)
        {
            CheckIfPlayerWantsToGoToMiniGame();
        }
    }

    private void CheckIfPlayerWantsToGoToMiniGame()
    {
        Ray ray = Utils.CastRayMetaQuest();

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            bool isToShowData = hitInfo.collider == _collider;

            buttonToPlayMiniGame.SetActive(isToShowData);
        }
    }

    /// <summary>Instantiates <see cref="miniGamePropPrefab"/> slightly above the placeholder anchor and parents it to this tent.</summary>
    private void AddMiniGameObject()
    {
        _miniGameProp = Instantiate(
            miniGamePropPrefab,
            miniGamePropPlaceHolderTransform.position + miniGamePropPlaceHolderTransform.up * 0.1f,
            miniGamePropPrefab.transform.rotation
        );

        _miniGameProp.transform.parent = transform;
    }

    /// <summary>Loads the mini-game scene assigned to this tent.</summary>
    /// <remarks>Invocado via Inspector no botão <c>buttonToPlayMiniGame</c> (OnClick).</remarks>
    public void GoToMiniGame()
    {
        switch (miniGame)
        {
            case MINI_GAMES.ARCHERY:
                SceneManager.LoadScene("ArcheryGame");
                return;

            case MINI_GAMES.FRISBEE:
                SceneManager.LoadScene("FrisbeeGame");
                return;

            default:
                return;
        }
    }

    private void SetTentNumber(int number)
    {
        tentNumber.text = number.ToString();
    }

    /// <summary>Hides UI elements and notifies <see cref="PlaceHolderManager"/> that this tent was picked up.</summary>
    private void TentSelected()
    {
        buttonToPlayMiniGame.SetActive(false);

        tentText.gameObject.SetActive(false);
        tentNumber.gameObject.SetActive(false);
        dropdownItem.SetActive(false);

        ToggleRibbonStuff(false);

        OnElementSelectionChanged.Invoke(true, this);
    }

    /// <summary>
    /// Restores UI elements and schedules a snap to the current placeholder after the physics release frame.
    /// </summary>
    private void TentUnselected()
    {
        Debug.Log("Tent Unselected");
        buttonToPlayMiniGame.SetActive(true);

        tentText.gameObject.SetActive(true);
        tentNumber.gameObject.SetActive(true);
        dropdownItem.SetActive(true);

        ToggleRibbonStuff(true);

        StartCoroutine(SnapToPlaceHolderNextFixedUpdate());
    }

    private IEnumerator SnapToPlaceHolderNextFixedUpdate()
    {
        // Wait one fixed step so the Grabbable/Interactable finishes its release logic
        // (which writes velocity to the Rigidbody for throw inertia).
        yield return new WaitForFixedUpdate();

        SnapToCurrentPlaceHolder();

        OnElementSelectionChanged.Invoke(false, this);
    }

    private void ToggleRibbonStuff(bool isActive)
    {
        ribbon.SetActive(isActive);
        tentText.gameObject.SetActive(isActive);
    }

    public override void HandleGrab(bool isGrabbed)
    {    
       _isSelected = isGrabbed;
    
        if (isGrabbed)
        {
            TentSelected();
            return;
        }

        TentUnselected();
    }

        /// <summary>
    /// Teleports this tent and its play button to the current placeholder's position and rotation,
    /// and refreshes the ribbon number.
    /// </summary>
    public override void SnapToCurrentPlaceHolder()
    {
        base.SnapToCurrentPlaceHolder();

        TentPlaceHolder currentTentPlaceHolder = currentPlaceHolder as TentPlaceHolder;

        if (currentTentPlaceHolder == null)
        {
            Debug.LogError($"Current PlaceHolder '{currentPlaceHolder.name}' is not a TentPlaceHolder.");
            return;
        }

        Transform buttonToPlayMiniGamePlaceHolderTransform = currentTentPlaceHolder.miniGameButtonPlaceHolderTransform;

        buttonToPlayMiniGame.transform.SetPositionAndRotation(buttonToPlayMiniGamePlaceHolderTransform.position, buttonToPlayMiniGamePlaceHolderTransform.rotation);

        SetTentNumber(currentPlaceHolder.number);
        _previousPlaceHolder = currentPlaceHolder;
    }
}
