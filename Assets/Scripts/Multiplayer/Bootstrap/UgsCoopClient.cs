using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class UgsCoopClient : MonoBehaviour
{
    private const string LogTag = "[UgsCoopClient]";

    public static UgsCoopClient Instance { get; private set; }

    public event Action<string> OnStatusChanged;

    [SerializeField] private float quickJoinTimeoutSeconds = 10f;

    private ISession currentSession;
    private bool isBusy;

    public ISession CurrentSession => currentSession;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
#if !UNITY_SERVER
        EnsureInstance();
#endif
    }

    public static UgsCoopClient EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        GameObject gameObject = new GameObject(nameof(UgsCoopClient));
        DontDestroyOnLoad(gameObject);
        Instance = gameObject.AddComponent<UgsCoopClient>();
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async void JoinByCode(string joinCode)
    {
        await JoinByCodeAsync(joinCode);
    }

    public async void AutoMatchDedicatedServer()
    {
        await AutoMatchDedicatedServerAsync();
    }

    public async Task JoinByCodeAsync(string joinCode)
    {
        if (string.IsNullOrWhiteSpace(joinCode))
        {
            SetStatus("Enter a room code first.");
            return;
        }

        await RunExclusiveAsync(async () =>
        {
            await EnsureServicesReadyAsync();
            if (!ValidateNetworkManager())
            {
                return;
            }

            string trimmedCode = joinCode.Trim().ToUpperInvariant();
            SetStatus($"Joining room code {trimmedCode}...");

            JoinSessionOptions joinOptions = new JoinSessionOptions
            {
                Type = TomatoMultiplayerConstants.SessionType
            };

            currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(trimmedCode, joinOptions);
            SetStatus($"Joined session. Waiting for NGO connection... Code={currentSession.Code}");
        });
    }

    public async Task AutoMatchDedicatedServerAsync()
    {
        await RunExclusiveAsync(async () =>
        {
            await EnsureServicesReadyAsync();
            if (!ValidateNetworkManager())
            {
                return;
            }

            SetStatus("Searching for a dedicated co-op server...");

            QuickJoinOptions quickJoinOptions = new QuickJoinOptions
            {
                CreateSession = false,
                Timeout = TimeSpan.FromSeconds(Mathf.Max(1f, quickJoinTimeoutSeconds))
            };
            quickJoinOptions.Filters.Add(new FilterOption(
                FilterField.StringIndex1,
                TomatoMultiplayerConstants.SessionFilterValue,
                FilterOperation.Equal));

            SessionOptions sessionOptions = new SessionOptions
            {
                Type = TomatoMultiplayerConstants.SessionType
            };

            currentSession = await MultiplayerService.Instance.MatchmakeSessionAsync(quickJoinOptions, sessionOptions);
            SetStatus($"Matched session. Waiting for NGO connection... Code={currentSession.Code}");
        });
    }

    public async void LeaveCurrentSession()
    {
        if (currentSession == null)
        {
            SetStatus("No active co-op session.");
            return;
        }

        try
        {
            ISession leavingSession = currentSession;
            currentSession = null;
            await leavingSession.LeaveAsync();
            SetStatus("Left co-op session.");
        }
        catch (Exception exception)
        {
            SetStatus($"Leave failed: {exception.Message}");
        }
    }

    private async Task RunExclusiveAsync(Func<Task> action)
    {
        if (isBusy)
        {
            SetStatus("A co-op connection request is already running.");
            return;
        }

        isBusy = true;
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            SetStatus($"Co-op connection failed: {exception.Message}");
        }
        finally
        {
            isBusy = false;
        }
    }

    private static async Task EnsureServicesReadyAsync()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private bool ValidateNetworkManager()
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager == null)
        {
            SetStatus("NetworkManager is missing in this scene. Add NetworkManager + UnityTransport before using Co-op.");
            return false;
        }

        DontDestroyOnLoad(networkManager.gameObject);

        if (networkManager.NetworkConfig.NetworkTransport == null)
        {
            SetStatus("NetworkManager has no Network Transport assigned. Assign UnityTransport in the Inspector.");
            return false;
        }

        if (networkManager.NetworkConfig.PlayerPrefab == null)
        {
            SetStatus("NetworkManager has no Player Prefab assigned. Assign the NetworkPlayer prefab first.");
            return false;
        }

        if (networkManager.IsListening)
        {
            SetStatus("NetworkManager is already running.");
            return false;
        }

        return true;
    }

    private void SetStatus(string message)
    {
        Debug.Log($"{LogTag} {message}");
        OnStatusChanged?.Invoke(message);
    }
}
