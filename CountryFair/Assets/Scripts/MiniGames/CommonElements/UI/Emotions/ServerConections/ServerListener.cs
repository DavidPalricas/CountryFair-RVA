using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;
using Unity.VisualScripting;

/// <summary>
/// Connects to a TCP server to listen for emoji commands.
/// Handles connection lifecycle and threading.
/// </summary>
public class ServerListener : MonoBehaviour
{
    [Header("Server Connection Settings")]
    [SerializeField] 
    private string serverIP = "172.20.10.2";
    [SerializeField]
     private int serverPort = 50050;
    [SerializeField] 
    private ExpressionDisplay emojiDisplay;
    [SerializeField] 
    private SliderDisplay sliderDisplay;

    [SerializeField]
    private ExpressionDisplay faceDisplay;

    [Header("Update Rate")]
    [Tooltip("Interval in seconds between processing received messages")]
    [SerializeField] private float pollIntervalSeconds = 0.5f;

    private TcpClient _client;
    private Thread _receiveThread;
    private volatile bool _isRunning = false;

    // Latest-value buffer: receive thread writes, main thread reads.
    // No lock needed — worst case main thread reads a slightly stale value, which is acceptable for emotion display.
    private volatile string _latestMessage = null;


    private EmotionDisplay _currentDisplayMode;

    public enum DISPLAYMODE
    {
        EMOJI,
        FACE,
        SLIDER
    }

    private void Awake()
    {
        if (sliderDisplay == null || emojiDisplay == null || faceDisplay == null)
        {
            Debug.LogError("One or more types of emotion display are missing in ServerListener");
            return;
        }

        SetCurrentDisplayMode();
    }

    private void Start()
    {
        ConnectToTcpServer();
        InvokeRepeating(nameof(FlushLatestMessage), pollIntervalSeconds, pollIntervalSeconds);
    }

    /// <summary>
    /// Called on the main thread at pollIntervalSeconds cadence.
    /// Atomically swaps the latest message out and dispatches it.
    /// </summary>
    private void FlushLatestMessage()
    {
        // Interlocked.Exchange ensures we grab exactly one message and clear the buffer atomically.
        // If no new message arrived since last poll, we do nothing.
        string message = Interlocked.Exchange(ref _latestMessage, null);
        if (message == null)
        {
            return;
        } 

        _currentDisplayMode.ProcessServerString(message);
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

    private void ListenForData()
    {
        try
        {
            byte[] bytes = new byte[1024];

            while (_isRunning && _client != null && _client.Connected)
            {
                using NetworkStream stream = _client.GetStream();
                int length;

                while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    if (!_isRunning) break;

                    // Overwrite buffer with the latest — older unprocessed messages are intentionally discarded.
                    _latestMessage = Encoding.ASCII.GetString(bytes, 0, length);
                }
            }
        }
        catch (System.IO.IOException)
        {
            if (_isRunning)
                Debug.LogWarning("Connection lost with the server.");
        }
        catch (Exception e)
        {
            if (_isRunning)
                Debug.LogWarning($"Socket warning: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(FlushLatestMessage));
        _isRunning = false;
        _client?.Close();
        _client = null;
    }

    private void SetCurrentDisplayMode()
    {  
        float randomValue = Utils.RandomValueInRange(0f, 1f);

        if (randomValue <= 0.33f)
        {
           faceDisplay.gameObject.SetActive(false);
           sliderDisplay.gameObject.SetActive(false);

           _currentDisplayMode = emojiDisplay;

            return;
        }

        if (randomValue <= 0.66f)
        {     
            emojiDisplay.gameObject.SetActive(false);
            faceDisplay.gameObject.SetActive(false);

            _currentDisplayMode = sliderDisplay;

            return;
        }

        emojiDisplay.gameObject.SetActive(false);
        sliderDisplay.gameObject.SetActive(false);

        _currentDisplayMode = faceDisplay;
    }

    public void UpdateDisplayMode(DISPLAYMODE newMode)
    {   
        _currentDisplayMode.gameObject.SetActive(false);

        if (newMode == DISPLAYMODE.SLIDER)
        {   
            _currentDisplayMode = sliderDisplay;
            _currentDisplayMode.gameObject.SetActive(true);

            return;
        }

        if (newMode == DISPLAYMODE.FACE)
        {   
            _currentDisplayMode = faceDisplay;
            _currentDisplayMode.gameObject.SetActive(true);

            return;
        }
        
       _currentDisplayMode = emojiDisplay;
        _currentDisplayMode.gameObject.SetActive(true);
    }
}