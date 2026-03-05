using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;

public class CheatCodes : MonoBehaviour
{
    protected string _playerInput = "";

    /// <summary>
    /// Maximum length of any cheat code, used to limit input buffer size.
    /// Updated automatically when RegisterCheat() is called.
    /// </summary>
    protected int _maxCheatLength;

    /// <summary>
    /// Dictionary mapping cheat code strings to their corresponding commands (Actions).
    /// Subclasses register their own cheats via RegisterCheat().
    /// </summary>
    protected Dictionary<string, Action> _cheatCommands = new();

    /// <summary>
    /// Subscribes to keyboard text input and device change events when enabled.
    /// </summary>
    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        SubscribeKeyboard();
    }

    /// <summary>
    /// Unsubscribes from all input events when disabled.
    /// </summary>
    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;

        if (Keyboard.current != null)
        {
             Keyboard.current.onTextInput -= OnTextInput;
        }
           
    }

    /// <summary>
    /// Subscribes to the current keyboard if one is available.
    /// </summary>
    private void SubscribeKeyboard()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= OnTextInput; // avoid double subscription
            Keyboard.current.onTextInput += OnTextInput;
            Debug.Log($"[CheatCodes] Keyboard subscribed: {Keyboard.current.name}");

            return;
        }
       
        Debug.LogWarning("[CheatCodes] No keyboard detected.");
    }

    /// <summary>
    /// Listens for new devices being connected (e.g. Bluetooth keyboard on Quest)
    /// and subscribes to text input when a keyboard is added.
    /// </summary>
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Keyboard keyboard)
        {
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
            {
                keyboard.onTextInput -= OnTextInput; // avoid double subscription
                keyboard.onTextInput += OnTextInput;

                return;
            }

            if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected)
            {
                Debug.Log($"[CheatCodes] Keyboard disconnected: {keyboard.name}");
                keyboard.onTextInput -= OnTextInput;
            }
        }
    }

    /// <summary>
    /// Handles keyboard character input and checks for cheat code patterns.
    /// </summary>
    private void OnTextInput(char c)
    {
        if (!char.IsLetterOrDigit(c)){
            return;
        }

        _playerInput += c.ToString().ToLower();

        if (_playerInput.Length > _maxCheatLength)
        {
            _playerInput = _playerInput[^_maxCheatLength..];
        }
        
        CheckCheatCode();
    }

    protected virtual void CheckCheatCode()
    {
        Debug.LogError("CheckCheatCode should be overridden in derived classes.");
    }

    protected virtual void RegisterBaseCheats()
    {
        Debug.LogError("RegisterBaseCheats should be overridden in derived classes.");
    }

    /// <summary>
    /// Registers a cheat code with its associated command.
    /// Automatically updates _maxCheatLength so the input buffer is always large enough.
    /// </summary>
    /// <param name="code">The cheat code string.</param>
    /// <param name="command">The action to execute when the code is entered.</param>
    protected void RegisterCheat(string code, Action command)
    {
        _cheatCommands[code] = command;
    }
}