using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP; // Necessário para aceder ao UnityTransport

/// <summary>
/// Configures the Unity Transport IP and port, then auto-connects as a client on Start.
/// Call <see cref="StartHost"/> manually to launch a host instead (used on the therapist device).
/// </summary>
public class ConnectionManager : MonoBehaviour
{
    /// <summary>Target server IP address injected into <see cref="UnityTransport"/> at startup.</summary>
    [SerializeField]
    private string serverIp = "172.20.10.2";
    /// <summary>Target server port injected into <see cref="UnityTransport"/> at startup.</summary>
    [SerializeField]
    private ushort serverPort = 50050;
    /// <summary>When true, <c>Start</c> automatically calls <see cref="NetworkManager.StartClient"/>.</summary>
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

    /// <summary>
    /// Configures the transport with the assigned IP/port and starts the NetworkManager as host.
    /// </summary>
    /// <remarks>Invocado via Inspector num botão de debug para lançar o jogo como host na máquina do terapeuta.</remarks>
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