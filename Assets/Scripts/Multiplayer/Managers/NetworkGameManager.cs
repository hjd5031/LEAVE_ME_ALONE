using Unity.Netcode;
using UnityEngine;

public enum MatchState
{
    Waiting,
    Countdown,
    Playing,
    Finished
}

[RequireComponent(typeof(NetworkObject))]
public class NetworkGameManager : NetworkBehaviour
{
    private const string LogTag = "[NetworkGameManager]";

    public static NetworkGameManager Instance { get; private set; }

    public NetworkVariable<MatchState> CurrentMatchState = new NetworkVariable<MatchState>(
        MatchState.Waiting,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<float> RemainingTime = new NetworkVariable<float>(
        300f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] private float matchDurationSeconds = 300f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"{LogTag} Duplicate NetworkGameManager found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            RemainingTime.Value = matchDurationSeconds;
            CurrentMatchState.Value = MatchState.Waiting;
            Debug.Log($"{LogTag} Server spawned. Match waiting. Duration={matchDurationSeconds:0}s");
        }

        CurrentMatchState.OnValueChanged += OnMatchStateChanged;
        RemainingTime.OnValueChanged += OnRemainingTimeChanged;
    }

    public override void OnNetworkDespawn()
    {
        CurrentMatchState.OnValueChanged -= OnMatchStateChanged;
        RemainingTime.OnValueChanged -= OnRemainingTimeChanged;
    }

    public override void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        base.OnDestroy();
    }

    private void Update()
    {
        if (!IsServer || CurrentMatchState.Value != MatchState.Playing)
        {
            return;
        }

        RemainingTime.Value = Mathf.Max(0f, RemainingTime.Value - Time.deltaTime);
        if (RemainingTime.Value <= 0f)
        {
            EndMatch();
        }
    }

    public void StartMatch()
    {
        if (!IsServer)
        {
            Debug.LogWarning($"{LogTag} StartMatch ignored because this instance is not the server.");
            return;
        }

        RemainingTime.Value = matchDurationSeconds;
        CurrentMatchState.Value = MatchState.Playing;
        Debug.Log($"{LogTag} Match started. Duration={matchDurationSeconds:0}s");
    }

    public void EndMatch()
    {
        if (!IsServer)
        {
            Debug.LogWarning($"{LogTag} EndMatch ignored because this instance is not the server.");
            return;
        }

        if (CurrentMatchState.Value == MatchState.Finished)
        {
            return;
        }

        RemainingTime.Value = 0f;
        CurrentMatchState.Value = MatchState.Finished;
        Debug.Log($"{LogTag} Match ended.");
        MatchEndedClientRpc();
    }

    [Rpc(SendTo.NotServer)]
    private void MatchEndedClientRpc()
    {
        Debug.Log($"{LogTag} Match ended notification received on client.");
    }

    private void OnMatchStateChanged(MatchState previousValue, MatchState newValue)
    {
        Debug.Log($"{LogTag} MatchState changed: {previousValue} -> {newValue}");
    }

    private void OnRemainingTimeChanged(float previousValue, float newValue)
    {
        // UI can subscribe to RemainingTime later. Avoid per-frame log spam here.
    }
}
