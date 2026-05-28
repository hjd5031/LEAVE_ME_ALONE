using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkScoreManager : NetworkBehaviour
{
    private const string LogTag = "[NetworkScoreManager]";

    public static NetworkScoreManager Instance { get; private set; }

    public event Action<ulong, int> OnScoreChanged;
    public event Action<ulong> OnPlayerRemoved;

    private readonly Dictionary<ulong, int> scoresByClientId = new Dictionary<ulong, int>();

    public IReadOnlyDictionary<ulong, int> ScoresByClientId => scoresByClientId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"{LogTag} Duplicate NetworkScoreManager found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer || NetworkManager.Singleton == null)
        {
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            RegisterPlayer(clientId);
        }

        Debug.Log($"{LogTag} Server score manager ready.");
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    public override void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        base.OnDestroy();
    }

    public void AddScore(ulong clientId, int amount)
    {
        if (!IsServer)
        {
            Debug.LogWarning($"{LogTag} AddScore ignored on non-server instance.");
            return;
        }

        if (!scoresByClientId.ContainsKey(clientId))
        {
            RegisterPlayer(clientId);
        }

        scoresByClientId[clientId] += amount;
        int newScore = scoresByClientId[clientId];
        Debug.Log($"{LogTag} Client {clientId} score changed by {amount}. NewScore={newScore}");
        ScoreUpdatedClientRpc(clientId, newScore);
    }

    public int GetScore(ulong clientId)
    {
        return scoresByClientId.TryGetValue(clientId, out int score) ? score : 0;
    }

    public ulong GetWinnerClientId()
    {
        if (scoresByClientId.Count == 0)
        {
            Debug.LogWarning($"{LogTag} GetWinnerClientId called with no registered players.");
            return ulong.MaxValue;
        }

        ulong winnerClientId = ulong.MaxValue;
        int highestScore = int.MinValue;

        foreach (KeyValuePair<ulong, int> scoreEntry in scoresByClientId)
        {
            if (scoreEntry.Value > highestScore)
            {
                winnerClientId = scoreEntry.Key;
                highestScore = scoreEntry.Value;
            }
        }

        return winnerClientId;
    }

    private void HandleClientConnected(ulong clientId)
    {
        RegisterPlayer(clientId);
        Debug.Log($"{LogTag} Client connected. ClientId={clientId}");
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (!scoresByClientId.Remove(clientId))
        {
            return;
        }

        Debug.Log($"{LogTag} Client disconnected. ClientId={clientId}");
        PlayerRemovedClientRpc(clientId);
    }

    private void RegisterPlayer(ulong clientId)
    {
        if (scoresByClientId.ContainsKey(clientId))
        {
            return;
        }

        scoresByClientId.Add(clientId, 0);
        Debug.Log($"{LogTag} Registered client score. ClientId={clientId}");
        ScoreUpdatedClientRpc(clientId, 0);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ScoreUpdatedClientRpc(ulong clientId, int score)
    {
        scoresByClientId[clientId] = score;
        Debug.Log($"{LogTag} Score update received. ClientId={clientId} Score={score}");
        OnScoreChanged?.Invoke(clientId, score);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerRemovedClientRpc(ulong clientId)
    {
        scoresByClientId.Remove(clientId);
        Debug.Log($"{LogTag} Player removed from score table. ClientId={clientId}");
        OnPlayerRemoved?.Invoke(clientId);
    }
}
