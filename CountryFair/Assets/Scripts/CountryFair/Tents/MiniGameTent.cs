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
    [Header("Text elements")]
    [SerializeField] 
    private TextMeshProUGUI tentText;

    [SerializeField] 
    private TextMeshProUGUI tentNumber;

    [Header("PlaceHolders")]
    [SerializeField] 
    private TentPlaceHolder currentTentPlaceHolder;
    [SerializeField] 
    private Transform miniGamePropPlaceHolderTransform;
    
    [Header("Tent Objects")]
    [SerializeField] 
    private GameObject miniGamePropPrefab;
    [SerializeField] 
    private GameObject buttonToPlayMiniGame;

    [SerializeField] 
    private GameObject ribbon;
  
    [Header("Info to Display")]
    [SerializeField] 
    private string miniGameName = string.Empty;
    [SerializeField] 
    private string textToShow = string.Empty;

    [SerializeField] 
    private UnityEvent<bool, MiniGameTent> OnTentSelectionChanged;

    private GameObject _miniGameProp;
    private bool _isSelected = false;
   
    private TentPlaceHolder _previousPlaceHolder;

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

        _previousPlaceHolder = currentTentPlaceHolder;

        SetTentNumber(currentTentPlaceHolder.tentNumber);

        SnapToCurrentPlaceHolder();
    }

    private void Start()
    {
        AddMiniGameObject();
    }

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
            bool isToShowData = hitInfo.collider == GetComponent<Collider>();
            buttonToPlayMiniGame.SetActive(isToShowData);
        }
    }

    private void AddMiniGameObject()
    {
        _miniGameProp = Instantiate(
            miniGamePropPrefab,
            miniGamePropPlaceHolderTransform.position + miniGamePropPlaceHolderTransform.up * 0.1f,
            miniGamePropPrefab.transform.rotation
        );

        _miniGameProp.transform.parent = transform;
    }

    public void GoToMiniGame()
    {
        if (SceneManager.GetSceneByName(miniGameName) == null)
        {
            Debug.LogWarning("Scene " + miniGameName + " not found. Make sure the scene is added to the build settings.");
            return;
        }

        SceneManager.LoadScene(miniGameName);
    }

    private void SetTentNumber(int number)
    {
        tentNumber.text = number.ToString();
    }

    private void TentSelected()
    {
        buttonToPlayMiniGame.SetActive(false);

        tentText.gameObject.SetActive(false);
        tentNumber.gameObject.SetActive(false);

        ToggleRibbonStuff(false);

        OnTentSelectionChanged.Invoke(true, this);
    }

    private void TentUnselected()
    {   
        Debug.Log("Tent Unselected");
        buttonToPlayMiniGame.SetActive(true);

        tentText.gameObject.SetActive(true);
        tentNumber.gameObject.SetActive(true);

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

   public void SnapToCurrentPlaceHolder()
    {    
        Transform PlaceHolderTransform = currentTentPlaceHolder.transform;

        transform.SetPositionAndRotation(PlaceHolderTransform.position, PlaceHolderTransform.rotation);
        
        Transform buttonToPlayMiniGamePlaceHolderTransform = currentTentPlaceHolder.miniGameButtonPlaceHolderTransform;

        buttonToPlayMiniGame.transform.SetPositionAndRotation(buttonToPlayMiniGamePlaceHolderTransform.position, buttonToPlayMiniGamePlaceHolderTransform.rotation);
        
        SetTentNumber(currentTentPlaceHolder.tentNumber);
        _previousPlaceHolder = currentTentPlaceHolder;
    }

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

    public void UpdateTentPlaceHolder(TentPlaceHolder newPlaceHolder)
    {   
        if (newPlaceHolder == null){
            currentTentPlaceHolder = _previousPlaceHolder;

            return;
        }

        currentTentPlaceHolder = newPlaceHolder;
    }

    public TentPlaceHolder GetCurrentPlaceHolder()
    {
        return currentTentPlaceHolder;
    }
}