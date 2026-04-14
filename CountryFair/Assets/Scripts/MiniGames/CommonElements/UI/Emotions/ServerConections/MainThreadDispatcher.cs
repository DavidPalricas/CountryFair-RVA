using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// A helper class to dispatch actions to the main Unity thread.
/// Useful for handling network or thread callbacks that need to interact with Unity API.
/// </summary>
public class MainThreadDispatcher : MonoBehaviour
{
    /// <summary>
    /// A queue of actions to be executed on the main thread.
    /// </summary>
    private static readonly Queue<Action> _executionQueue = new();

    /// <summary>
    /// Unity Update method. Executes all queued actions on the main thread.
    /// </summary>
    public void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    /// <summary>
    /// Enqueues an action to be executed on the main thread.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public static void Enqueue(Action action)
    {
        if (action == null)
        {
            return;
        }
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }
}