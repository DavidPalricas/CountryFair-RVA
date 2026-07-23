using UnityEngine;

public class TentPersonalizationMenu : MonoBehaviour
{  
    [SerializeField]
    private GameObject menu = null;

    private bool _menuActive = false;

    private void Awake()
    {   
        if (menu == null)
        {   
            Debug.LogError("Menu reference is null in TentPersonalizationMenu");

            return;
        }

        menu.SetActive(_menuActive);
        gameObject.SetActive(false);
    }

    public void ButtonClicked()
    {
        _menuActive = !_menuActive;
         
        menu.SetActive(_menuActive);
    }

    public void IntroCompleted()
    {
        gameObject.SetActive(true);
    }

}
