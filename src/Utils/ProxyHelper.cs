using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Versioning;

#if DEBUG
using System.Diagnostics;
#endif

namespace BlackDuckReport.GitHubAction.Utils;

/// <summary>
/// Fiddler Utility class
/// </summary>
public static class ProxyHelper
{
    private const int FiddlerDefaultTcpPort = 8888;
    private static readonly TimeSpan FiddlerDetectionTimeout = TimeSpan.FromMilliseconds(50);

    public static int FiddlerTcpPort { get; }
    public static IPAddress FiddlerIpAddress { get; private set; }
    public static bool FiddlerRunning { get; }

    static ProxyHelper()
    {
        FiddlerTcpPort = FiddlerDefaultTcpPort;
        FiddlerIpAddress = IPAddress.Loopback;
#if DEBUG
        FiddlerRunning = DetectFiddler();
#endif
    }

    /// <summary>
    /// Detect if Fiddler is running
    /// </summary>
    /// <returns>true if fiddler is running, false otherwise</returns>
    private static bool DetectFiddler()
    {
        var connected = TryConnectToFiddler();
        if (!connected && OperatingSystem.IsLinux())
        {
            // Try WSL2 Gateway
            var gatewayAddress = GetFiddlerIpAddress();

            if (gatewayAddress != IPAddress.Loopback)
            {
                FiddlerIpAddress = gatewayAddress;
                connected = TryConnectToFiddler();
            }
        }
        return connected;
    }

    /// <summary>
    /// Try to connect to fiddler
    /// </summary>
    /// <returns></returns>
    private static bool TryConnectToFiddler()
    {
        try
        {
#if DEBUG
            Debug.WriteLine($"Trying to connect to Fiddler at {FiddlerIpAddress}:{FiddlerTcpPort} ...");
#endif
            using var client = new TcpClient();
            client.SendTimeout = (int)FiddlerDetectionTimeout.TotalMilliseconds;
            client.ReceiveTimeout = (int)FiddlerDetectionTimeout.TotalMilliseconds;
            client.Connect(FiddlerIpAddress, FiddlerTcpPort);

#if DEBUG
            Debug.WriteLine($"Connected to Fiddler at {FiddlerIpAddress}:{FiddlerTcpPort}");
#endif

            return true;
        }
        catch (SocketException)
        {
#if DEBUG
            Debug.WriteLine($"Could not detect Fiddler at {FiddlerIpAddress}:{FiddlerTcpPort}");
#endif

            return false;
        }
    }

    /// <summary>
    /// Retrieve the WSL2 host IPAddress to access Fiddler running on Windows Host.
    /// 
    /// Note: In order to being able to connect to Fiddler from WSL2, there are requirements: 
    ///     * Requires to authorize Remote Connections from Fiddler Options.
    ///     * Requires .wlsConf specific settings:
    ///     
    /// # https://learn.microsoft.com/en-us/windows/wsl/wsl-config
    /// 
    /// [wsl2]
    /// networkingMode=mirrored
    /// firewall = false
    /// 
    /// [experimental]
    /// useWindowsDnsCache=true
    /// hostAddressLoopback=true
    /// </summary>
    /// <returns></returns>
    [SupportedOSPlatform("Linux")]
    private static IPAddress GetFiddlerIpAddress()
    {
        IPAddress? ipAddress = null;

        try
        {
            ipAddress = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties().GatewayAddresses)
                .Select(g => g.Address)
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                .FirstOrDefault();
        }
        catch (NetworkInformationException)
        {
        }

        return ipAddress ?? IPAddress.Loopback;
    }
}
