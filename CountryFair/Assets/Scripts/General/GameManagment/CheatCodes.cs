using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;

public class CheatCodes: MonoBehaviour
{
    protected string _playerInput = "";

    /// <summary>
    /// Maximum length of any cheat code, used to limit input buffer size.
    /// </summary>
    protected int _maxCheatLength;

    /// <summary>
    /// Dictionary mapping cheat code strings to their corresponding commands (Actions).
    /// Subclasses register their own cheats via RegisterCheat().
    /// </summary>
    protected Dictionary<string, Action> _cheatCommands = new();


    /// <summary>
    /// Subscribes to keyboard text input events when the component is enabled.
    /// </summary>
    private void OnEnable()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput += OnTextInput;
        }
            
    }

    /// <summary>
    /// Unsubscribes from keyboard text input events when the component is disabled.
    /// </summary>
    private void OnDisable()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= OnTextInput;
        }
            
    }


    /// <summary>
    /// Handles keyboard character input and checks for cheat code patterns.
    /// </summary>
    private void OnTextInput(char c)
    {
        if (!char.IsLetterOrDigit(c)) return;

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
    /// Subclasses call this in Start() (after base.Start()) to add game-specific cheats.
    /// </summary>
    /// <param name="code">The cheat code string.</param>
    /// <param name="command">The action to execute when the code is entered.</param>
    protected void RegisterCheat(string code, Action command)
    {
        _cheatCommands[code] = command;
    }

}