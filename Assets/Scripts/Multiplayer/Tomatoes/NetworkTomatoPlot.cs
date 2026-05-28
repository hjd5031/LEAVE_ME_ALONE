using Unity.Netcode;
using UnityEngine;

public enum TomatoState
{
    Empty,
    Planted,
    Growing,
    Ripe,
    Harvested
}

[RequireComponent(typeof(NetworkObject))]
public class NetworkTomatoPlot : NetworkBehaviour
{
    private const string LogTag = "[NetworkTomatoPlot]";

    public NetworkVariable<TomatoState> State = new NetworkVariable<TomatoState>(
        TomatoState.Empty,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] private TomatoState initialState = TomatoState.Empty;
    [SerializeField] private float harvestRange = 3f;
    [SerializeField] private int scoreValue = 1;

    public override void OnNetworkSpawn()
    {
        State.OnValueChanged += OnTomatoStateChanged;

        if (IsServer)
        {
            State.Value = initialState;
            Debug.Log($"{LogTag} Server spawned tomato plot. InitialState={initialState}");
        }
    }

    public override void OnNetworkDespawn()
    {
        State.OnValueChanged -= OnTomatoStateChanged;
    }

    public void ServerSetState(TomatoState tomatoState)
    {
        if (!IsServer)
        {
            Debug.LogWarning($"{LogTag} ServerSetState ignored on non-server instance.");
            return;
        }

        State.Value = tomatoState;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestHarvestServerRpc(RpcParams rpcParams = default)
    {
        if (!IsServer)
        {
            return;
        }

        ulong clientId = rpcParams.Receive.SenderClientId;
        if (!CanHarvest(clientId, out NetworkObject playerObject))
        {
            return;
        }

        State.Value = TomatoState.Harvested;

        if (NetworkScoreManager.Instance != null)
        {
            NetworkScoreManager.Instance.AddScore(clientId, scoreValue);
        }
        else
        {
            Debug.LogWarning($"{LogTag} Harvest accepted, but NetworkScoreManager.Instance is missing.");
        }

        Debug.Log($"{LogTag} Client {clientId} harvested tomato plot {NetworkObjectId}.");
        HarvestedClientRpc(clientId, playerObject.transform.position);
    }

    private bool CanHarvest(ulong clientId, out NetworkObject playerObject)
    {
        playerObject = null;

        if (NetworkGameManager.Instance == null || NetworkGameManager.Instance.CurrentMatchState.Value != MatchState.Playing)
        {
            Debug.Log($"{LogTag} Harvest rejected for client {clientId}. Match is not playing.");
            return false;
        }

        if (State.Value != TomatoState.Ripe)
        {
            Debug.Log($"{LogTag} Harvest rejected for client {clientId}. TomatoState={State.Value}");
            return false;
        }

        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient networkClient) ||
            networkClient.PlayerObject == null)
        {
            Debug.Log($"{LogTag} Harvest rejected for client {clientId}. Player object not found.");
            return false;
        }

        playerObject = networkClient.PlayerObject;
        float distance = Vector3.Distance(playerObject.transform.position, transform.position);
        if (distance > harvestRange)
        {
            Debug.Log($"{LogTag} Harvest rejected for client {clientId}. Distance={distance:0.00} Range={harvestRange:0.00}");
            return false;
        }

        return true;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void HarvestedClientRpc(ulong clientId, Vector3 harvestPosition)
    {
        Debug.Log($"{LogTag} Harvest VFX/SFX hook. ClientId={clientId} Position={harvestPosition}");
    }

    private void OnTomatoStateChanged(TomatoState previousValue, TomatoState newValue)
    {
        Debug.Log($"{LogTag} TomatoState changed: {previousValue} -> {newValue}");
    }
}
