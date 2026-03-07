using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;

/// <summary>
/// Connects to a TCP server to listen for emoji commands.
/// Handles connection lifecycle and threading.
/// </summary>
[RequireComponent(typeof(ActivateEmoji))]
public class ServerListener : MonoBehaviour
{ 
    [Header("Server Connection Settings")]
    /// <summary>
    /// The IP address of the server to connect to.
    /// </summary>
    [SerializeField] private string serverIP = "172.20.10.2"; 

    /// <summary>
    /// The port number of the server to connect to.
    /// </summary>
    [SerializeField] private int serverPort = 50050;

    /// <summary>
    /// Reference to the emoji script component.
    /// </summary>
    private ActivateEmoji _emojiScript; 
    
    /// <summary>
    /// The TCP client used for communication.
    /// </summary>
    private TcpClient _client;

    /// <summary>
    /// The thread responsible for listening to incoming data.
    /// </summary>
    private Thread _receiveThread;
    
    /// <summary>
    /// Critical flag to control the thread's life and safe shutdown.
    /// Values written by one thread are immediately visible to others (volatile).
    /// </summary>
    private volatile bool _isRunning = false; 

    /// <summary>
    /// Unity Awake method. Initializes references.
    /// </summary>
    private void Awake()
    {
        _emojiScript = GetComponent<ActivateEmoji>();
    }

    /// <summary>
    /// Unity Start method. Begins the connection process.
    /// </summary>
    private void Start()
    {
        ConnectToTcpServer();
    }

private void ConnectToTcpServer()
{
    string sceneName = gameObject.scene.name;

    Thread connectThread = new(() =>
    {
        try
        {
            _client = new TcpClient();
            IAsyncResult result = _client.BeginConnect(serverIP, serverPort, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

            if (!success || !_client.Connected)
            {
                _client.Close();
                _client = null;
                Debug.LogWarning($"[Optional] Could not connect to emotion server in scene {sceneName}. Timeout.");
                return;
            }

            _client.EndConnect(result);
            _client.ReceiveTimeout = 5000;
            _isRunning = true;

            _receiveThread = new Thread(ListenForData) { IsBackground = true };
            _receiveThread.Start();
        }
        catch (Exception e)
        {
            _isRunning = false;
            Debug.LogWarning($"[Optional] Could not connect to emotion server in scene {sceneName}. Reason: {e.Message}");
        }
    })
    { IsBackground = true };

    connectThread.Start();
}

    /// <summary>
    /// Listens for incoming data on a separate thread.
    /// Manages the data stream and dispatches messages to the main thread.
    /// </summary>
    private void ListenForData()
    {
        try 
        {
            byte[] bytes = new byte[1024];
            
            while (_isRunning && _client != null && _client.Connected) 
            {
                using (NetworkStream stream = _client.GetStream()) 
                {
                    int length;

                    // Read blocks execution, but if we close the client in OnDestroy, it throws an exception and exits
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        if (!_isRunning){
                            break;
                        } 

                        byte[] incomingData = new byte[length];
                        Array.Copy(bytes, 0, incomingData, 0, length);

                        string serverMessage = Encoding.ASCII.GetString(incomingData);

                        // Send to Main Thread
                        MainThreadDispatcher.Enqueue(() =>
                        {
                            // Small extra safety check to avoid error if object was destroyed in the meantime
                            if (_emojiScript != null) 
                            {
                                _emojiScript.ProcessServerString(serverMessage);
                            }
                        });
                    }
                }
            }
        }
        catch (System.IO.IOException) 
        {
            // This normally happens when we close the socket when changing scenes.
            // Ignore if we are closing explicitly.
            if (_isRunning){
                Debug.LogWarning("Connection lost with the server.");
            }
        }
        catch (Exception e) 
        {
            if (_isRunning){
                Debug.LogWarning($"Socket warning: {e.Message}"); // Also changed to Warning here
            } 
        }
    }

    /// <summary>
    /// Unity OnDestroy method.
    /// Cleans up the connection and stops the thread to prevent memory leaks and zombie threads.
    /// </summary>
    /// <remarks>
    /// --- THE MOST IMPORTANT PART ---
    /// This ensures the old connection dies before the new one is born.
    /// </remarks>
    private void OnDestroy()
    {
        // Only logs if it was running, to avoid spam if it never connected
        if (_isRunning) {
           // Debug.Log($"[SCENE: {gameObject.scene.name}] Cleaning up connection...");
        }
        
        _isRunning = false;
        _client?.Close(); // This forces the stream.Read to stop immediately
        _client = null;
    }
}