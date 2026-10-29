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

    // Data sent with the ping, created once (see IPScannerOptions.ICMPBufferSize)
    private readonly byte[] _icmpBuffer = new byte[options.ICMPBufferSize];

    private int _progressValue;
    private readonly InterlockedCounter _hostsUp = new();
    private readonly InterlockedCounter _hostsDown = new();

    // Snapshot of the neighbor table (IP address -> MAC address), see GetMACAddressFromNeighborTableAsync
    private readonly SemaphoreSlim _neighborTableSemaphore = new(1, 1);
    private Dictionary<IPAddress, string> _neighborTable = [];
    private DateTime _neighborTableTimestamp = DateTime.MinValue;

    /// <summary>
    ///     Gets the number of hosts finished so far (all probes and lookups). Thread-safe; may be read from
    ///     any thread while the scan is running.
    /// </summary>
    public int HostsScanned => Volatile.Read(ref _progressValue);

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
    ///     Occurs when a host has been found (reachable, or unreachable if unreachable hosts are shown). Further
    ///     results of the host arrive via <see cref="HostUpdated" />.
    /// </summary>
    public event EventHandler<IPScannerHostFoundArgs> HostFound;

    private void OnHostFound(IPScannerHostFoundArgs e)
    {
        HostFound?.Invoke(this, e);
    }

    /// <summary>
    ///     Occurs when a result of a found host arrived (e.g. DNS hostname, MAC address). Never raised for a host
    ///     before its <see cref="HostFound" /> and never after <see cref="ScanComplete" />.
    /// </summary>
    public event EventHandler<IPScannerHostUpdatedArgs> HostUpdated;

    private void OnHostUpdated(IPScannerHostUpdatedArgs e)
    {
        HostUpdated?.Invoke(this, e);
    }

    /// <summary>
    ///     Occurs when the scan is complete. Raised exactly once per scan, also when the scan was canceled
    ///     or failed (see <see cref="ScanCompleteArgs" />) - only after every host is finished, including all
    ///     lookups (no <see cref="HostFound" /> / <see cref="HostUpdated" /> afterwards).
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
                // an unreachable host takes (the slowest of ping attempts, port scan and NetBIOS). In long, the port
                // scan can exceed int (e.g. 15000 ms x 10 attempts x 65535 ports with one port thread)
                var hostDuration = Math.Max((long)options.ICMPAttempts * options.ICMPTimeout,
                    Math.Max(
                        options.PortScanEnabled
                            ? (long)options.PortScanTimeout * options.PortScanAttempts *
                              (long)Math.Ceiling(options.PortScanPorts.Length / (double)options.MaxPortThreads)
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

                    // Returns only when every probe and lookup of the host is finished
                    await ScanHostAsync(host.ipAddress, portScanParallelOptions, networkInterfaces, ct)
                        .ConfigureAwait(false);

                    IncreaseProgress();
                }).ConfigureAwait(false);

                // Parallel.ForEachAsync completes (also on cancellation or an exception) only after every host body
                // has returned, so nothing is still running when ScanComplete is raised below
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
    ///     Scans a host: ping, port scan and NetBIOS request run concurrently (none of these block a thread). The host
    ///     is raised in <see cref="HostFound" /> as soon as it answers the first probe (ping reply, open or refused
    ///     port, NetBIOS answer) with placeholders for the probes that are still running - their results follow as
    ///     <see cref="HostUpdated" />, and the DNS hostname and MAC address are resolved right away
    ///     (<see cref="EnrichAsync" />). An unreachable host is raised when all probes are finished, if unreachable
    ///     hosts are shown. Returns only when every probe and lookup of the host is finished, also on cancellation or
    ///     an error.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="portScanParallelOptions">Options for the concurrent port probes of the host.</param>
    /// <param name="networkInterfaces">Local network interfaces (for the MAC address lookup).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private async Task ScanHostAsync(IPAddress ipAddress, ParallelOptions portScanParallelOptions,
        List<NetworkInterfaceInfo> networkInterfaces, CancellationToken cancellationToken)
    {
        // Completed by the port scan when the first port answers (open or refused), so the host is found before
        // all ports are finished
        var portAnswered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var pingTask = PingProbe.PingAsync(ipAddress, options.ICMPTimeout, options.ICMPAttempts,
            _icmpBuffer, cancellationToken);

        var portScanTask = options.PortScanEnabled
            ? PortScanAsync(ipAddress, portScanParallelOptions, () => portAnswered.TrySetResult(), cancellationToken)
            : Task.FromResult(new List<PortInfo>());

        // NetBIOS only exists over IPv4 - an IPv6 host would never answer and wait for the timeout
        var netBIOSTask = options.NetBIOSEnabled && ipAddress.AddressFamily == AddressFamily.InterNetwork
            ? NetBIOSResolver.ResolveAsync(ipAddress, options.NetBIOSTimeout, cancellationToken)
            : Task.FromResult(new NetBIOSInfo(ipAddress));

        var enrichTask = Task.CompletedTask;

        // Results of the finished probes, null while a probe is running
        PingInfo pingInfo = null;
        List<PortInfo> ports = null;
        NetBIOSInfo netBIOSInfo = null;

        IPScannerHostInfo hostInfo = null;

        try
        {
            List<Task> pending = [pingTask, portScanTask, netBIOSTask, portAnswered.Task];

            while (pending.Count > 0)
            {
                var completed = await Task.WhenAny(pending).ConfigureAwait(false);
                pending.Remove(completed);

                // Rethrow the cancellation / error of the probe
                await completed.ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                bool isAnswer;
                Action<IPScannerHostInfo> update = null;

                if (completed == pingTask)
                {
                    var value = pingInfo = pingTask.Result;

                    isAnswer = value.Status == IPStatus.Success; // ICMP response
                    update = x =>
                    {
                        x.PingInfo = value;
                        x.IsPingPending = false;
                    };
                }
                else if (completed == portScanTask)
                {
                    ports = portScanTask.Result;

                    // If no port answered, the signal never completes
                    pending.Remove(portAnswered.Task);

                    // Any port is open or the connection was refused (TCP reset)
                    isAnswer = ports.Any(x => x.State is PortState.Open or PortState.Closed);

                    var shownPorts = GetShownPorts(ports);
                    var isAnyPortOpen = ports.Any(x => x.State == PortState.Open);

                    update = x =>
                    {
                        x.Ports = shownPorts;
                        x.IsAnyPortOpen = isAnyPortOpen;
                        x.IsPortScanPending = false;
                    };
                }
                else if (completed == netBIOSTask)
                {
                    var value = netBIOSInfo = netBIOSTask.Result;

                    isAnswer = value.IsReachable; // NetBIOS response
                    update = x =>
                    {
                        x.NetBIOSInfo = value;
                        x.IsNetBIOSPending = false;
                    };
                }
                else
                {
                    // A port answered - its result follows with the port scan
                    isAnswer = true;
                }

                if (hostInfo != null)
                {
                    if (update != null)
                        OnHostUpdated(new IPScannerHostUpdatedArgs(hostInfo, update));

                    continue;
                }

                if (!isAnswer)
                    continue;

                // First answer: the host is up. Before it could answer, the local system had to resolve its MAC
                // address, so it is in the neighbor table (ARP/NDP) from now on.
                hostInfo = CreateHostInfo(ipAddress, true, pingInfo, ports, netBIOSInfo);

                _hostsUp.Increment();

                OnHostFound(new IPScannerHostFoundArgs(hostInfo));
                OnProgressChanged();

                enrichTask = EnrichAsync(ipAddress, hostInfo, DateTime.UtcNow, networkInterfaces, cancellationToken);
            }

            if (hostInfo == null)
            {
                // Count unreachable hosts unconditionally, since ShowAllResults may prevent them from ever
                // reaching HostFound
                _hostsDown.Increment();

                if (options.ShowAllResults || options.ReportAllHosts)
                {
                    hostInfo = CreateHostInfo(ipAddress, false, pingInfo, ports, netBIOSInfo);

                    OnHostFound(new IPScannerHostFoundArgs(hostInfo));

                    // An unreachable host doesn't need a new snapshot of the neighbor table - the table is only read
                    // if it was not read yet (e.g. a host that blocks ICMP still has an ARP entry from the ping)
                    enrichTask = EnrichAsync(ipAddress, hostInfo, DateTime.MinValue, networkInterfaces,
                        cancellationToken);
                }
            }

            await enrichTask.ConfigureAwait(false);
        }
        finally
        {
            // Never return while a probe or lookup is still running (e.g. another probe was canceled or failed)
            await WaitIgnoringErrorsAsync(pingTask, portScanTask, netBIOSTask, enrichTask).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Waits until all tasks are finished. Their errors are ignored - the caller already handles (or rethrows)
    ///     the first one.
    /// </summary>
    /// <param name="tasks">Tasks to wait for.</param>
    private static async Task WaitIgnoringErrorsAsync(params Task[] tasks)
    {
        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Handled by the caller (see summary)
        }
    }

    /// <summary>
    ///     Gets the ports shown in the result: open ports, all ports if unreachable ones are shown.
    /// </summary>
    /// <param name="ports">All probed ports.</param>
    /// <returns>Ports to show, sorted by port.</returns>
    private List<PortInfo> GetShownPorts(List<PortInfo> ports)
    {
        return ports
            .Where(x => x.State == PortState.Open || options.ShowAllResults)
            .OrderBy(x => x.Port).ToList();
    }

    /// <summary>
    ///     Creates the result of a host with the results of the finished probes and placeholders for the running ones
    ///     (marked as pending).
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="isReachable">Indicates whether the host answered a probe.</param>
    /// <param name="pingInfo">Result of the ping, null if it is still running.</param>
    /// <param name="ports">All probed ports, null if the port scan is still running.</param>
    /// <param name="netBIOSInfo">Result of the NetBIOS request, null if it is still running.</param>
    /// <returns>Result of the host.</returns>
    private IPScannerHostInfo CreateHostInfo(IPAddress ipAddress, bool isReachable, PingInfo pingInfo,
        List<PortInfo> ports, NetBIOSInfo netBIOSInfo)
    {
        return new IPScannerHostInfo(
            ipAddress,
            isReachable,
            pingInfo ?? new PingInfo(DateTime.Now, ipAddress, IPStatus.Unknown),
            ports?.Any(x => x.State == PortState.Open) ?? false,
            ports != null ? GetShownPorts(ports) : [],
            netBIOSInfo ?? new NetBIOSInfo(ipAddress))
        {
            IsPingPending = pingInfo == null,
            IsPortScanPending = ports == null,
            IsNetBIOSPending = netBIOSInfo == null
        };
    }

    /// <summary>
    ///     Enriches a found host with the DNS hostname (PTR), MAC address and vendor. Each result is raised as
    ///     <see cref="HostUpdated" /> as soon as it arrives; the task completes only when all lookups are finished, so
    ///     the host (and the scan) is never finished while a lookup is still running.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="host">Result of the host, already raised in <see cref="HostFound" />.</param>
    /// <param name="discoveredAt">Time the host answered, <see cref="DateTime.MinValue" /> if it is unreachable.</param>
    /// <param name="networkInterfaces">Local network interfaces (for the MAC address lookup).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private async Task EnrichAsync(IPAddress ipAddress, IPScannerHostInfo host, DateTime discoveredAt,
        List<NetworkInterfaceInfo> networkInterfaces, CancellationToken cancellationToken)
    {
        async Task UpdateHostnameAsync()
        {
            var dnsHostname = await DNSClient.GetInstance().ResolvePtrOrEmptyAsync(ipAddress, cancellationToken)
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(dnsHostname))
                OnHostUpdated(new IPScannerHostUpdatedArgs(host, x => x.DNSHostname = dnsHostname));
        }

        async Task UpdateMACAddressAsync()
        {
            var (macAddress, vendor) = await ResolveMACAddressAsync(ipAddress, discoveredAt, networkInterfaces,
                cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(macAddress))
                OnHostUpdated(new IPScannerHostUpdatedArgs(host, x =>
                {
                    x.NeighborMACAddress = macAddress;
                    x.NeighborVendor = vendor;
                }));
        }

        // DNS and MAC address lookup don't depend on each other - run them concurrently, the host is finished
        // after the slower one instead of after both
        await Task.WhenAll(
            options.ResolveHostname ? UpdateHostnameAsync() : Task.CompletedTask,
            options.ResolveMACAddress ? UpdateMACAddressAsync() : Task.CompletedTask).ConfigureAwait(false);
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
        // host it contains the router), an unreachable host only reads it if it was not read yet.
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

            // Only a MAC address (6 bytes) - e.g. a tunnel adapter has none or a link-layer address of another length
            if (networkInterfaceInfo?.PhysicalAddress?.GetAddressBytes().Length == 6)
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
    /// <param name="discoveredAt">
    ///     Time the host answered, <see cref="DateTime.MinValue" /> to use the cache - or to read the table if it was not
    ///     read yet.
    /// </param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>MAC address or an empty string.</returns>
    private async Task<string> GetMACAddressFromNeighborTableAsync(IPAddress ipAddress, DateTime discoveredAt,
        CancellationToken cancellationToken)
    {
        // Hosts wait without blocking a thread while another host reads the table
        await _neighborTableSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // With the scope id if it was entered (e.g. "fe80::1%3" and "fe80::1%7" are two hosts on different
            // interfaces), see NeighborTable.GetMACAddressMap
            if (_neighborTable.TryGetValue(ipAddress, out var macAddress))
                return macAddress;

            // Already read after the host answered -> no entry
            if (_neighborTableTimestamp > discoveredAt)
                return string.Empty;

            // Timestamp before reading the table, so entries created while reading are covered by the next read
            _neighborTableTimestamp = DateTime.UtcNow;
            _neighborTable = NeighborTable.GetMACAddressMap();

            return _neighborTable.GetValueOrDefault(ipAddress, string.Empty);
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

    /// <summary>
    ///     Probes all configured ports of a host. Returns every port (also closed / timed out ones), because
    ///     a closed port is used to detect a reachable host as well - the caller filters what is shown.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="parallelOptions">Options for the concurrent port probes.</param>
    /// <param name="onAnswer">Called when a port answered (open or refused), may be called multiple times.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private async Task<List<PortInfo>> PortScanAsync(IPAddress ipAddress, ParallelOptions parallelOptions,
        Action onAnswer, CancellationToken cancellationToken)
    {
        ConcurrentBag<PortInfo> results = [];

        await Parallel.ForEachAsync(options.PortScanPorts, parallelOptions, async (port, ct) =>
        {
            var portState = await PortProbe.ProbeAsync(ipAddress, port, options.PortScanTimeout,
                    options.PortScanAttempts, ct)
                .ConfigureAwait(false);

            results.Add(new PortInfo(port, PortLookup.LookupByPortAndProtocol(port), portState));

            if (portState is PortState.Open or PortState.Closed)
                onAnswer();
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