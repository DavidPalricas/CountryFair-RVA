using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP; // Necessário para aceder ao UnityTransport

public class ConnectionManager : MonoBehaviour
{
    // Define aqui os dados, acessíveis via Inspector se necessário
    [SerializeField] 
    private string serverIp = "172.20.10.2";
    [SerializeField] 
    private ushort serverPort = 50050;
    [SerializeField] private bool autoConnectClient = true;

    private void Start()
    {
        ConfigureTransport();

        if (autoConnectClient)
        {
            // Debug.Log($"Connecting to {serverIp}:{serverPort}...");
            NetworkManager.Singleton.StartClient();
        }
    }

    public void StartHost()
    {
        ConfigureTransport();
        NetworkManager.Singleton.StartHost();
    }

    private void ConfigureTransport()
    {
        if (NetworkManager.Singleton.TryGetComponent<UnityTransport>(out var transport))
        {
            // É aqui que se injeta o IP e a Porta programaticamente
            transport.SetConnectionData(serverIp, serverPort);
        }
        else
        {
            Debug.LogError("UnityTransport não encontrado no NetworkManager!");
        }
    }
}