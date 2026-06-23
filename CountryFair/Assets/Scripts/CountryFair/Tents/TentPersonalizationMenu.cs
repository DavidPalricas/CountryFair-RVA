using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TentPersonalizationMenu : MonoBehaviour
{  
    [SerializeField]
    private GameObject menu = null;

    [SerializeField]
    private Transform tentPanels = null;

    private bool _menuActive = false;


    private TentPanel[] _tentPanels = null;

    private void Awake()
    {   
        if (menu == null)
        {   
            Debug.LogError("Menu reference is null in TentPersonalizationMenu");

            return;
        }

        if (tentPanels == null)
        {
             Debug.LogError("TentPanel reference is null in TentPersonalizationMenu");

            return;
        }

        SetTentPanels();

        menu.SetActive(_menuActive);
        gameObject.SetActive(false);
    }


    private void SetTentPanels()
    {
        _tentPanels = new TentPanel[tentPanels.childCount];

        for (int i = 0; i < _tentPanels.Length; i++)
        {
            _tentPanels[i] = tentPanels.GetChild(i).GetComponent<TentPanel>();

            if (_tentPanels[i] == null)
            {
                Debug.LogError($"'{tentPanels.GetChild(i).name}' must have a TentPanel component");
                _tentPanels = null;
                return;
            }
        }

        _tentPanels.OrderBy(panel => panel.number);
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


    public void UpdatePanels(MiniGameTent[] tents)
    {   
        int numberOfTents = tents.Length;

        if (numberOfTents != _tentPanels.Length)
        {
            Debug.LogError("The number of mini-game tents must be equal with the number of tent panels");

            return;
        }
      
       for (int i = 0; i < numberOfTents; i++)
        {
            _tentPanels[i].UpdateTentName(tents[i].miniGame);
        }
    }
}
