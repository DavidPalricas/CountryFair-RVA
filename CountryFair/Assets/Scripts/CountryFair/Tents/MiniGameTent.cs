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

public class MiniGameTent : MonoBehaviour
{
    [Header("Game Tent Data")]
    [SerializeField] private TextMeshProUGUI textBox;
    [SerializeField] private GameObject ribbon;
    [SerializeField] private TextMeshProUGUI ribbonNumber;


    [Header("PlaceHolders")]
    [SerializeField] 
    private TentPlaceHolder currentPlaceHolder;
    [SerializeField] 
    private Transform placeHolderTransform;

    [SerializeField] private GameObject miniGameObjectPrefab;
    [SerializeField] private GameObject redButton;
  

    [SerializeField] private string gameName = string.Empty;
    [SerializeField] private string textToShow = string.Empty;

    [SerializeField]    
    public Rigidbody distanceGrabRb;

    [SerializeField] private UnityEvent<bool, MiniGameTent> OnTentSelectionChanged;


  

    private GameObject miniGameObject;
    private bool isSelected = false;
   
    private TentPlaceHolder _previousPlaceHolder;

    private void Awake()
    {
        textBox.text = textToShow;

        if (redButton == null)
        {
            Debug.LogError("Red Button is not assigned in MiniGameTent script.");
            return;
        }

        redButton.SetActive(false);

        if (miniGameObjectPrefab == null)
        {
            Debug.LogError("Mini Game Object is not assigned in MiniGameTent script.");
            return;
        }

        if (ribbonNumber == null)
        {
            Debug.LogError("Ribbon Number Text is not assigned in MiniGameTent script.");
            return;
        }

        if (currentPlaceHolder == null || placeHolderTransform == null)
        {
            Debug.LogError("One or more required transforms are not assigned in MiniGameTent script.");
        }

        _previousPlaceHolder = currentPlaceHolder;

        SetRibbonNumber(currentPlaceHolder.tentNumber);

        SnapToCurrentPlaceHolder();
    }

    private void Start()
    {
        AddMiniGameObject();
    }

    private void LateUpdate()
    {
        if (!isSelected)
        {
            CheckIfPlayerWantsToGoToMiniGame();
        }
    }

    private void CheckIfPlayerWantsToGoToMiniGame()
    {
        Ray ray = Utils.CastRayMetaQuest();

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            bool isToShowData = hitInfo.collider == GetComponent<Collider>();
            redButton.SetActive(isToShowData);
        }
    }

    private void AddMiniGameObject()
    {
        miniGameObject = Instantiate(
            miniGameObjectPrefab,
            placeHolderTransform.position + placeHolderTransform.up * 0.1f,
            miniGameObjectPrefab.transform.rotation
        );

        miniGameObject.transform.parent = transform;
    }

    public void GoToMiniGame()
    {
        if (SceneManager.GetSceneByName(gameName) == null)
        {
            Debug.LogError("Scene " + gameName + " not found. Make sure the scene is added to the build settings.");
            return;
        }

        SceneManager.LoadScene(gameName);
    }

    private void SetRibbonNumber(int number)
    {
        ribbonNumber.text = number.ToString();
    }

    private void TentSelected()
    {
        redButton.SetActive(false);
        ToggleRibbonStuff(false);
        OnTentSelectionChanged.Invoke(true, this);
    }

    private void TentUnselected()
    {
        redButton.SetActive(true);

        ToggleRibbonStuff(true);

        // Defer the snap to the next FixedUpdate so the ISDK has fully released
        // control of the Rigidbody before we reposition it.
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
        ribbonNumber.gameObject.SetActive(isActive);
    }

   public void SnapToCurrentPlaceHolder()
    {
        // Kill any residual throw velocity the ISDK applied on release.
        distanceGrabRb.linearVelocity = Vector3.zero;
        distanceGrabRb.angularVelocity = Vector3.zero;
        distanceGrabRb.isKinematic = true;

        Transform distanceGrabPlaceHolderTransform = currentPlaceHolder.distanceGrabPlaceHolderTransform;

        distanceGrabRb.transform.SetPositionAndRotation(distanceGrabPlaceHolderTransform.position, distanceGrabPlaceHolderTransform.rotation);

        Transform PlaceHolderTransform = currentPlaceHolder.transform;

        transform.SetPositionAndRotation(PlaceHolderTransform.position, PlaceHolderTransform.rotation);

        Transform redButtonPlaceHolderTransform = currentPlaceHolder.redButtonPlaceHolderTransform;

        redButton.transform.SetPositionAndRotation(redButtonPlaceHolderTransform.position, redButtonPlaceHolderTransform.rotation);

        SetRibbonNumber(currentPlaceHolder.tentNumber);
        _previousPlaceHolder = currentPlaceHolder;
    }

    public void HandleGrab(bool isGrabbed)
    {
        isSelected = isGrabbed;

        if (isGrabbed)
        {
            TentSelected();
            return;
        }

        TentUnselected();
    }

    public void UpdateTentPlaceHolder(TentPlaceHolder newPlaceHolder)
    {   
        if (newPlaceHolder == null){
            currentPlaceHolder = _previousPlaceHolder;

            return;
        }

        currentPlaceHolder = newPlaceHolder;
    }

    public TentPlaceHolder GetCurrentPlaceHolder()
    {
        return currentPlaceHolder;
    }
}