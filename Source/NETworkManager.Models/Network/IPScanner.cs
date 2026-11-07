using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using log4net;
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

    private static readonly ILog Log = LogManager.GetLogger(typeof(IPScanner));

    // The port scan checks TCP 445 (SMB): the Windows host information is only read for hosts with 445 open
    private readonly bool _isSMBPortScanned =
        options.PortScanEnabled && options.PortScanPorts.Contains(SMBInfoResolver.SMBPort);

    /// <summary>
    ///     Max. number of hosts whose NetApi requests run at the same time, in all scans of the application. The calls
    ///     are blocking and can't be canceled (see <see cref="WindowsNetApiResolver" />): each one has its own thread, a
    ///     call that exceeds the timeout keeps its slot until Windows returns - also after its scan is completed, so
    ///     the slots are shared by all scans (tabs, repeated scans), otherwise blocked calls could pile up.
    /// </summary>
    private const int MaxWindowsThreads = 16;

    private static readonly SemaphoreSlim WindowsSemaphore = new(MaxWindowsThreads, MaxWindowsThreads);

    // Network logon with the Windows credentials of the profile / group (null: the user running the application) and
    // the error if it could not be created (every host gets it instead of using another user)
    private WindowsLogon _windowsLogon;
    private int _windowsLogonError;

    // Checks the credentials on one host before the others use them (account lockout), null without credentials
    private WindowsLogonGate _windowsLogonGate;

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

    // Roles of the hosts for the local computer (own IP addresses, gateways, DNS / DHCP servers), set before the
    // hosts are scanned and only read afterwards
    private Dictionary<IPAddress, IPScannerHostRole> _hostRoles = [];

    // Neighbor table (ARP / NDP) of the scan, created with the roles of the hosts
    private IPScannerNeighborCache _neighborCache;

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
                // Get all network interfaces (for the roles of the hosts, the MAC address lookup and the neighbor
                // check of hosts in a local subnet)
                var networkInterfaces = NetworkInterface.GetNetworkInterfaces();

                _hostRoles = IPScannerHostRoleHelper.GetRoles(networkInterfaces);
                _neighborCache = new IPScannerNeighborCache(_hostRoles);

                // Network logon for the NetApi requests of the Windows host information, once per scan
                if (options.WindowsEnabled && options.WindowsCredentials != null)
                {
                    // Of the tab (all its scans), otherwise for this scan
                    _windowsLogonGate = options.WindowsLogonGate ?? new WindowsLogonGate(options.WindowsCredentials);
                    _windowsLogonGate.StartScan(options.WindowsMaxLogonFailures);

                    try
                    {
                        _windowsLogon = WindowsLogon.Create(options.WindowsCredentials);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn($"Could not log on with the Windows credentials of the profile / group " +
                                 $"({options.WindowsCredentials}), the NetApi requests are skipped.", ex);

                        // Not a wrong password (the logon doesn't check it)
                        _windowsLogonError = GetWin32Error(ex);
                    }
                }

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
                var hostDuration = Math.Max(
                    options.ICMPEnabled ? (long)options.ICMPAttempts * options.ICMPTimeout : 0,
                    Math.Max(
                        options.PortScanEnabled
                            ? (long)options.PortScanTimeout * options.PortScanAttempts *
                              (long)Math.Ceiling(options.PortScanPorts.Length / (double)options.MaxPortThreads)
                            : 0,
                        Math.Max(options.NetBIOSEnabled ? options.NetBIOSTimeout : 0,
                            // v3: discovery and request, each with the timeout
                            options.SNMPEnabled
                                ? (long)options.SNMPTimeout *
                                  (options.SNMPCredentials.Version == SNMPVersion.V3 ? 2 : 1)
                                : 0)));

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
                // A NetApi call that exceeded the timeout may still run, the thread keeps its impersonation token
                _windowsLogon?.Dispose();

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
        // Neighbor entry before the probes are sent: a Reachable entry is not resolved again by the probes, so after
        // them it doesn't prove an answer (see IPScannerNeighborCheck.Check)
        var isNeighborCheckEnabled = options.ARPEnabled && IsInLocalSubnet(ipAddress, networkInterfaces);

        var wasNeighborReachableBefore = isNeighborCheckEnabled &&
                                         await _neighborCache.WasReachableBeforeAsync(ipAddress, cancellationToken)
                                             .ConfigureAwait(false);

        // Completed by the port scan when the first port answers (open or refused), so the host is found before
        // all ports are finished
        var portAnswered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // Without ping, the host gets the placeholder of the ping (see CreateHostInfo)
        var pingTask = options.ICMPEnabled
            ? PingProbe.PingAsync(ipAddress, options.ICMPTimeout, options.ICMPAttempts, _icmpBuffer,
                cancellationToken)
            : null;

        var portScanTask = options.PortScanEnabled
            ? PortScanAsync(ipAddress, portScanParallelOptions, () => portAnswered.TrySetResult(), cancellationToken)
            : Task.FromResult(new List<PortInfo>());

        // NetBIOS only exists over IPv4 - an IPv6 host would never answer and wait for the timeout
        var netBIOSTask = options.NetBIOSEnabled && ipAddress.AddressFamily == AddressFamily.InterNetwork
            ? NetBIOSResolver.ResolveAsync(ipAddress, options.NetBIOSTimeout, cancellationToken)
            : Task.FromResult(new NetBIOSInfo(ipAddress));

        // A host that answers the SNMP request is up as well (null if it didn't answer)
        var snmpTask = options.SNMPEnabled
            ? SNMPSystemInfoResolver.ResolveAsync(ipAddress, options.SNMPCredentials, options.SNMPTimeout,
                cancellationToken)
            : null;

        var enrichTask = Task.CompletedTask;

        // Results of the finished probes, null while a probe is running
        PingInfo pingInfo = null;
        List<PortInfo> ports = null;
        NetBIOSInfo netBIOSInfo = null;

        // Result of the SNMP request (null if the host didn't answer), see isSNMPFinished
        SNMPSystemInfo snmpInfo = null;
        var isSNMPFinished = snmpTask == null;

        // Probes that answered so far
        var detectedBy = IPScannerDetectionMethod.None;

        // The title of the web page and the Windows host information are read for the found host (when the port scan
        // is finished)
        var isWebPagePending = false;
        var isWindowsPending = false;

        // DNS hostname of the host (empty if none or disabled) - the NetApi requests use it to connect by name
        var dnsHostname = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        IPScannerHostInfo hostInfo = null;

        var isWindowsStarted = false;

        // Windows host information of a found host - as soon as the result of TCP 445 is known if the port scan
        // checks it (not after the other probes, e.g. SNMP or a long port list), otherwise right away
        void StartWindows()
        {
            if (!isWindowsPending || isWindowsStarted || (_isSMBPortScanned && ports == null))
                return;

            isWindowsStarted = true;

            if (IsSMBPortOpenOrNotScanned(ports))
                enrichTask = Task.WhenAll(enrichTask,
                    UpdateWindowsAsync(ipAddress, hostInfo, dnsHostname.Task, cancellationToken));
            else
                OnHostUpdated(new IPScannerHostUpdatedArgs(hostInfo, x => x.IsWindowsPending = false));
        }

        // The host answered (the first probe, or its neighbor entry): it is up - raise it as found with the results so
        // far and start the lookups (DNS, MAC address, mDNS, Windows)
        void RaiseHostFound()
        {
            hostInfo = CreateHostInfo(ipAddress, detectedBy, pingInfo, ports, netBIOSInfo, snmpInfo, isSNMPFinished);
            isWebPagePending = hostInfo.IsWebPagePending;
            isWindowsPending = hostInfo.IsWindowsPending;

            _hostsUp.Increment();

            OnHostFound(new IPScannerHostFoundArgs(hostInfo));
            OnProgressChanged();

            enrichTask = EnrichAsync(ipAddress, hostInfo, DateTime.UtcNow, networkInterfaces, dnsHostname,
                cancellationToken);

            StartWindows();
        }

        try
        {
            List<Task> pending = [portScanTask, netBIOSTask, portAnswered.Task];

            if (pingTask != null)
                pending.Add(pingTask);

            if (snmpTask != null)
                pending.Add(snmpTask);

            while (pending.Count > 0)
            {
                var completed = await Task.WhenAny(pending).ConfigureAwait(false);
                pending.Remove(completed);

                // Rethrow the cancellation / error of the probe
                await completed.ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                bool isAnswer;
                IPScannerDetectionMethod method;
                Action<IPScannerHostInfo> update = null;

                if (completed == pingTask)
                {
                    var value = pingInfo = pingTask.Result;

                    isAnswer = value.Status == IPStatus.Success; // ICMP response
                    method = IPScannerDetectionMethod.ICMP;
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
                    method = IPScannerDetectionMethod.TCP;

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
                    method = IPScannerDetectionMethod.NetBIOS;
                    update = x =>
                    {
                        x.NetBIOSInfo = value;
                        x.IsNetBIOSPending = false;
                    };
                }
                else if (completed == snmpTask)
                {
                    var value = snmpInfo = snmpTask.Result;
                    isSNMPFinished = true;

                    isAnswer = value != null; // SNMP response
                    method = IPScannerDetectionMethod.SNMP;
                    update = x =>
                    {
                        x.SNMPName = value?.Name ?? string.Empty;
                        x.SNMPDescription = value?.Description ?? string.Empty;
                        x.SNMPLocation = value?.Location ?? string.Empty;
                        x.SNMPError = value?.Error ?? SNMPV3ErrorCode.None;
                        x.IsSNMPPending = false;
                    };
                }
                else
                {
                    // A port answered - its result follows with the port scan
                    isAnswer = true;
                    method = IPScannerDetectionMethod.TCP;
                }

                if (isAnswer)
                    detectedBy |= method;

                if (hostInfo != null)
                {
                    // Another probe answered after the host was found
                    if (isAnswer)
                    {
                        var detectedByValue = detectedBy;

                        update += x => x.DetectedBy = detectedByValue;
                    }

                    if (update != null)
                        OnHostUpdated(new IPScannerHostUpdatedArgs(hostInfo, update));

                    // E.g. the port scan finished
                    StartWindows();

                    continue;
                }

                if (!isAnswer)
                    continue;

                // First answer: the host is up. Before it could answer, the local system had to resolve its MAC
                // address, so it is in the neighbor table (ARP/NDP) from now on.
                RaiseHostFound();
            }

            // No probe answered: a host in a local subnet may still have answered the ARP request / neighbor
            // solicitation of the probes (e.g. a firewall that drops everything else)
            if (hostInfo == null && isNeighborCheckEnabled &&
                await _neighborCache.IsNeighborAsync(ipAddress, wasNeighborReachableBefore, cancellationToken)
                    .ConfigureAwait(false))
            {
                detectedBy = IPScannerDetectionMethod.ARP;
                RaiseHostFound();
            }

            // Title of the web page - needs the open ports, so after the port scan. Without an open web port, the
            // placeholder of a host found while the port scan was running is replaced.
            if (isWebPagePending)
            {
                var webPorts = GetWebPorts(ports);

                if (webPorts.Count > 0)
                    enrichTask = Task.WhenAll(enrichTask,
                        UpdateWebPageAsync(ipAddress, hostInfo, webPorts, cancellationToken));
                else
                    OnHostUpdated(new IPScannerHostUpdatedArgs(hostInfo, x => x.IsWebPagePending = false));
            }

            if (hostInfo == null)
            {
                // Count unreachable hosts unconditionally, since ShowAllResults may prevent them from ever
                // reaching HostFound
                _hostsDown.Increment();

                if (options.ShowAllResults || options.ReportAllHosts ||
                    options.ReportHosts?.Contains(ipAddress) == true)
                {
                    hostInfo = CreateHostInfo(ipAddress, IPScannerDetectionMethod.None, pingInfo, ports,
                        netBIOSInfo, snmpInfo, isSNMPFinished);

                    OnHostFound(new IPScannerHostFoundArgs(hostInfo));

                    // An unreachable host doesn't need a new snapshot of the neighbor table - the table is only read
                    // if it was not read yet (e.g. a host that blocks ICMP still has an ARP entry from the ping)
                    enrichTask = EnrichAsync(ipAddress, hostInfo, DateTime.MinValue, networkInterfaces,
                        dnsHostname, cancellationToken);
                }
            }

            await enrichTask.ConfigureAwait(false);
        }
        finally
        {
            // Never return while a probe or lookup is still running (e.g. another probe was canceled or failed)
            await WaitIgnoringErrorsAsync(pingTask ?? Task.CompletedTask, portScanTask, netBIOSTask,
                snmpTask ?? Task.CompletedTask, enrichTask).ConfigureAwait(false);
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
    ///     (marked as pending) and the disabled ones.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="detectedBy">Probes the host answered so far, <see cref="IPScannerDetectionMethod.None" /> if it is
    /// unreachable.</param>
    /// <param name="pingInfo">Result of the ping, null if it is still running or disabled.</param>
    /// <param name="ports">All probed ports, null if the port scan is still running.</param>
    /// <param name="netBIOSInfo">Result of the NetBIOS request, null if it is still running.</param>
    /// <param name="snmpInfo">Result of the SNMP request, null if it is still running or the host didn't answer.</param>
    /// <param name="isSNMPFinished">The SNMP request is finished (or disabled).</param>
    /// <returns>Result of the host.</returns>
    private IPScannerHostInfo CreateHostInfo(IPAddress ipAddress, IPScannerDetectionMethod detectedBy,
        PingInfo pingInfo, List<PortInfo> ports, NetBIOSInfo netBIOSInfo, SNMPSystemInfo snmpInfo,
        bool isSNMPFinished)
    {
        var isReachable = detectedBy != IPScannerDetectionMethod.None;

        return new IPScannerHostInfo(
            ipAddress,
            isReachable,
            pingInfo ?? new PingInfo(DateTime.Now, ipAddress, IPStatus.Unknown),
            ports?.Any(x => x.State == PortState.Open) ?? false,
            ports != null ? GetShownPorts(ports) : [],
            netBIOSInfo ?? new NetBIOSInfo(ipAddress))
        {
            DetectedBy = detectedBy,
            Role = IPScannerHostRoleHelper.GetRole(_hostRoles, ipAddress),
            IsPingPending = pingInfo == null && options.ICMPEnabled,
            IsOSGuessEnabled = options.OSGuessEnabled,
            // Lookups of every shown host and requests to reachable hosts, started after the host was found (see
            // EnrichAsync, ScanHostAsync)
            IsDNSPending = options.ResolveHostname,
            IsNeighborMACAddressPending = options.ResolveMACAddress,
            IsMDNSPending = isReachable && options.MDNSEnabled,
            IsSNMPPending = !isSNMPFinished,
            SNMPName = snmpInfo?.Name ?? string.Empty,
            SNMPDescription = snmpInfo?.Description ?? string.Empty,
            SNMPLocation = snmpInfo?.Location ?? string.Empty,
            SNMPError = snmpInfo?.Error ?? SNMPV3ErrorCode.None,
            IsWebPagePending = isReachable && options.WebPageEnabled &&
                                 (ports == null ? options.PortScanEnabled : GetWebPorts(ports).Count > 0),
            IsWindowsPending = isReachable && options.WindowsEnabled && (ports == null || IsSMBPortOpenOrNotScanned(ports)),
            IsPortScanPending = ports == null,
            IsNetBIOSPending = netBIOSInfo == null
        };
    }

    /// <summary>
    ///     Enriches a found host with the DNS hostname (PTR), MAC address and vendor and the mDNS hostname (only
    ///     reachable hosts). Each result is raised as <see cref="HostUpdated" /> as soon as it arrives; the task
    ///     completes only when all lookups are finished, so the host (and the scan) is never finished while a lookup is
    ///     still running.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="host">Result of the host, already raised in <see cref="HostFound" />.</param>
    /// <param name="discoveredAt">Time the host answered, <see cref="DateTime.MinValue" /> if it is unreachable.</param>
    /// <param name="networkInterfaces">Local network interfaces (for the MAC address lookup).</param>
    /// <param name="dnsHostnameResult">Completed with the DNS hostname (empty if none or disabled).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private async Task EnrichAsync(IPAddress ipAddress, IPScannerHostInfo host, DateTime discoveredAt,
        List<NetworkInterfaceInfo> networkInterfaces, TaskCompletionSource<string> dnsHostnameResult,
        CancellationToken cancellationToken)
    {
        async Task UpdateHostnameAsync()
        {
            string dnsHostname;

            try
            {
                dnsHostname = await DNSClient.GetInstance().ResolvePtrOrEmptyAsync(ipAddress, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch
            {
                // Also on cancellation / an error, nothing waits for a hostname that never comes
                dnsHostnameResult.TrySetResult(string.Empty);
                throw;
            }

            dnsHostnameResult.TrySetResult(dnsHostname ?? string.Empty);

            // Also without a hostname, so the placeholder is replaced
            OnHostUpdated(new IPScannerHostUpdatedArgs(host, x =>
            {
                x.DNSHostname = dnsHostname ?? string.Empty;
                x.IsDNSPending = false;
            }));
        }

        async Task UpdateMDNSHostnameAsync()
        {
            var mdnsHostname = await MDNSResolver.ResolveAsync(ipAddress, options.MDNSTimeout, cancellationToken)
                .ConfigureAwait(false);

            // Also without an answer, so the placeholder is replaced
            OnHostUpdated(new IPScannerHostUpdatedArgs(host, x =>
            {
                x.MDNSHostname = mdnsHostname;
                x.IsMDNSPending = false;
            }));
        }

        async Task UpdateMACAddressAsync()
        {
            var (macAddress, vendor) = await ResolveMACAddressAsync(ipAddress, discoveredAt, networkInterfaces,
                cancellationToken).ConfigureAwait(false);

            // Also without a MAC address, so the placeholder is replaced
            OnHostUpdated(new IPScannerHostUpdatedArgs(host, x =>
            {
                x.NeighborMACAddress = macAddress;
                x.NeighborVendor = vendor;
                x.IsNeighborMACAddressPending = false;
            }));
        }

        // Requests sent to the host (e.g. mDNS) only for reachable hosts
        var isReachable = discoveredAt != DateTime.MinValue;

        if (!options.ResolveHostname)
            dnsHostnameResult.TrySetResult(string.Empty);

        // The lookups don't depend on each other - run them concurrently, the host is finished after the slowest one
        // instead of after all of them
        await Task.WhenAll(
            options.ResolveHostname ? UpdateHostnameAsync() : Task.CompletedTask,
            options.ResolveMACAddress ? UpdateMACAddressAsync() : Task.CompletedTask,
            options.MDNSEnabled && isReachable ? UpdateMDNSHostnameAsync() : Task.CompletedTask).ConfigureAwait(false);
    }

    /// <summary>
    ///     Gets the ports whose web page title is read (see <see cref="IPScannerOptions.WebPagePorts" />) that are
    ///     open, in the configured order.
    /// </summary>
    /// <param name="ports">All probed ports, null if the port scan is disabled.</param>
    /// <returns>Open web ports.</returns>
    private List<int> GetWebPorts(List<PortInfo> ports)
    {
        if (ports == null)
            return [];

        var openPorts = ports.Where(x => x.State == PortState.Open).Select(x => x.Port).ToHashSet();

        return [.. options.WebPagePorts.Where(openPorts.Contains)];
    }

    /// <summary>
    ///     Reads the title of the web page of a host and raises it as <see cref="HostUpdated" />.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="host">Result of the host, already raised in <see cref="HostFound" />.</param>
    /// <param name="webPorts">Open ports to request, in the order to try them.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private async Task UpdateWebPageAsync(IPAddress ipAddress, IPScannerHostInfo host, List<int> webPorts,
        CancellationToken cancellationToken)
    {
        var webPageInfo = await WebPageResolver.ResolveAsync(ipAddress, webPorts, options.WebPageTimeout,
            cancellationToken).ConfigureAwait(false);

        // Also without an answer, so the placeholder is replaced
        OnHostUpdated(new IPScannerHostUpdatedArgs(host, x =>
        {
            x.WebTitle = webPageInfo?.Title ?? string.Empty;
            x.WebURL = webPageInfo?.URL ?? string.Empty;
            x.WebStatusCode = webPageInfo?.StatusCode ?? 0;
            x.WebServer = webPageInfo?.Server ?? string.Empty;
            x.WebPoweredBy = webPageInfo?.PoweredBy ?? string.Empty;
            x.WebRealm = webPageInfo?.Realm ?? string.Empty;
            x.WebRedirectURL = webPageInfo?.RedirectURL ?? string.Empty;
            x.WebCertificate = webPageInfo?.Certificate;
            x.IsWebPagePending = false;
        }));
    }

    /// <summary>
    ///     Gets the Win32 error of an exception of the Windows logon / NetApi requests.
    /// </summary>
    /// <param name="ex">Exception.</param>
    /// <returns>Error of a <see cref="Win32Exception" />, otherwise <c>ERROR_GEN_FAILURE</c> (31).</returns>
    private static int GetWin32Error(Exception ex)
    {
        const int errorGenFailure = 31;

        return ex is Win32Exception win32Exception ? win32Exception.NativeErrorCode : errorGenFailure;
    }

    /// <summary>
    ///     Checks if the Windows host information is read for a found host: with the port scan and TCP 445 in its
    ///     ports only if 445 is open, otherwise for every found host (the SMB request tells if 445 is open).
    /// </summary>
    /// <param name="ports">
    ///     All probed ports (empty if the port scan is disabled; null while it runs, only if it doesn't check 445).
    /// </param>
    /// <returns>True if the Windows host information is read.</returns>
    private bool IsSMBPortOpenOrNotScanned(List<PortInfo> ports)
    {
        if (!_isSMBPortScanned)
            return true;

        return ports.Any(x => x.Port == SMBInfoResolver.SMBPort && x.State == PortState.Open);
    }

    /// <summary>
    ///     Reads the Windows host information of a host and raises it as <see cref="HostUpdated" />: the SMB / NTLM
    ///     values (no credentials), then - if the host answered on TCP 445 - the NetApi values with the credentials of
    ///     the scan.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="host">Result of the host, already raised in <see cref="HostFound" />.</param>
    /// <param name="dnsHostname">DNS hostname of the host (empty if none or disabled).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private async Task UpdateWindowsAsync(IPAddress ipAddress, IPScannerHostInfo host, Task<string> dnsHostname,
        CancellationToken cancellationToken)
    {
        var smbInfo = await SMBInfoResolver.ResolveAsync(ipAddress, options.WindowsTimeout, cancellationToken)
            .ConfigureAwait(false);

        WindowsNetApiInfo netApiInfo = null;

        // No SMB answer: no NetApi requests (they would wait for the SMB / RPC timeouts)
        if (smbInfo != null)
        {
            var target = GetWindowsTarget(ipAddress, await dnsHostname.ConfigureAwait(false), smbInfo.NTLM);

            netApiInfo = await ResolveNetApiAsync(ipAddress.ToString(), target, smbInfo.NTLM, cancellationToken)
                .ConfigureAwait(false);
        }

        var windowsInfo = smbInfo == null ? null : new WindowsHostInfo(smbInfo, netApiInfo);

        // Also without an answer, so the placeholder is replaced
        OnHostUpdated(new IPScannerHostUpdatedArgs(host, x =>
        {
            x.Windows = windowsInfo;
            x.IsWindowsPending = false;
        }));
    }

    /// <summary>
    ///     Gets the name the NetApi requests connect to: the DNS hostname if it is the DNS name the host itself sends in
    ///     the NTLM challenge (the full name - a stale PTR record or a computer with the same name in another domain
    ///     could resolve to another host, which would get the credentials and show its values), so Kerberos can be
    ///     used; otherwise the IP address (NTLM only), e.g. for a computer in a workgroup (no DNS domain).
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="dnsHostname">DNS hostname (empty if none).</param>
    /// <param name="ntlm">NTLM challenge of the host, null if none.</param>
    /// <returns>Hostname or IP address.</returns>
    public static string GetWindowsTarget(IPAddress ipAddress, string dnsHostname, NTLMChallengeInfo ntlm)
    {
        if (string.IsNullOrEmpty(dnsHostname) || string.IsNullOrEmpty(ntlm?.DNSComputerName) ||
            !ntlm.DNSComputerName.Contains('.'))
            return ipAddress.ToString();

        var hostname = dnsHostname.TrimEnd('.');

        return string.Equals(hostname, ntlm.DNSComputerName.TrimEnd('.'), StringComparison.OrdinalIgnoreCase)
            ? hostname
            : ipAddress.ToString();
    }

    /// <summary>
    ///     Runs the blocking NetApi requests of a host on its own thread (max. <see cref="MaxWindowsThreads" /> at the
    ///     same time) with the network logon of the scan, waits at most the timeout.
    /// </summary>
    /// <param name="ipAddress">IP address of the host (if the hostname can't be found).</param>
    /// <param name="target">Hostname or IP address to connect to, see <see cref="GetWindowsTarget" />.</param>
    /// <param name="ntlm">NTLM challenge of the host (domain for the lockout protection), null if none.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>NetApi information, with <see cref="WindowsHostInfo.ErrorTimeout" /> after the timeout.</returns>
    private async Task<WindowsNetApiInfo> ResolveNetApiAsync(string ipAddress, string target, NTLMChallengeInfo ntlm,
        CancellationToken cancellationToken)
    {
        if (_windowsLogonError != 0)
            return WindowsNetApiInfo.FromError(_windowsLogonError);

        var timeout = TimeSpan.FromMilliseconds(options.WindowsTimeout);

        // Credentials: checked on one host before the others use them (a wrong password must not lock the account)
        var decision = _windowsLogonGate == null
            ? WindowsLogonGate.Decision.Proceed
            : await _windowsLogonGate.WaitAsync(ntlm, ipAddress, timeout * 2, cancellationToken).ConfigureAwait(false);

        switch (decision)
        {
            case WindowsLogonGate.Decision.Skip:
                return WindowsNetApiInfo.FromError(WindowsHostInfo.ErrorLogonSkipped);
            // Never contacted: not the timeout of the host
            case WindowsLogonGate.Decision.Timeout:
                return WindowsNetApiInfo.FromError(WindowsHostInfo.ErrorLogonNotChecked);
        }

        // The host checks the credentials: its result is reported when the call returns - also after the timeout of
        // the host, so the next host doesn't check them while this logon is still running (a late logon failure in
        // the domain still rejects them)
        // A host that is not gated reports its own logon failure (skipped in the next scans)
        var isUngated = _windowsLogonGate != null && !_windowsLogonGate.IsGated(ntlm);

        Action<WindowsNetApiInfo> onCallCompleted = decision == WindowsLogonGate.Decision.Check
            ? x => _windowsLogonGate.Report(ntlm, x)
            : isUngated
                ? x => _windowsLogonGate.ReportUngated(ipAddress, x)
                : null;

        // Local account (".\admin"): logged on with the computer name of the host ("PC01\admin"), so only the host
        // checks it (not gated, the computer name is known)
        var hostCredentials = isUngated ? options.WindowsCredentials.ForHost(ntlm.NetBIOSComputerName) : null;

        return await ResolveNetApiWithLogonAsync(ipAddress, target, timeout, hostCredentials, onCallCompleted,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Runs the blocking NetApi requests of a host, see <see cref="ResolveNetApiAsync" />.
    /// </summary>
    /// <param name="hostCredentials">
    ///     Credentials of a local account for this host (logged on for this call), null for the logon of the scan.
    /// </param>
    /// <param name="onCallCompleted">
    ///     Called once with the result of the call when it is known (the NetApi values were read before the remote
    ///     registry, or the call returned - also after the timeout), null if it was not started (canceled while waiting
    ///     for a slot); optional.
    /// </param>
    private async Task<WindowsNetApiInfo> ResolveNetApiWithLogonAsync(string ipAddress, string target,
        TimeSpan timeout, WindowsCredentials hostCredentials, Action<WindowsNetApiInfo> onCallCompleted,
        CancellationToken cancellationToken)
    {
        // Every path reports through Complete, exactly once (the lockout protection releases its turn): the NetApi
        // values (the registry doesn't change the logon), the result of the call, or why it didn't run
        var isCompleted = 0;

        void Complete(WindowsNetApiInfo info)
        {
            if (Interlocked.Exchange(ref isCompleted, 1) == 0)
                onCallCompleted?.Invoke(info);
        }

        // All slots are taken by calls that wait for Windows (e.g. hosts that don't answer RPC)
        bool isSlotFree;

        try
        {
            isSlotFree = await WindowsSemaphore.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            Complete(null);
            throw;
        }

        if (!isSlotFree)
        {
            var info = WindowsNetApiInfo.FromError(WindowsHostInfo.ErrorTimeout);

            Complete(info);

            return info;
        }

        var logon = _windowsLogon;

        // Values of the NetApi before the remote registry is read: kept if the registry exceeds the timeout
        WindowsNetApiInfo netApiRead = null;

        void OnNetApiRead(WindowsNetApiInfo info)
        {
            Volatile.Write(ref netApiRead, info);

            Complete(info);
        }

        var task = Task.Factory.StartNew(() =>
        {
            WindowsNetApiInfo result = null;
            WindowsLogon hostLogon = null;

            try
            {
                WindowsNetApiInfo Resolve()
                {
                    var info = WindowsNetApiResolver.Resolve(target, OnNetApiRead);

                    // The hostname can't be found (e.g. no forward lookup zone): the IP address (NTLM)
                    return target != ipAddress && info.Error is 53 or 67 or 1214 or 1231
                        ? WindowsNetApiResolver.Resolve(ipAddress, OnNetApiRead)
                        : info;
                }

                // Local logon, no network: an error is not a failed logon at the host (e.g. invalid username)
                if (hostCredentials != null)
                    logon = hostLogon = WindowsLogon.Create(hostCredentials);

                result = logon != null ? logon.Run(Resolve) : Resolve();

                return result;
            }
            catch (Exception ex)
            {
                // E.g. the logon was disposed by the end of the scan while the call waited for a slot
                Log.Debug($"NetApi requests to {target} failed.", ex);

                result = WindowsNetApiInfo.FromError(GetWin32Error(ex));

                return result;
            }
            finally
            {
                hostLogon?.Dispose();

                WindowsSemaphore.Release();

                Complete(result);
            }
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        try
        {
            return await task.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            // The call keeps its thread (and slot) until Windows returns, its result is only reported (see
            // onCallCompleted). The NetApi values if only the remote registry is still read (without patch level).
            return Volatile.Read(ref netApiRead) ?? WindowsNetApiInfo.FromError(WindowsHostInfo.ErrorTimeout);
        }
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
            macAddress = (await _neighborCache.GetEntryAsync(ipAddress, discoveredAt, cancellationToken)
                .ConfigureAwait(false))?.MACAddress.ToString() ?? string.Empty;

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
    ///     Checks if <paramref name="ipAddress" /> is in a subnet of a local network interface (on-link). Only these
    ///     hosts are in the neighbor table - for a host behind a router, its MAC address can't be resolved via ARP/NDP
    ///     (the table contains the router), so there is no need to look it up (and to read the table again).
    ///     IPv6: link-local addresses, other addresses if they are in the same /64 as a local address (the prefix
    ///     length is not known, local IPv6 subnets are /64). Only operational network interfaces (an adapter that is
    ///     down may still have its static IPv4 address, e.g. of a subnet that is reached via VPN).
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="networkInterfaces">Local network interfaces.</param>
    /// <returns>True if the IP address is in a local subnet.</returns>
    private static bool IsInLocalSubnet(IPAddress ipAddress, IEnumerable<NetworkInterfaceInfo> networkInterfaces)
    {
        var operationalInterfaces = networkInterfaces.Where(x => x.IsOperational);

        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            var address = IPv4Address.ToUInt32(ipAddress);

            return operationalInterfaces.Any(networkInterface => networkInterface.IPv4Address.Any(x =>
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

        return operationalInterfaces.Any(networkInterface => networkInterface.IPv6Address.Any(x =>
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