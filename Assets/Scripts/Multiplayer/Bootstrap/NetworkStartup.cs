using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

[RequireComponent(typeof(NetworkManager))]
[RequireComponent(typeof(UnityTransport))]
public class NetworkStartup : MonoBehaviour
{
    private const string LogTag = "[NetworkStartup]";

    [SerializeField] private string defaultIp = TomatoMultiplayerConstants.DefaultServerAddress;
    [SerializeField] private ushort defaultPort = TomatoMultiplayerConstants.DefaultServerPort;

    private enum StartupMode
    {
        None,
        Server,
        Client,
        Host
    }

    private struct StartupOptions
    {
        public StartupMode Mode;
        public string Ip;
        public ushort Port;
    }

    private void Start()
    {
        StartupOptions options = ParseCommandLineArguments();

        if (IsMultiplayerServicesSessionRequested())
        {
            Debug.Log($"{LogTag} Multiplayer Services session bootstrap selected. Direct command-line startup skipped.");
            return;
        }

#if UNITY_SERVER
        options.Mode = StartupMode.Server;
        Debug.Log($"{LogTag} UNITY_SERVER build detected. Starting dedicated server automatically.");
#endif

        if (options.Mode == StartupMode.None)
        {
            Debug.Log($"{LogTag} No network mode selected. Use -mode server, -mode client, or -mode host.");
            return;
        }

        StartNetwork(options);
    }

    private StartupOptions ParseCommandLineArguments()
    {
        StartupOptions options = new StartupOptions
        {
            Mode = StartupMode.None,
            Ip = defaultIp,
            Port = defaultPort
        };

        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (string.Equals(arg, "-mode", StringComparison.OrdinalIgnoreCase) && TryGetNext(args, i, out string mode))
            {
                options.Mode = ParseMode(mode);
                i++;
            }
            else if (string.Equals(arg, "-ip", StringComparison.OrdinalIgnoreCase) && TryGetNext(args, i, out string ip))
            {
                options.Ip = ip;
                i++;
            }
            else if (string.Equals(arg, "-port", StringComparison.OrdinalIgnoreCase) && TryGetNext(args, i, out string portText))
            {
                if (ushort.TryParse(portText, out ushort port))
                {
                    options.Port = port;
                }
                else
                {
                    Debug.LogWarning($"{LogTag} Invalid port '{portText}'. Falling back to {defaultPort}.");
                }

                i++;
            }
        }

        return options;
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

    private static StartupMode ParseMode(string mode)
    {
        if (string.Equals(mode, "server", StringComparison.OrdinalIgnoreCase))
        {
            return StartupMode.Server;
        }

        if (string.Equals(mode, "client", StringComparison.OrdinalIgnoreCase))
        {
            return StartupMode.Client;
        }

        if (string.Equals(mode, "host", StringComparison.OrdinalIgnoreCase))
        {
            return StartupMode.Host;
        }

        Debug.LogWarning($"{LogTag} Unknown mode '{mode}'. Network startup skipped.");
        return StartupMode.None;
    }

    private static bool IsMultiplayerServicesSessionRequested()
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "-useMpsSession", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(args[i], "-mpsSession", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(args[i], "-mode", StringComparison.OrdinalIgnoreCase) &&
                TryGetNext(args, i, out string mode) &&
                (string.Equals(mode, "mpsserver", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(mode, "sessionserver", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }

    private void StartNetwork(StartupOptions options)
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager == null)
        {
            Debug.LogError($"{LogTag} NetworkManager.Singleton is missing. Attach this script to the NetworkManager GameObject.");
            return;
        }

        UnityTransport transport = GetComponent<UnityTransport>();
        if (transport == null)
        {
            Debug.LogError($"{LogTag} UnityTransport is missing. Attach UnityTransport to the same GameObject.");
            return;
        }

        if (networkManager.NetworkConfig.NetworkTransport == null)
        {
            networkManager.NetworkConfig.NetworkTransport = transport;
            Debug.Log($"{LogTag} Assigned UnityTransport to NetworkManager.NetworkConfig.");
        }

        if (networkManager.IsListening)
        {
            Debug.LogWarning($"{LogTag} NetworkManager is already listening. Startup skipped.");
            return;
        }

        switch (options.Mode)
        {
            case StartupMode.Server:
                StartServer(transport, options.Port);
                break;
            case StartupMode.Client:
                StartClient(transport, options.Ip, options.Port);
                break;
            case StartupMode.Host:
                StartHost(transport, options.Ip, options.Port);
                break;
            default:
                Debug.Log($"{LogTag} No network mode selected.");
                break;
        }
    }

    private static void StartServer(UnityTransport transport, ushort port)
    {
        transport.SetConnectionData(TomatoMultiplayerConstants.DefaultServerAddress, port, TomatoMultiplayerConstants.DefaultServerListenAddress);
        bool started = NetworkManager.Singleton.StartServer();
        Debug.Log($"{LogTag} StartServer requested. PublicAddress={TomatoMultiplayerConstants.DefaultServerAddress} ListenAddress={TomatoMultiplayerConstants.DefaultServerListenAddress} Port={port} Started={started}");
    }

    private static void StartClient(UnityTransport transport, string ip, ushort port)
    {
        transport.SetConnectionData(ip, port);
        bool started = NetworkManager.Singleton.StartClient();
        Debug.Log($"{LogTag} StartClient requested. ServerIp={ip} Port={port} Started={started}");
    }

    private static void StartHost(UnityTransport transport, string ip, ushort port)
    {
        transport.SetConnectionData(ip, port, "0.0.0.0");
        bool started = NetworkManager.Singleton.StartHost();
        Debug.Log($"{LogTag} StartHost requested. Ip={ip} ListenAddress=0.0.0.0 Port={port} Started={started}");
    }
}
