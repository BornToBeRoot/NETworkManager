using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NETworkManager.Models.Lookup;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class to scan for IP addresses in a network.
/// </summary>
/// <param name="options">The scan options.</param>
public sealed class IPScanner(IPScannerOptions options)
{
    #region Variables

    /// <summary>
    ///     Time between the start of two hosts during the ramp-up, see <see cref="StartScheduler" />. With the
    ///     default settings the first 128 hosts are started within ~1.3 s instead of at the same moment (~11 connection
    ///     requests per 10 ms instead of ~1400 at once), the following ones when a slot is free. Shorter if a host
    ///     takes less than 128 x 10 ms (see <see cref="StartScheduler.GetInterval" />).
    /// </summary>
    private static readonly TimeSpan MaxHostStartInterval = TimeSpan.FromMilliseconds(10);

    private int _progressValue;
    private readonly InterlockedCounter _hostsUp = new();
    private readonly InterlockedCounter _hostsDown = new();

    // Snapshot of the neighbor table (IP address -> MAC address), see GetMACAddressFromNeighborTableAsync
    private readonly SemaphoreSlim _neighborTableSemaphore = new(1, 1);
    private Dictionary<IPAddress, string> _neighborTable = [];
    private DateTime _neighborTableTimestamp = DateTime.MinValue;

    /// <summary>
    ///     Gets the number of hosts found to be reachable so far. Thread-safe; may be read from
    ///     any thread while the scan is running.
    /// </summary>
    public int HostsUp => _hostsUp.Value;

    /// <summary>
    ///     Gets the number of hosts found to be unreachable so far. Thread-safe; may be read from
    ///     any thread while the scan is running.
    /// </summary>
    public int HostsDown => _hostsDown.Value;

    #endregion

    #region Events

    /// <summary>
    ///     Occurs when a host has been scanned.
    /// </summary>
    public event EventHandler<IPScannerHostScannedArgs> HostScanned;

    private void OnHostScanned(IPScannerHostScannedArgs e)
    {
        HostScanned?.Invoke(this, e);
    }

    /// <summary>
    ///     Occurs when the scan is complete. Raised exactly once per scan, also when the scan was canceled
    ///     or failed (see <see cref="ScanCompleteArgs" />).
    /// </summary>
    public event EventHandler<ScanCompleteArgs> ScanComplete;

    private void OnScanComplete(ScanCompleteArgs e)
    {
        ScanComplete?.Invoke(this, e);
    }

    /// <summary>
    ///     Occurs when the scan progress has changed.
    /// </summary>
    public event EventHandler<ProgressChangedArgs> ProgressChanged;

