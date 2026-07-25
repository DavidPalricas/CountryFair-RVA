using UnityEngine;
using Colyseus;

public class ConnectToWebApp : MonoBehaviour
{   
    [SerializeField]
    private int serverPort = 2567;
    private static ConnectToWebApp _instance = null;


   private Room<FairState> _room;


    private Client _client;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            return;
        }

       
        Destroy(gameObject);
    }

     private async void Start()
    {   
        string serverHost = "localhost";
        string endpoint = $"ws://{serverHost}:{serverPort}";
        
        _client = new Client(endpoint);

        _room = await _client.JoinOrCreate<FairState>("fairsceneroom");
    
    } 
}
