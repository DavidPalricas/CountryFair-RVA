using UnityEngine;
using Colyseus;
using System.Collections.Generic;

public class ConnectToWebApp : MonoBehaviour
{   
    [SerializeField]
    private int serverPort = 2567;
    private static ConnectToWebApp instance = null;

   private Room<FairState> room;

    private Client _client;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            return;
        }

       
        Destroy(gameObject);
    }

     private async void Start()
    {   
        string serverHost = "localhost";
        string endpoint = $"ws://{serverHost}:{serverPort}";

        string plataform = "game";
        
        _client = new Client(endpoint);

        room = await _client.JoinOrCreate<FairState>("fairsceneroom", new Dictionary<string, object> { { "platform", plataform } });
       

       if (room == null)
        {
            Debug.LogError("Failed to join or create the room.");
        }
    } 


    public async void UpdateFairState(Dictionary<string, string> fairState)
    {
        await room.Send("updateFairState", fairState);
    }
}
