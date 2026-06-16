using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

#if UNITY_SERVER || ENABLE_UCS_SERVER
using Unity.Services.Authentication.Server;
#endif

public class UgsDedicatedServerSession : MonoBehaviour
{
    private const string LogTag = "[UgsDedicatedServerSession]";

#if UNITY_SERVER || ENABLE_UCS_SERVER
    private IServerSession serverSession;
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
#if UNITY_SERVER || ENABLE_UCS_SERVER
        if (!IsMpsSessionRequested())
        {
            return;
        }

        if (FindFirstObjectByType<UgsDedicatedServerSession>() != null)
        {
            return;
        }

        GameObject bootstrapObject = new GameObject(nameof(UgsDedicatedServerSession));
        DontDestroyOnLoad(bootstrapObject);
        bootstrapObject.AddComponent<UgsDedicatedServerSession>();
#endif
    }

    private async void Start()
    {
#if UNITY_SERVER || ENABLE_UCS_SERVER
        DontDestroyOnLoad(gameObject);
        await StartServerSessionAsync();
#else
        Destroy(gameObject);
        await Task.CompletedTask;
#endif
    }

#if UNITY_SERVER || ENABLE_UCS_SERVER
    private async Task StartServerSessionAsync()
    {
        ServerOptions options = ParseServerOptions();

        if (!ValidateNetworkManager())
        {
            return;
        }

        if (!StartNetcodeServer(options))
        {
            return;
        }

        try
        {
            Debug.Log($"{LogTag} Initializing Unity Services...");
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            await SignInServerAsync(options);

            SessionOptions sessionOptions = new SessionOptions
            {
                Type = TomatoMultiplayerConstants.SessionType,
                Name = options.SessionName,
                MaxPlayers = options.MaxPlayers,
                IsPrivate = false,
                IsLocked = false,
                SessionProperties = new Dictionary<string, SessionProperty>
                {
                    {
                        TomatoMultiplayerConstants.SessionFilterKey,
                        new SessionProperty(
                            TomatoMultiplayerConstants.SessionFilterValue,
                            VisibilityPropertyOptions.Public,
                            PropertyIndex.String1)
                    }
                }
            }.WithDirectNetwork(options.ListenIp, options.PublishIp, options.Port);

            Debug.Log($"{LogTag} Creating dedicated server session. Listen={options.ListenIp}:{options.Port} Publish={options.PublishIp}:{options.Port}");

            if (string.IsNullOrWhiteSpace(options.SessionId))
            {
                serverSession = await MultiplayerServerService.Instance.CreateSessionAsync(sessionOptions);
            }
            else
            {
                serverSession = await MultiplayerServerService.Instance.CreateSessionAsync(options.SessionId, sessionOptions);
            }

            Debug.Log($"{LogTag} Session online. Id={serverSession.Id} Code={serverSession.Code} Name={serverSession.Name}");
            Debug.Log($"{LogTag} Give this room code to clients, or use Auto Match to find this public server session.");
        }
        catch (Exception exception)
        {
            Debug.LogError($"{LogTag} Failed to create server session: {exception}");
        }
    }

    private static async Task SignInServerAsync(ServerOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ServerKey) && !string.IsNullOrWhiteSpace(options.ServerSecret))
        {
            Debug.Log($"{LogTag} Signing in with service account credentials.");
            await ServerAuthenticationService.Instance.SignInWithServiceAccountAsync(options.ServerKey, options.ServerSecret);
            return;
        }

        Debug.Log($"{LogTag} No service account credentials were provided. Trying hosted-server sign-in.");
        try
        {
            await ServerAuthenticationService.Instance.SignInFromServerAsync();
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                "Local personal servers need service account credentials. Run with -serverKey <keyId> -serverSecret <secret>, " +
                "or set UGS_SERVICE_ACCOUNT_KEY_ID and UGS_SERVICE_ACCOUNT_SECRET environment variables.",
                exception);
        }
    }

    private static bool ValidateNetworkManager()
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager == null)
        {
            Debug.LogError($"{LogTag} NetworkManager.Singleton is missing. DedicatedServerScene must contain NetworkManager.");
            return false;
        }

        DontDestroyOnLoad(networkManager.gameObject);

        if (networkManager.NetworkConfig.NetworkTransport == null)
        {
            UnityTransport transport = networkManager.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError($"{LogTag} UnityTransport is missing on NetworkManager.");
                return false;
            }

            networkManager.NetworkConfig.NetworkTransport = transport;
            Debug.Log($"{LogTag} Assigned UnityTransport to NetworkManager.NetworkConfig.");
        }

        if (networkManager.NetworkConfig.PlayerPrefab == null)
        {
            Debug.LogWarning($"{LogTag} Player Prefab is not assigned. Clients can join the session, but NGO cannot spawn players yet.");
        }

        return true;
    }

    private static bool StartNetcodeServer(ServerOptions options)
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager == null)
        {
            Debug.LogError($"{LogTag} NetworkManager.Singleton is missing. Cannot start NGO server.");
            return false;
        }

        if (networkManager.IsListening)
        {
            Debug.Log($"{LogTag} NetworkManager is already listening.");
            return true;
        }

        UnityTransport transport = networkManager.NetworkConfig.NetworkTransport as UnityTransport;
        if (transport == null)
        {
            transport = networkManager.GetComponent<UnityTransport>();
        }

        if (transport == null)
        {
            Debug.LogError($"{LogTag} UnityTransport is missing on NetworkManager.");
            return false;
        }

        networkManager.NetworkConfig.NetworkTransport = transport;
        transport.SetConnectionData(options.PublishIp, options.Port, options.ListenIp);

        bool started = networkManager.StartServer();
        Debug.Log($"{LogTag} NGO StartServer requested. Listen={options.ListenIp}:{options.Port} Publish={options.PublishIp}:{options.Port} Started={started}");
        return started;
    }

    private static ServerOptions ParseServerOptions()
    {
        string[] args = Environment.GetCommandLineArgs();
        ServerOptions options = new ServerOptions
        {
            Port = TomatoMultiplayerConstants.DefaultServerPort,
            ListenIp = TomatoMultiplayerConstants.DefaultServerListenAddress,
            PublishIp = TomatoMultiplayerConstants.DefaultServerAddress,
            MaxPlayers = 8,
            SessionName = "Tomato Co-op Server",
            ServerKey = Environment.GetEnvironmentVariable("UGS_SERVICE_ACCOUNT_KEY_ID"),
            ServerSecret = Environment.GetEnvironmentVariable("UGS_SERVICE_ACCOUNT_SECRET")
        };

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (EqualsArg(arg, "-port") && TryGetNext(args, i, out string portText))
            {
                if (ushort.TryParse(portText, out ushort port))
                    options.Port = port;
                i++;
            }
            else if (EqualsArg(arg, "-listenIp") && TryGetNext(args, i, out string listenIp))
            {
                options.ListenIp = listenIp;
                i++;
            }
            else if (EqualsArg(arg, "-publishIp") && TryGetNext(args, i, out string publishIp))
            {
                options.PublishIp = publishIp;
                i++;
            }
            else if (EqualsArg(arg, "-maxPlayers") && TryGetNext(args, i, out string maxPlayersText))
            {
                if (int.TryParse(maxPlayersText, out int maxPlayers))
                    options.MaxPlayers = Mathf.Max(1, maxPlayers);
                i++;
            }
            else if (EqualsArg(arg, "-sessionName") && TryGetNext(args, i, out string sessionName))
            {
                options.SessionName = sessionName;
                i++;
            }
            else if (EqualsArg(arg, "-sessionId") && TryGetNext(args, i, out string sessionId))
            {
                options.SessionId = sessionId;
                i++;
            }
            else if (EqualsArg(arg, "-serverKey") && TryGetNext(args, i, out string serverKey))
            {
                options.ServerKey = serverKey;
                i++;
            }
            else if (EqualsArg(arg, "-serverSecret") && TryGetNext(args, i, out string serverSecret))
            {
                options.ServerSecret = serverSecret;
                i++;
            }
        }

        return options;
    }

    private struct ServerOptions
    {
        public ushort Port;
        public string ListenIp;
        public string PublishIp;
        public int MaxPlayers;
        public string SessionName;
        public string SessionId;
        public string ServerKey;
        public string ServerSecret;
    }
#endif

    private static bool IsMpsSessionRequested()
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (EqualsArg(args[i], "-useMpsSession") || EqualsArg(args[i], "-mpsSession"))
            {
                return true;
            }

            if (EqualsArg(args[i], "-mode") &&
                TryGetNext(args, i, out string mode) &&
                (string.Equals(mode, "mpsserver", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(mode, "sessionserver", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryGetNext(string[] args, int index, out string value)
    {
        int nextIndex = index + 1;
        if (nextIndex < args.Length)
        {
            value = args[nextIndex];
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static bool EqualsArg(string left, string right)
    {
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }
}
