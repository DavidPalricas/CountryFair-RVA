using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using System;

/// <summary>
/// Manages expressions activation (emojis or faces) and synchronization over the network.
/// Works in conjunction with EmojiDisplayManager to support both server and slider modes.
/// </summary>
/// <remarks>
/// This component handles the core expression visualization logic.
/// </remarks>
public class ExpressionDisplay : EmotionDisplay
{
    /// <summary>
    /// Dictionary to look up expression GameObjects by name.
    /// </summary>
    private readonly Dictionary<string, GameObject> _expressions = new();

    /// <summary>
    /// The currently active expression GameObject.
    /// </summary>
    private GameObject _currentExpressionActive = null;

      /// <summary>
    /// Network variable to maintain expression state across clients.
    /// Only the server has permission to write to this variable.
    /// </summary>
    private readonly NetworkVariable<EXPRESSION_TYPE> _netExpressionState = new(
        EXPRESSION_TYPE.NEUTRAL,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    /// <summary>
    /// Enumeration of supported expression types.
    /// </summary>
    public enum EXPRESSION_TYPE {
        /// <summary>Represents a sad expression.</summary>
        SAD,
        /// <summary>Represents a happy expression.</summary>
        HAPPY,
        /// <summary>Represents an angry expression.</summary>
        ANGRY,
        /// <summary>Represents a disgusted expression.</summary>
        DISGUST,
        /// <summary>Represents a surprised expression.</summary>
        SURPRISE,
        /// <summary>Represents a fearful expression.</summary>
        FEAR,
        /// <summary>Represents a neutral expression.</summary>
        NEUTRAL
    }

    /// <summary>
    /// Unity Awake method. Initializes internal state.
    /// </summary>
    /// <remarks>
    /// This component focuses solely on expression visualization and network sync.
    /// </remarks>
    private void Awake()
    {
        SetExpressions();
    }

    /// <summary>
    /// Called when the object is spawned on the network.
    /// Subscribes to state changes and updates visuals.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        _netExpressionState.OnValueChanged += OnExpressionChanged;
        
        // Force initial visual update without triggering send logic
        _isUpdatingFromNetwork = true;
        UpdateVisuals(_netExpressionState.Value);
        _isUpdatingFromNetwork = false;
    }

    /// <summary>
    /// Called when the object is despawned from the network.
    /// Unsubscribes from state changes.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        _netExpressionState.OnValueChanged -= OnExpressionChanged;
    }

    /// <summary>
    /// Callback when the network variable changes.
    /// </summary>
    /// <param name="previous">Previous state.</param>
    /// <param name="current">New state.</param>
    private void OnExpressionChanged(EXPRESSION_TYPE previous, EXPRESSION_TYPE current)
    {
        // Protection: Notify UpdateVisuals that this came from the network
        _isUpdatingFromNetwork = true;
        UpdateVisuals(current);
        _isUpdatingFromNetwork = false;
    }

    /// <summary>
    /// Updates the visual representation of the active expression.
    /// Does both local visual updates and network synchronization.
    /// </summary>
    /// <param name="type">The expression type to display.</param>
    public void UpdateVisuals(EXPRESSION_TYPE type)
    { 
        // 1. VISUAL UPDATE (Local and Immediate)
        string expressionName = type.ToString().ToLower();

        if (_expressions.TryGetValue(expressionName, out GameObject targetEmoji))
        {   
            if (_currentExpressionActive != targetEmoji)
            {
                if (_currentExpressionActive != null) _currentExpressionActive.SetActive(false);
                targetEmoji.SetActive(true);
                _currentExpressionActive = targetEmoji;
            }
        }
        else
        {
            Debug.LogError("Invalid expression type: " + type);
            return; // If it fails visually, do not propagate to the network
        }

        // 2. NETWORK LOGIC (Only executes if NOT updating via network callback)
        if (!_isUpdatingFromNetwork)
        {
            SyncToNetwork(type);
        }
    }

    /// <summary>
    /// Isolated logic for sending state to the network.
    /// </summary>
    /// <param name="displayValue">The expression type to synchronize.</param>
    private void SyncToNetwork(EXPRESSION_TYPE type)
    {
        // Checks if NetworkManager exists and is running
        if (NetworkManager.Singleton == null || (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer))
        {
            return; // We are offline (test mode), do nothing on network
        }

        // Avoid redundant sending if the value is already the same
        if (_netExpressionState.Value == type) return;

        if (IsServer)
        {
            _netExpressionState.Value = type;

            return;
        }

        if (IsOwner) // If it's a local cheat by the owner
        {
            RequestDisplayChangeServerRpc(type);
        }
    }

    /// <summary>
    /// Server RPC to request an expression change from a client.
    /// </summary>
    /// <param name="newType">The new expression type requested.</param>
    [ServerRpc]
    private void RequestDisplayChangeServerRpc(EXPRESSION_TYPE newType)
    {
        _netExpressionState.Value = newType;
    }

    // --- UTILS E PARSING ---

    /// <summary>
    /// Processes a string message (e.g., from server) and updates the expression.
    /// </summary>
    /// <param name="message">String representation of the EmojiType.</param>
    public override void ProcessServerString(string message)
    {
        base.ProcessServerString(message);

        string emotionName = message.Split(_messageSeparator)[0];

        if (Enum.TryParse(emotionName, true, out EXPRESSION_TYPE result))
        {
            UpdateVisuals(result); 
        }
    }

    /// <summary>
    /// Initializes the expressions dictionary from child objects.
    /// </summary>
    private void SetExpressions()
    {
        foreach (GameObject expression in Utils.GetChildren(transform))
        {
            string expressionName = expression.name.ToLower();
            _expressions[expressionName] = expression;
            expression.SetActive(false);
        }

        if (_expressions.ContainsKey("neutral")) _currentExpressionActive = _expressions["neutral"];
    }
}