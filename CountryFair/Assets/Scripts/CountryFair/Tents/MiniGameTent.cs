using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
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
[RequireComponent(typeof(Collider))]
public class MiniGameTent : MonoBehaviour
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
    /// <summary>The placeholder slot this tent currently occupies; determines snap position and badge number.</summary>
    [SerializeField]
    private TentPlaceHolder currentTentPlaceHolder;

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

    /// <summary>Which mini-game scene this tent leads to when <see cref="GoToMiniGame"/> is called.</summary>

    public MINI_GAMES miniGame = MINI_GAMES.ARCHERY;

    /// <summary>
    /// Raised when the tent grab state changes.
    /// Passes <c>true</c> when grabbed, <c>false</c> when released, along with this tent instance.
    /// </summary>
    [SerializeField]
    private UnityEvent<bool, MiniGameTent> OnTentSelectionChanged;

    /// <summary>Identifies the mini-game associated with each tent.</summary>
    public enum MINI_GAMES
    {
        ARCHERY,
        DUCK,
        FISHING,
        FRISBEE,
    }

    private GameObject _miniGameProp;
    private bool _isSelected = false;

    private TentPlaceHolder _previousPlaceHolder;

    private Collider _collider  = null;

    /// <summary>
    /// Initialises UI text, validates required references, snaps the tent to its starting placeholder,
    /// and hides the play button until the player aims at the tent.
    /// </summary>
    private void Awake()
    {
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

        if (currentTentPlaceHolder == null || miniGamePropPlaceHolderTransform == null)
        {
            Debug.LogError("One or more required transforms are not assigned in MiniGameTent script.");
        }

        _collider = GetComponent<Collider>();

        _previousPlaceHolder = currentTentPlaceHolder;


        SetTentNumber(currentTentPlaceHolder.tentNumber);

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

    /// <summary>Hides UI elements and notifies <see cref="TentsPlaceHolderManager"/> that this tent was picked up.</summary>
    private void TentSelected()
    {
        buttonToPlayMiniGame.SetActive(false);

        tentText.gameObject.SetActive(false);
        tentNumber.gameObject.SetActive(false);
        dropdownItem.SetActive(false);

        ToggleRibbonStuff(false);

        OnTentSelectionChanged.Invoke(true, this);
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

        OnTentSelectionChanged.Invoke(false, this);
    }

    private void ToggleRibbonStuff(bool isActive)
    {
        ribbon.SetActive(isActive);
        tentText.gameObject.SetActive(isActive);
    }

    /// <summary>
    /// Teleports this tent and its play button to the current placeholder's position and rotation,
    /// and refreshes the ribbon number.
    /// </summary>
    public void SnapToCurrentPlaceHolder()
    {
        Transform PlaceHolderTransform = currentTentPlaceHolder.transform;

        transform.SetPositionAndRotation(PlaceHolderTransform.position, PlaceHolderTransform.rotation);

        Transform buttonToPlayMiniGamePlaceHolderTransform = currentTentPlaceHolder.miniGameButtonPlaceHolderTransform;

        buttonToPlayMiniGame.transform.SetPositionAndRotation(buttonToPlayMiniGamePlaceHolderTransform.position, buttonToPlayMiniGamePlaceHolderTransform.rotation);

        SetTentNumber(currentTentPlaceHolder.tentNumber);
        _previousPlaceHolder = currentTentPlaceHolder;
    }

    /// <summary>
    /// Responds to a grab start or release event, toggling between selected and unselected states.
    /// </summary>
    /// <param name="isGrabbed">True when the grab begins; false when the player releases the tent.</param>
    /// <remarks>Invocado via Inspector nos eventos OnSelectEntered/OnSelectExited do componente XR Grab Interactable.</remarks>
    public void HandleGrab(bool isGrabbed)
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
    /// Sets the target placeholder for this tent.
    /// If <paramref name="newPlaceHolder"/> is null, reverts to the last valid placeholder —
    /// covers the case where the tent exits all trigger zones mid-drag.
    /// Called by <see cref="TentPlaceHolder"/> trigger enter/exit callbacks.
    /// </summary>
    public void UpdateTentPlaceHolder(TentPlaceHolder newPlaceHolder)
    {
        if (newPlaceHolder == null){
            currentTentPlaceHolder = _previousPlaceHolder;

            return;
        }

        currentTentPlaceHolder = newPlaceHolder;
    }

    /// <summary>Returns the placeholder slot this tent currently occupies.</summary>
    public TentPlaceHolder GetCurrentPlaceHolder()
    {
        return currentTentPlaceHolder;
    }
}