    private void OnProgressChanged()
    {
        ProgressChanged?.Invoke(this, new ProgressChangedArgs(_progressValue));
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Starts the IP scan asynchronously.
    /// </summary>
    /// <param name="hosts">The list of hosts to scan.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public void ScanAsync(IEnumerable<(IPAddress ipAddress, string hostname)> hosts,
        CancellationToken cancellationToken)
    {
        // Start the scan in a separate task. The token is not passed to Task.Run on purpose: with an
        // already canceled token the delegate would never run and ScanComplete would never be raised.
        Task.Run(async () =>
        {
            _progressValue = 0;

            var isCanceled = false;
            Exception exception = null;

            try
            {
                // Get all network interfaces (for local mac address lookup)
                var networkInterfaces = options.ResolveMACAddress ? NetworkInterface.GetNetworkInterfaces() : [];

                var hostParallelOptions = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = options.MaxHostThreads
                };

                var portScanParallelOptions = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = options.MaxPortThreads
                };

                // Spread the start of the hosts over time instead of starting all at once - based on the time
                // an unreachable host takes (the slowest of ping attempts, port scan and NetBIOS)
                var hostDuration = Math.Max(options.ICMPAttempts * options.ICMPTimeout,
                    Math.Max(
                        options.PortScanEnabled
                            ? options.PortScanTimeout * options.PortScanAttempts *
                              (int)Math.Ceiling(options.PortScanPorts.Length / (double)options.MaxPortThreads)
                            : 0,
                        options.NetBIOSEnabled ? options.NetBIOSTimeout : 0));

                var hostConcurrency = hosts.TryGetNonEnumeratedCount(out var hostCount)
                    ? Math.Min(hostCount, options.MaxHostThreads)
                    : options.MaxHostThreads;

                var hostStartScheduler = new StartScheduler(StartScheduler.GetInterval(MaxHostStartInterval,
                    TimeSpan.FromMilliseconds(hostDuration), hostConcurrency), hostConcurrency);

                // Start scan
                await Parallel.ForEachAsync(hosts, hostParallelOptions, async (host, ct) =>
                {
                    // Spread the start of the hosts over time instead of starting all at once
                    await hostStartScheduler.WaitAsync(ct).ConfigureAwait(false);

                    var discovery = await DiscoverAsync(host.ipAddress, portScanParallelOptions, ct)
                        .ConfigureAwait(false);

                    // Cancel if the user has canceled
                    ct.ThrowIfCancellationRequested();

                    // Count reachable/unreachable hosts unconditionally, since ShowAllResults
                    // (below) may prevent unreachable hosts from ever reaching HostScanned
                    if (discovery.IsReachable)
                        _hostsUp.Increment();
                    else
                        _hostsDown.Increment();

                    if (discovery.IsReachable || options.ShowAllResults)
                        OnHostScanned(new IPScannerHostScannedArgs(
                            await EnrichAsync(host.ipAddress, discovery, networkInterfaces, ct)
                                .ConfigureAwait(false)));

                    IncreaseProgress();
                }).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                isCanceled = true;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                OnScanComplete(new ScanCompleteArgs(isCanceled, exception));
            }
        });
    }

    /// <summary>
    ///     Result of <see cref="DiscoverAsync" />: the probes that decide if a host is reachable. Immutable, the
    ///     derived values are computed once.
    /// </summary>
    /// <param name="pingInfo">Result of the ping (last attempt).</param>
    /// <param name="ports">All probed ports (also closed / timed out ones).</param>
    /// <param name="netBIOSInfo">Result of the NetBIOS request.</param>
    /// <param name="discoveredAt">Time all probes were finished (UTC).</param>
    private sealed class HostDiscovery(
        PingInfo pingInfo,
        List<PortInfo> ports,
        NetBIOSInfo netBIOSInfo,
        DateTime discoveredAt)
    {
        /// <summary>
        ///     Result of the ping (last attempt).
        /// </summary>
        public PingInfo PingInfo { get; } = pingInfo;

        /// <summary>
        ///     All probed ports (also closed / timed out ones).
        /// </summary>
        public List<PortInfo> Ports { get; } = ports;

        /// <summary>
        ///     Result of the NetBIOS request.
        /// </summary>
        public NetBIOSInfo NetBIOSInfo { get; } = netBIOSInfo;

        /// <summary>
        ///     Time all probes were finished (UTC).
        /// </summary>
        public DateTime DiscoveredAt { get; } = discoveredAt;

        /// <summary>
        ///     Indicates whether any port is open.
        /// </summary>
        public bool IsAnyPortOpen { get; } = ports.Any(x => x.State == PortState.Open);

        /// <summary>
        ///     Indicates whether the host answered any probe.
        /// </summary>
        public bool IsReachable { get; } = pingInfo.Status == IPStatus.Success || // ICMP response
                                           // Any port is open or the connection was refused (TCP reset)
                                           ports.Any(x => x.State is PortState.Open or PortState.Closed) ||
                                           netBIOSInfo.IsReachable; // NetBIOS response
    }

    /// <summary>
    ///     Discovers a host: ping, port scan and NetBIOS request run concurrently (none of these block a thread).
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="portScanParallelOptions">Options for the concurrent port probes of the host.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Results of all probes.</returns>
    private async Task<HostDiscovery> DiscoverAsync(IPAddress ipAddress, ParallelOptions portScanParallelOptions,
        CancellationToken cancellationToken)
    {
        var pingTask = PingAsync(ipAddress, cancellationToken);

        var portScanTask = options.PortScanEnabled
            ? PortScanAsync(ipAddress, portScanParallelOptions, cancellationToken)
            : Task.FromResult(new List<PortInfo>());

        var netbiosTask = options.NetBIOSEnabled
            ? NetBIOSResolver.ResolveAsync(ipAddress, options.NetBIOSTimeout, cancellationToken)
            : Task.FromResult(new NetBIOSInfo(ipAddress));

        await Task.WhenAll(pingTask, portScanTask, netbiosTask).ConfigureAwait(false);

        // A reachable host is in the neighbor table (ARP/NDP) from now on (before it can answer, the
        // local system has to resolve its MAC address)
        return new HostDiscovery(pingTask.Result, portScanTask.Result, netbiosTask.Result, DateTime.UtcNow);
    }

    /// <summary>
    ///     Enriches a discovered host with the DNS hostname (PTR), MAC address and vendor and creates the result.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="discovery">Result of <see cref="DiscoverAsync" />.</param>
    /// <param name="networkInterfaces">Local network interfaces (for the MAC address lookup).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Result of the host.</returns>
    private async Task<IPScannerHostInfo> EnrichAsync(IPAddress ipAddress, HostDiscovery discovery,
        List<NetworkInterfaceInfo> networkInterfaces, CancellationToken cancellationToken)
    {
        // DNS and MAC address lookup don't depend on each other - run them concurrently, the host is finished
        // after the slower one instead of after both
        var dnsTask = options.ResolveHostname
            ? DNSClient.GetInstance().ResolvePtrOrEmptyAsync(ipAddress, cancellationToken)
            : Task.FromResult(string.Empty);

        var macAddressTask = options.ResolveMACAddress
            ? ResolveMACAddressAsync(ipAddress, discovery.IsReachable ? discovery.DiscoveredAt : DateTime.MinValue,
                networkInterfaces, cancellationToken)
            : Task.FromResult((macAddress: string.Empty, vendor: string.Empty));

        await Task.WhenAll(dnsTask, macAddressTask).ConfigureAwait(false);

        var dnsHostname = dnsTask.Result;
        var (arpMACAddress, arpVendor) = macAddressTask.Result;

        // Never null (an empty NetBIOSInfo if NetBIOS is disabled or there was no answer), its values may be
        var netBIOSInfo = discovery.NetBIOSInfo;

        return new IPScannerHostInfo(
            discovery.IsReachable,
            discovery.PingInfo,
            // DNS is default, fallback to netbios
            !string.IsNullOrEmpty(dnsHostname)
                ? dnsHostname
                : netBIOSInfo.ComputerName ?? string.Empty,
            dnsHostname,
            discovery.IsAnyPortOpen,
            discovery.Ports
                .Where(x => x.State == PortState.Open || options.ShowAllResults)
                .OrderBy(x => x.Port).ToList(),
            netBIOSInfo,
            // ARP/NDP is preferred, fallback to NetBIOS
            !string.IsNullOrEmpty(arpMACAddress)
                ? arpMACAddress
                : netBIOSInfo.MACAddress ?? string.Empty,
            !string.IsNullOrEmpty(arpMACAddress)
                ? arpVendor
                : netBIOSInfo.Vendor ?? string.Empty
        );
    }

    /// <summary>
    ///     Resolves the MAC address of a host from the neighbor table (ARP/NDP) or the local network interfaces,
    ///     and its vendor.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="discoveredAt">Time the host answered, <see cref="DateTime.MinValue" /> if it is unreachable.</param>
    /// <param name="networkInterfaces">Local network interfaces.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>MAC address (default format) and vendor, empty strings if not found.</returns>
    private async Task<(string macAddress, string vendor)> ResolveMACAddressAsync(IPAddress ipAddress,
        DateTime discoveredAt, List<NetworkInterfaceInfo> networkInterfaces, CancellationToken cancellationToken)
    {
        var macAddress = string.Empty;

        // Get info from neighbor table. Only hosts in a local subnet are in it (for any other
        // host it contains the router), an unreachable host never needs a new snapshot.
        if (IsInLocalSubnet(ipAddress, networkInterfaces))
            macAddress = await GetMACAddressFromNeighborTableAsync(ipAddress, discoveredAt, cancellationToken)
                .ConfigureAwait(false);

        // Check if it is the local mac
        if (string.IsNullOrEmpty(macAddress))
        {
            var ipAddressWithoutScopeId = IPAddressHelper.WithoutScopeId(ipAddress);

            var networkInterfaceInfo = networkInterfaces.FirstOrDefault(p =>
                p.IPv4Address.Any(x => x.Item1.Equals(ipAddressWithoutScopeId)) ||
                p.IPv6Address.Concat(p.IPv6AddressLinkLocal)
                    .Any(x => IPAddressHelper.WithoutScopeId(x).Equals(ipAddressWithoutScopeId)));

            if (networkInterfaceInfo != null)
                macAddress = networkInterfaceInfo.PhysicalAddress.ToString();
        }

        if (string.IsNullOrEmpty(macAddress))
            return (string.Empty, string.Empty);

        // Vendor lookup & default format
        var vendor = OUILookup.LookupByMacAddress(macAddress).FirstOrDefault()?.Vendor ?? string.Empty;

        return (MACAddressHelper.GetDefaultFormat(macAddress), vendor);
    }

    /// <summary>
    ///     Gets the MAC address of <paramref name="ipAddress" /> from the neighbor table (ARP / NDP). The table is
    ///     read once and cached for the scan instead of reading it for every host. It is only read again on a miss
    ///     if the cached snapshot is older than <paramref name="discoveredAt" /> (the time the host answered, when
    ///     its entry was already created) - a host is always looked up in a table read after its answer, and hosts
    ///     that finish at the same time share one read.
    /// </summary>
    /// <param name="ipAddress">IP address of the host (in a local subnet, see <see cref="IsInLocalSubnet" />).</param>
    /// <param name="discoveredAt">Time the host answered, <see cref="DateTime.MinValue" /> to use the cache only.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>MAC address or an empty string.</returns>
    private async Task<string> GetMACAddressFromNeighborTableAsync(IPAddress ipAddress, DateTime discoveredAt,
        CancellationToken cancellationToken)
    {
        var key = IPAddressHelper.WithoutScopeId(ipAddress);

        // Hosts wait without blocking a thread while another host reads the table
        await _neighborTableSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_neighborTable.TryGetValue(key, out var macAddress))
                return macAddress;

            // Already read after the host answered -> no entry
            if (_neighborTableTimestamp > discoveredAt)
                return string.Empty;

            // Timestamp before reading the table, so entries created while reading are covered by the next read
            _neighborTableTimestamp = DateTime.UtcNow;
            _neighborTable = NeighborTable.GetMACAddressMap();

            return _neighborTable.GetValueOrDefault(key, string.Empty);
        }
        finally
        {
            _neighborTableSemaphore.Release();
        }
    }

    /// <summary>
    ///     Checks if <paramref name="ipAddress" /> is in a subnet of a local network interface (on-link). Only these
    ///     hosts are in the neighbor table - for a host behind a router, its MAC address can't be resolved via ARP/NDP
    ///     (the table contains the router), so there is no need to look it up (and to read the table again).
    ///     IPv6: link-local addresses, other addresses if they are in the same /64 as a local address (the prefix
    ///     length is not known, local IPv6 subnets are /64).
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="networkInterfaces">Local network interfaces.</param>
    /// <returns>True if the IP address is in a local subnet.</returns>
    private static bool IsInLocalSubnet(IPAddress ipAddress, IEnumerable<NetworkInterfaceInfo> networkInterfaces)
    {
        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            var address = IPv4Address.ToUInt32(ipAddress);

            return networkInterfaces.Any(networkInterface => networkInterface.IPv4Address.Any(x =>
            {
                if (x.Item2 == null)
                    return false;

                var subnetmask = IPv4Address.ToUInt32(x.Item2);

                return (address & subnetmask) == (IPv4Address.ToUInt32(x.Item1) & subnetmask);
            }));
        }

        if (ipAddress.IsIPv6LinkLocal)
            return true;

        var prefix = ipAddress.GetAddressBytes().AsSpan(0, 8).ToArray();

        return networkInterfaces.Any(networkInterface => networkInterface.IPv6Address.Any(x =>
            x.GetAddressBytes().AsSpan(0, 8).SequenceEqual(prefix)));
    }

    private async Task<PingInfo> PingAsync(IPAddress ipAddress, CancellationToken cancellationToken)
    {
        using var ping = new System.Net.NetworkInformation.Ping();

        PingInfo pingInfo = null;

        for (var i = 0; i < options.ICMPAttempts; i++)
        {
            // Get timestamp
            var timestamp = DateTime.Now;

            try
            {
                // Note: the CancellationToken-accepting overload requires a TimeSpan timeout,
                // unlike the legacy int-based overloads used elsewhere in .NET's Ping API.
                var pingReply = await ping.SendPingAsync(ipAddress, TimeSpan.FromMilliseconds(options.ICMPTimeout),
                    options.ICMPBuffer, cancellationToken: cancellationToken).ConfigureAwait(false);

                // Success
                if (pingReply is { Status: IPStatus.Success })
                {
                    switch (ipAddress.AddressFamily)
                    {
                        case AddressFamily.InterNetwork:
                            return new PingInfo(
                                                         timestamp,
                                                         pingReply.Address,
                                                         pingReply.Buffer.Length,
                                                         pingReply.RoundtripTime,
                                                         pingReply.Options!.Ttl,
                                                         pingReply.Status);
                        case AddressFamily.InterNetworkV6:
                            return new PingInfo(
                                                     timestamp,
                                                     pingReply.Address,
                                                     pingReply.Buffer.Length,
                                                     pingReply.RoundtripTime,
                                                     pingReply.Status);
                    }
                }

                // Failed
                pingInfo = new PingInfo(timestamp, ipAddress, pingReply.Status);

                // Only try again if the request timed out (e.g. a lost packet). Other states like
                // "destination host unreachable" are a definitive answer.
                if (pingReply.Status != IPStatus.TimedOut)
                    break;
            }
            catch (PingException)
            {
                // Ping failed with unknown status
                return new PingInfo(timestamp, ipAddress, IPStatus.Unknown);
            }
        }

        // Result of the last attempt (falls back to unknown status if there was no attempt)
        return pingInfo ?? new PingInfo(DateTime.Now, ipAddress, IPStatus.Unknown);
    }

    /// <summary>
    ///     Probes all configured ports of a host. Returns every port (also closed / timed out ones), because
    ///     a closed port is used to detect a reachable host as well - the caller filters what is shown.
    /// </summary>
    private async Task<List<PortInfo>> PortScanAsync(IPAddress ipAddress, ParallelOptions parallelOptions,
        CancellationToken cancellationToken)
    {
        ConcurrentBag<PortInfo> results = [];

        await Parallel.ForEachAsync(options.PortScanPorts, parallelOptions, async (port, ct) =>
        {
            var portState = await PortProbe.ProbeAsync(ipAddress, port, options.PortScanTimeout,
                    options.PortScanAttempts, ct)
                .ConfigureAwait(false);

            results.Add(new PortInfo(port, PortLookup.LookupByPortAndProtocol(port), portState));
        }).ConfigureAwait(false);

        return results.ToList();
    }

    private void IncreaseProgress()
    {
        // Increase the progress                        
        Interlocked.Increment(ref _progressValue);
        OnProgressChanged();
    }

    #endregion
}