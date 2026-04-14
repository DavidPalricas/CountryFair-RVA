using UnityEngine;
using Unity.Netcode;
using System.Linq;

public abstract class EmotionDisplay: NetworkBehaviour
{
    protected bool _isUpdatingFromNetwork = false;

    protected char _messageSeparator = ';';

    public override void OnNetworkSpawn()
    {
       Debug.LogError("OnNetworkSpawn should be overridden in derived classes of EmotionDisplay.");
    }

    public override void OnNetworkDespawn()
    {
        Debug.LogError("OnNetworkDespawn should be overridden in derived classes of EmotionDisplay.");
    }

    public virtual void ProcessServerString(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        } 

        message = message.Trim();

        int seperatorCount = message.Count(c => c == ';');

        if (seperatorCount != 1)
        {
            Debug.LogWarning("Server message format invalid: " + message + "must have only one ';' as a seprator.");

            return;
        }
    }
}