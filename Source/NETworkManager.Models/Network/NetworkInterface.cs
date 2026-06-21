using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using log4net;
using Microsoft.Win32;
using NETworkManager.Utilities;
using SMA = System.Management.Automation;

namespace NETworkManager.Models.Network;

/// <summary>
/// Provides static and instance methods for retrieving information about network interfaces, detecting local IP
/// addresses and gateways, and configuring network interface settings on the local machine.
/// </summary>
/// <remarks>The NetworkInterface class offers both synchronous and asynchronous methods for enumerating network
/// interfaces, detecting routing and gateway information, and performing network configuration tasks such as setting IP
/// addresses, DNS servers, and flushing the DNS cache. Most configuration operations require administrative privileges.
/// Events are provided to notify when user-initiated cancellations occur, such as when a UAC prompt is dismissed. This
/// class is intended for use in applications that need to query or modify network interface settings
/// programmatically.</remarks>
public sealed class NetworkInterface
{
    #region Variables

    /// <summary>
    /// List of network interface name patterns to filter out virtual/filter adapters
    /// introduced in .NET 9/10. These are typically not actual network interfaces but rather
    /// drivers, filters, or extensions attached to real network interfaces.
    /// See: https://github.com/dotnet/runtime/issues/122751
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(NetworkInterface));

    private static readonly List<string> NetworkInterfaceFilteredPatterns =
    [
        "Hyper-V Virtual Switch Extension Filter",
        "Hyper-V Virtual Switch Extension Adapter",
        "WFP Native MAC Layer LightWeight Filter",
        "Npcap Packet Driver (NPCAP)",
        "QoS Packet Scheduler",
        "WFP 802.3 MAC Layer LightWeight Filter",
        "Ethernet (Kerneldebugger)",
        "Filter Driver",
        "WAN Miniport",
        "Microsoft Wi-Fi Direct Virtual Adapter"
    ];

    #endregion

    #region Methods

    /// <summary>
    /// Asynchronously retrieves a list of available network interfaces on the local machine.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
    /// cref="NetworkInterfaceInfo"/> objects describing each detected network interface.</returns>
    public static Task<List<NetworkInterfaceInfo>> GetNetworkInterfacesAsync()
    {
        return Task.Run(GetNetworkInterfaces);
    }

    /// <summary>
    /// Retrieves a list of network interfaces on the local machine, including detailed information about each interface
    /// such as addresses, gateways, and DHCP settings.
    /// </summary>
    /// <remarks>Only Ethernet, Wireless80211, and proprietary virtual/internal interfaces are included. The
    /// returned information includes both IPv4 and IPv6 details, as well as DHCP and DNS configuration where available.
    /// This method may require appropriate permissions to access network configuration data.</remarks>
    /// <returns>A list of <see cref="NetworkInterfaceInfo"/> objects, each representing a network interface with its associated
    /// properties. The list is empty if no matching interfaces are found.</returns>
    public static List<NetworkInterfaceInfo> GetNetworkInterfaces()
    {
        List<NetworkInterfaceInfo> listNetworkInterfaceInfo = [];

        // Query network profiles (Domain/Private/Public) for all connected interfaces via PowerShell.
        // Keyed by InterfaceAlias which matches networkInterface.Name in the .NET API.
        var profileByAlias = new Dictionary<string, NetworkProfile>(StringComparer.OrdinalIgnoreCase);

        try
        {
            using var ps = SMA.PowerShell.Create();
            ps.AddScript("Get-NetConnectionProfile | Select-Object InterfaceAlias, NetworkCategory");

            foreach (var result in ps.Invoke())
            {
                var alias = result.Properties["InterfaceAlias"]?.Value?.ToString();
                var category = result.Properties["NetworkCategory"]?.Value?.ToString();

                if (string.IsNullOrEmpty(alias))
                    continue;

                profileByAlias[alias] = category switch
                {
                    "DomainAuthenticated" => NetworkProfile.Domain,
                    "Private" => NetworkProfile.Private,
                    "Public" => NetworkProfile.Public,
                    _ => NetworkProfile.NotConfigured
                };
            }
        }
        catch (Exception ex)
        {
            Log.Warn("Failed to query network connection profiles via Get-NetConnectionProfile.", ex);
        }

        foreach (var networkInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
        {
            // NetworkInterfaceType 53 is proprietary virtual/internal interface
            // https://docs.microsoft.com/en-us/windows-hardware/drivers/network/ndis-interface-types
            if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
                networkInterface.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                (int)networkInterface.NetworkInterfaceType != 53)
                continue;

            // Filter out virtual/filter adapters introduced in .NET 9/10
            // Check if the adapter name or description contains any filtered pattern
            // See: https://github.com/dotnet/runtime/issues/122751
            if (NetworkInterfaceFilteredPatterns.Any(pattern =>
                networkInterface.Name.Contains(pattern) ||
                networkInterface.Description.Contains(pattern)))
                continue;

            var listIPv4Address = new List<Tuple<IPAddress, IPAddress>>();
            var listIPv6AddressLinkLocal = new List<IPAddress>();
            var listIPv6Address = new List<IPAddress>();

            var dhcpLeaseObtained = new DateTime();
            var dhcpLeaseExpires = new DateTime();

            var ipProperties = networkInterface.GetIPProperties();

            foreach (var unicastIPAddrInfo in ipProperties.UnicastAddresses)
            {
                switch (unicastIPAddrInfo.Address.AddressFamily)
                {
                    case AddressFamily.InterNetwork:

                        listIPv4Address.Add(new Tuple<IPAddress, IPAddress>(unicastIPAddrInfo.Address,
                            unicastIPAddrInfo.IPv4Mask));
                        dhcpLeaseExpires =
                            (DateTime.UtcNow + TimeSpan.FromSeconds(unicastIPAddrInfo.AddressPreferredLifetime))
                            .ToLocalTime();
                        dhcpLeaseObtained =
                            (DateTime.UtcNow + TimeSpan.FromSeconds(unicastIPAddrInfo.AddressValidLifetime) -
                             TimeSpan.FromSeconds(unicastIPAddrInfo.DhcpLeaseLifetime)).ToLocalTime();
                        break;
                    case AddressFamily.InterNetworkV6 when unicastIPAddrInfo.Address.IsIPv6LinkLocal:
                        listIPv6AddressLinkLocal.Add(unicastIPAddrInfo.Address);
                        break;
                    case AddressFamily.InterNetworkV6:
                        listIPv6Address.Add(unicastIPAddrInfo.Address);
                        break;
                }
            }

            var listIPv4Gateway = new List<IPAddress>();
            var listIPv6Gateway = new List<IPAddress>();

            foreach (var gatewayIPAddrInfo in ipProperties.GatewayAddresses)
            {
                switch (gatewayIPAddrInfo.Address.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        listIPv4Gateway.Add(gatewayIPAddrInfo.Address);
                        break;
                    case AddressFamily.InterNetworkV6:
                        listIPv6Gateway.Add(gatewayIPAddrInfo.Address);
                        break;
                }
            }

            // Check if autoconfiguration for DNS is enabled (only possible via registry key)
            var nameServerKey =
                Registry.LocalMachine.OpenSubKey(
                    $@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\{networkInterface.Id}");
            var dnsAutoconfigurationEnabled = nameServerKey?.GetValue("NameServer") != null &&
                                              string.IsNullOrEmpty(nameServerKey.GetValue("NameServer")?.ToString());

            // Check if IPv4 protocol is available
            var ipv4ProtocolAvailable = true;
            IPv4InterfaceProperties ipv4Properties = null;

            try
            {
                ipv4Properties = ipProperties.GetIPv4Properties();
            }
            catch (NetworkInformationException)
            {
                ipv4ProtocolAvailable = false;
            }

            // Check if IPv6 protocol is available
            var ipv6ProtocolAvailable = true;

            try
            {
                ipProperties.GetIPv6Properties();
            }
            catch (NetworkInformationException)
            {
                ipv6ProtocolAvailable = false;
            }

            listNetworkInterfaceInfo.Add(new NetworkInterfaceInfo
            {
                Id = networkInterface.Id,
                Name = networkInterface.Name,
                Description = networkInterface.Description,
                Type = networkInterface.NetworkInterfaceType.ToString(),
                PhysicalAddress = networkInterface.GetPhysicalAddress(),
                Status = networkInterface.OperationalStatus,
                IsOperational = networkInterface.OperationalStatus == OperationalStatus.Up,
                Speed = networkInterface.Speed,
                IPv4ProtocolAvailable = ipv4ProtocolAvailable,
                IPv4Address = [.. listIPv4Address],
                IPv4Gateway = [.. listIPv4Gateway],
                DhcpEnabled = ipv4Properties is { IsDhcpEnabled: true },
                DhcpServer = [.. ipProperties.DhcpServerAddresses.Where(dhcpServerIPAddress =>
                    dhcpServerIPAddress.AddressFamily == AddressFamily.InterNetwork)],
                DhcpLeaseObtained = dhcpLeaseObtained,
                DhcpLeaseExpires = dhcpLeaseExpires,
                IPv6ProtocolAvailable = ipv6ProtocolAvailable,
                IPv6AddressLinkLocal = [.. listIPv6AddressLinkLocal],
                IPv6Address = [.. listIPv6Address],
                IPv6Gateway = [.. listIPv6Gateway],
                DNSAutoconfigurationEnabled = dnsAutoconfigurationEnabled,
                DNSSuffix = ipProperties.DnsSuffix,
                DNSServer = [.. ipProperties.DnsAddresses],
                Profile = profileByAlias.TryGetValue(networkInterface.Name, out var profile)
                    ? profile
                    : NetworkProfile.NotConfigured
            });
        }

        return listNetworkInterfaceInfo;
    }

    /// <summary>
    /// Asynchronously determines the local IP address that would be used to route traffic to the specified remote IP
    /// address.
    /// </summary>
    /// <remarks>This method is useful for identifying the local network interface that would be selected by
    /// the system's routing table when communicating with a given remote address. The result may vary depending on the
    /// current network configuration and routing rules.</remarks>
    /// <param name="remoteIPAddress">The destination IP address for which to determine the corresponding local source IP address. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the local IP address that would be
    /// used to reach the specified remote IP address.</returns>
    public static Task<IPAddress> DetectLocalIPAddressBasedOnRoutingAsync(IPAddress remoteIPAddress)
    {
        return Task.Run(() => DetectLocalIPAddressFromRouting(remoteIPAddress));
    }

    /// <summary>
    /// Determines the local IP address that would be used to route traffic to the specified remote IP address.
    /// </summary>
    /// <remarks>This method creates a UDP socket to determine the local IP address selected by the system's
    /// routing table for the given remote address. No data is sent over the network. This method may return null if the
    /// routing information is unavailable or an error occurs.</remarks>
    /// <param name="remoteIPAddress">The destination IP address for which to determine the local routing address. Must not be null.</param>
    /// <returns>An IPAddress representing the local address that would be used to reach the specified remote address; or null if
    /// the local address cannot be determined.</returns>
    private static IPAddress DetectLocalIPAddressFromRouting(IPAddress remoteIPAddress)
    {
        var isIPv4 = remoteIPAddress.AddressFamily == AddressFamily.InterNetwork;

        using var socket = new Socket(remoteIPAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

        // return null on error...
        try
        {
            socket.Bind(new IPEndPoint(isIPv4 ? IPAddress.Any : IPAddress.IPv6Any, 0));
            socket.Connect(new IPEndPoint(remoteIPAddress, 0));

            if (socket.LocalEndPoint is IPEndPoint ipAddress)
                return ipAddress.Address;
        }
        catch (SocketException) { }

        return null;
    }

    /// <summary>
    /// Asynchronously detects the local IP address associated with a network interface that matches the specified
    /// address family.
    /// </summary>
    /// <param name="addressFamily">The address family to use when searching for a local IP address. Typically, use AddressFamily.InterNetwork for
    /// IPv4 or AddressFamily.InterNetworkV6 for IPv6.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the detected local IP address, or
    /// null if no suitable address is found.</returns>
    public static Task<IPAddress> DetectLocalIPAddressFromNetworkInterfaceAsync(AddressFamily addressFamily)
    {
        return Task.Run(() => DetectLocalIPAddressFromNetworkInterface(addressFamily));
    }

    /// <summary>
    /// Detects and returns the first local IP address assigned to an operational network interface that matches the
    /// specified address family.
    /// </summary>
    /// <remarks>For IPv4, the method prefers non-link-local addresses but will return a link-local address if
    /// no other is available. For IPv6, the method returns the first global or unique local address if present;
    /// otherwise, it returns a link-local address if available. The returned address is selected from operational
    /// network interfaces only.</remarks>
    /// <param name="addressFamily">The address family to search for. Specify <see cref="AddressFamily.InterNetwork"/> for IPv4 addresses or <see
    /// cref="AddressFamily.InterNetworkV6"/> for IPv6 addresses.</param>
    /// <returns>An <see cref="IPAddress"/> representing the first detected local IP address for the specified address family, or
    /// <see langword="null"/> if no suitable address is found.</returns>
    public static IPAddress DetectLocalIPAddressFromNetworkInterface(AddressFamily addressFamily)
    {
        // Filter operational network interfaces
        var networkInterfaces = GetNetworkInterfaces()
            .Where(x => x.IsOperational);

        var candidates = new List<IPAddress>();

        // IPv4
        if (addressFamily == AddressFamily.InterNetwork)
        {
            foreach (var networkInterface in networkInterfaces)
            {
                foreach (var ipAddress in networkInterface.IPv4Address)
                    candidates.Add(ipAddress.Item1);
            }

            // Prefer non-link-local addresses
            var nonLinkLocal = candidates.Where(x =>
            {
                var bytes = x.GetAddressBytes();

                return !(bytes[0] == 169 && bytes[1] == 254);
            });

            // Return first non-link-local or first candidate if none found (might be null - no addresses at all)
            return nonLinkLocal.Any() ? nonLinkLocal.First() : candidates.FirstOrDefault();
        }

        // IPv6
        if (addressFamily == AddressFamily.InterNetworkV6)
        {
            // First try to get global or unique local addresses
            foreach (var networkInterface in networkInterfaces)
                candidates.AddRange(networkInterface.IPv6Address);

            // Return first candidate if any found
            if (candidates.Count != 0)
                return candidates.First();

            // Fallback to link-local addresses
            var firstWithLinkLocal = networkInterfaces
               .FirstOrDefault(ni => ni.IPv6AddressLinkLocal.Length != 0);

            if (firstWithLinkLocal != null)
                return firstWithLinkLocal.IPv6AddressLinkLocal.First();
        }

        return null;
    }

    /// <summary>
    /// Asynchronously detects the default gateway address associated with the specified local IP address.
    /// </summary>
    /// <param name="localIPAddress">The local IP address for which to determine the corresponding default gateway. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the IP address of the detected
    /// default gateway, or null if no gateway is found.</returns>
    public static Task<IPAddress> DetectGatewayFromLocalIPAddressAsync(IPAddress localIPAddress)
    {
        return Task.Run(() => DetectGatewayFromLocalIPAddress(localIPAddress));
    }

    /// <summary>
    /// Attempts to determine the default gateway address associated with the specified local IP address.
    /// </summary>
    /// <remarks>This method searches all available network interfaces to find one that has the specified
    /// local IP address assigned. If found, it returns the first associated gateway address for that interface and
    /// address family. Returns null if the local IP address is not assigned to any interface or if no gateway is
    /// configured.</remarks>
    /// <param name="localIPAddress">The local IP address for which to detect the corresponding gateway. Must be either an IPv4 or IPv6 address.</param>
    /// <returns>An IPAddress representing the default gateway for the specified local IP address, or null if no matching gateway
    /// is found.</returns>
    private static IPAddress DetectGatewayFromLocalIPAddress(IPAddress localIPAddress)
    {
        foreach (var networkInterface in GetNetworkInterfaces())
        {
            // IPv4
            if (localIPAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                if (networkInterface.IPv4Address.Any(x => x.Item1.Equals(localIPAddress)))
                    return networkInterface.IPv4Gateway.FirstOrDefault();
            }

            // IPv6
            if (localIPAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (networkInterface.IPv6Address.Contains(localIPAddress))
                    return networkInterface.IPv6Gateway.FirstOrDefault();
            }
        }

        return null;
    }

    /// <summary>
    /// Asynchronously applies the specified network interface configuration.
    /// </summary>
    /// <param name="config">The configuration settings to apply to the network interface. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task ConfigureNetworkInterfaceAsync(NetworkInterfaceConfig config)
    {
        return Task.Run(() => ConfigureNetworkInterface(config));
    }

    /// <summary>
    /// Configures the network interface according to the specified settings.
    /// </summary>
    /// <remarks>Requires the application to be running with administrator privileges.</remarks>
    /// <param name="config">An object containing the configuration parameters for the network interface, including IP address, subnet mask,
    /// gateway, and DNS server settings. Cannot be null.</param>
    private static void ConfigureNetworkInterface(NetworkInterfaceConfig config)
    {
        var name = PowerShellHelper.EscapeSingleQuotes(config.Name);
        var commands = new List<string>();

        // IP
        commands.Add(config.EnableStaticIPAddress
            ? $"& netsh interface ipv4 set address name='{name}' source=static address={config.IPAddress} mask={config.Subnetmask} gateway={config.Gateway}"
            : $"& netsh interface ipv4 set address name='{name}' source=dhcp");

        // DNS
        if (config.EnableStaticDNS)
        {
            commands.Add($"& netsh interface ipv4 set dnsservers name='{name}' source=static address={config.PrimaryDNSServer} register=primary validate=no");
            if (!string.IsNullOrEmpty(config.SecondaryDNSServer))
                commands.Add($"& netsh interface ipv4 add dnsservers name='{name}' address={config.SecondaryDNSServer} index=2 validate=no");
        }
        else
        {
            commands.Add($"& netsh interface ipv4 set dnsservers name='{name}' source=dhcp");
        }

        RunCommands(commands);
    }

    /// <summary>
    /// Asynchronously flushes the system DNS resolver cache.
    /// </summary>
    /// <remarks>This method initiates the DNS cache flush operation on a background thread. The operation may
    /// require elevated permissions depending on the system configuration.</remarks>
    /// <returns>A task that represents the asynchronous flush operation.</returns>
    public static Task FlushDnsAsync()
    {
        return Task.Run(FlushDns);
    }

    /// <summary>
    /// Clears the local DNS resolver cache on the system.
    /// </summary>
    /// <remarks>Requires the application to be running with administrator privileges.</remarks>
    private static void FlushDns()
    {
        RunCommands(["& ipconfig /flushdns"], checkExitCode: true);
    }

    /// <summary>
    /// Asynchronously releases and renews the IP configuration for the specified network adapter using the given mode.
    /// </summary>
    /// <param name="mode">The release and renew operation mode to apply to the network adapter.</param>
    /// <param name="adapterName">The name of the network adapter whose IP configuration will be released and renewed. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous release and renew operation.</returns>
    public static Task ReleaseRenewAsync(IPConfigReleaseRenewMode mode, string adapterName)
    {
        return Task.Run(() => ReleaseRenew(mode, adapterName));
    }

    /// <summary>
    /// Releases and/or renews the IP configuration for the specified network adapter using the given mode.
    /// </summary>
    /// <remarks>Requires the application to be running with administrator privileges.</remarks>
    /// <param name="mode">A value that specifies which IP configuration operation to perform.</param>
    /// <param name="adapterName">The name of the network adapter to target. Cannot be null or empty.</param>
    private static void ReleaseRenew(IPConfigReleaseRenewMode mode, string adapterName)
    {
        var name = PowerShellHelper.EscapeSingleQuotes(adapterName);
        var lines = new List<string>();

        if (mode is IPConfigReleaseRenewMode.ReleaseRenew or IPConfigReleaseRenewMode.Release)
            lines.Add($"& ipconfig /release '{name}'");

        if (mode is IPConfigReleaseRenewMode.ReleaseRenew or IPConfigReleaseRenewMode.Renew)
            lines.Add($"& ipconfig /renew '{name}'");

        if (mode is IPConfigReleaseRenewMode.ReleaseRenew6 or IPConfigReleaseRenewMode.Release6)
            lines.Add($"& ipconfig /release6 '{name}'");

        if (mode is IPConfigReleaseRenewMode.ReleaseRenew6 or IPConfigReleaseRenewMode.Renew6)
            lines.Add($"& ipconfig /renew6 '{name}'");

        RunCommands(lines, checkExitCode: true);
    }

    /// <summary>
    /// Asynchronously adds an IP address to the specified network interface using the provided configuration.
    /// </summary>
    /// <param name="config">The configuration settings that specify the network interface and IP address to add. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task AddIPAddressToNetworkInterfaceAsync(NetworkInterfaceConfig config)
    {
        return Task.Run(() => AddIPAddressToNetworkInterface(config));
    }

    /// <summary>
    /// Adds an IP address to the specified network interface using the provided configuration.
    /// </summary>
    /// <remarks>Requires the application to be running with administrator privileges.</remarks>
    /// <param name="config">The network interface configuration containing the interface name, IP address, subnet mask, and DHCP/static
    /// coexistence settings. Cannot be null.</param>
    private static void AddIPAddressToNetworkInterface(NetworkInterfaceConfig config)
    {
        var name = PowerShellHelper.EscapeSingleQuotes(config.Name);
        var commands = new List<string>();

        if (config.EnableDhcpStaticIpCoexistence)
            commands.Add($"& netsh interface ipv4 set interface interface='{name}' dhcpstaticipcoexistence=enabled");

        commands.Add($"& netsh interface ipv4 add address '{name}' {config.IPAddress} {config.Subnetmask}");

        RunCommands(commands);
    }

    /// <summary>
    /// Asynchronously removes the IP address specified in the configuration from the associated network interface.
    /// </summary>
    /// <param name="config">The configuration object that specifies the network interface and IP address to remove. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous remove operation.</returns>
    public static Task RemoveIPAddressFromNetworkInterfaceAsync(NetworkInterfaceConfig config)
    {
        return Task.Run(() => RemoveIPAddressFromNetworkInterface(config));
    }

    /// <summary>
    /// Removes the specified IP address from the given network interface configuration.
    /// </summary>
    /// <remarks>Requires the application to be running with administrator privileges.</remarks>
    /// <param name="config">The network interface configuration containing the name of the interface and the IP address to remove. Cannot be
    /// null.</param>
    private static void RemoveIPAddressFromNetworkInterface(NetworkInterfaceConfig config)
    {
        var name = PowerShellHelper.EscapeSingleQuotes(config.Name);
        RunCommands([$"& netsh interface ipv4 delete address '{name}' {config.IPAddress}"]);
    }

    /// <summary>
    /// Runs one or more native commands via a PowerShell runspace.
    /// When <paramref name="checkExitCode"/> is <see langword="true"/>, a non-zero
    /// <c>$LASTEXITCODE</c> is treated as an error via <c>Write-Error</c>.
    /// Use <see langword="false"/> for <c>netsh</c>, whose exit codes are unreliable
    /// for idempotent operations (e.g. returns 1 when already on DHCP).
    /// </summary>
    private static void RunCommands(IEnumerable<string> commands, bool checkExitCode = false)
    {
        string script;

        if (checkExitCode)
            script = string.Join(Environment.NewLine, commands.Select(cmd =>
                $"{cmd}{Environment.NewLine}if ($LASTEXITCODE -ne 0) {{ Write-Error \"Command failed with exit code $LASTEXITCODE\" }}"));
        else
            script = string.Join(Environment.NewLine, commands);

        using var ps = SMA.PowerShell.Create();
        ps.AddScript(script);
        ps.Invoke();

        if (checkExitCode && ps.HadErrors)
            throw new Exception(string.Join("; ", ps.Streams.Error.Select(e => e.ToString())));
    }

    #endregion

}