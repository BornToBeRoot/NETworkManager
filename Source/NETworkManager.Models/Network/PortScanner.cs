using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using NETworkManager.Models.Lookup;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

public sealed class PortScanner
{
    #region Constructor

    public PortScanner(PortScannerOptions options)
    {
        _options = options;
        _icmpBuffer = new byte[options.ICMPBufferSize];
    }

    #endregion

    #region Variables

    /// <summary>
    ///     Time between the start of two port probes (of all hosts) during the ramp-up, see
    ///     <see cref="StartScheduler" />. Smaller than in the IP Scanner, because there are more concurrent probes:
    ///     with the default settings the first 4 hosts x 128 ports are started within ~0.5 s instead of at the same
    ///     moment, the following ones when a slot is free. Shorter if a probe takes less than 512 x 1 ms
    ///     (see <see cref="StartScheduler.GetInterval" />).
    /// </summary>
    private static readonly TimeSpan MaxPortStartInterval = TimeSpan.FromMilliseconds(1);

    /// <summary>
    ///     Threads adjusted automatically (see <see cref="GetThreads" />): connections at the same time (the old
    ///     defaults 4 hosts x 128 ports).
    /// </summary>
    private const int AutoMaxConnections = 512;

    /// <summary>
    ///     Threads adjusted automatically: hosts at the same time - also the number of concurrent DNS (PTR) lookups,
    ///     like the default of the IP Scanner.
    /// </summary>
    private const int AutoMaxHostThreads = 128;

    /// <summary>
    ///     Threads adjusted automatically: ports of one host at the same time.
    /// </summary>
    private const int AutoMaxPortThreads = 128;

    /// <summary>
    ///     Threads adjusted automatically: pings before the scan at the same time (see
    ///     <see cref="PortScannerOptions.PingHostsBeforeScan" />), like the default host threads of the IP Scanner.
    /// </summary>
    private const int AutoMaxPingThreads = 128;

    private int _progressValue;
    private readonly InterlockedCounter _portsOpen = new();
    private readonly InterlockedCounter _portsClosed = new();
    private readonly InterlockedCounter _portsTimedOut = new();
    private readonly InterlockedCounter _hostsSkipped = new();

    // Closed and timed out ports per host, see GetClosedAndTimedOutPortsPerHost
    private readonly ConcurrentDictionary<IPAddress, (InterlockedCounter closed, InterlockedCounter timedOut)>
        _portsPerHost = new();

    private readonly PortScannerOptions _options;

    // Data sent with the ping before the scan, created once (see PortScannerOptions.ICMPBufferSize)
    private readonly byte[] _icmpBuffer;

    /// <summary>
    ///     Gets the number of ports scanned so far. Thread-safe; may be read from any thread while the scan is
    ///     running.
    /// </summary>
    public int PortsScanned => Volatile.Read(ref _progressValue);

    /// <summary>
    ///     Gets the number of ports found to be open so far. Thread-safe; may be read from any
    ///     thread while the scan is running.
    /// </summary>
    public int PortsOpen => _portsOpen.Value;

    /// <summary>
    ///     Gets the number of ports found to be closed (the host refused the connection) so far. Thread-safe; may be
    ///     read from any thread while the scan is running.
    /// </summary>
    public int PortsClosed => _portsClosed.Value;

    /// <summary>
    ///     Gets the number of ports without an answer (timed out) so far. Thread-safe; may be read from any thread
    ///     while the scan is running.
    /// </summary>
    public int PortsTimedOut => _portsTimedOut.Value;

    /// <summary>
    ///     Gets the number of hosts that were not scanned because they didn't answer the ping before the scan (see
    ///     <see cref="PortScannerOptions.PingHostsBeforeScan" />). Thread-safe; may be read from any thread while the
    ///     scan is running.
    /// </summary>
    public int HostsSkipped => _hostsSkipped.Value;

    /// <summary>
    ///     Gets the number of closed and timed out ports per host scanned so far, only hosts with such a port. These
    ///     ports are only raised in <see cref="PortScanned" /> if they are shown - e.g. a rescan of some hosts needs
    ///     them to keep the counters of the other hosts.
    /// </summary>
    /// <returns>Number of closed and timed out ports per IP address.</returns>
    public Dictionary<IPAddress, (int closed, int timedOut)> GetClosedAndTimedOutPortsPerHost()
    {
        return _portsPerHost.ToDictionary(x => x.Key, x => (x.Value.closed.Value, x.Value.timedOut.Value));
    }

    #endregion

    #region Events

    public event EventHandler<PortScannerPortScannedArgs> PortScanned;

    private void OnPortScanned(PortScannerPortScannedArgs e)
    {
        PortScanned?.Invoke(this, e);
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

    public event EventHandler<ProgressChangedArgs> ProgressChanged;

    private void OnProgressChanged()
    {
        ProgressChanged?.Invoke(this, new ProgressChangedArgs(_progressValue));
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Scans the ports of the hosts. Results are raised in <see cref="PortScanned" />, the end in
    ///     <see cref="ScanComplete" />.
    /// </summary>
    /// <param name="hosts">Hosts to scan.</param>
    /// <param name="ports">Ports to scan on every host.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public void ScanAsync(IEnumerable<(IPAddress ipAddress, string hostname)> hosts, IEnumerable<int> ports,
        CancellationToken cancellationToken)
    {
        var portList = ports as IReadOnlyCollection<int> ?? [.. ports];

        Scan(hosts, _ => portList, portList.Count, cancellationToken);
    }

    /// <summary>
    ///     Scans single ports of hosts (e.g. a rescan of selected ports), every host only with its ports.
    /// </summary>
    /// <param name="hostPorts">IP address and port of every port to scan.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public void ScanPortsAsync(IEnumerable<(IPAddress ipAddress, int port)> hostPorts,
        CancellationToken cancellationToken)
    {
        var portsPerHost = hostPorts.GroupBy(x => x.ipAddress).ToDictionary(x => x.Key,
            x => (IReadOnlyCollection<int>)[.. x.Select(y => y.port).Distinct()]);

        Scan([.. portsPerHost.Keys.Select(x => (x, string.Empty))], x => portsPerHost[x],
            portsPerHost.Count == 0 ? 0 : portsPerHost.Values.Max(x => x.Count), cancellationToken);
    }

    /// <summary>
    ///     Scans the hosts, each with its ports.
    /// </summary>
    /// <param name="hosts">Hosts to scan.</param>
    /// <param name="getPorts">Gets the ports of a host.</param>
    /// <param name="maxPortCount">Most ports of a host (threads and the spread of the start).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private void Scan(IEnumerable<(IPAddress ipAddress, string hostname)> hosts,
        Func<IPAddress, IReadOnlyCollection<int>> getPorts, int maxPortCount, CancellationToken cancellationToken)
    {
        _progressValue = 0;

        // The token is not passed to Task.Run on purpose: with an already canceled token the delegate
        // would never run and ScanComplete would never be raised.
        Task.Run(async () =>
        {
            var isCanceled = false;
            Exception exception = null;

            try
            {
                var portCount = maxPortCount;
                var (maxHostThreads, maxPortThreads) = GetThreads(portCount);

                var hostParallelOptions = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = maxHostThreads
                };

                var portParallelOptions = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = maxPortThreads
                };

                // Spread the start of the port probes over time instead of starting all at once
                var hostConcurrency = hosts.TryGetNonEnumeratedCount(out var hostCount)
                    ? Math.Min(hostCount, maxHostThreads)
                    : maxHostThreads;

                var portConcurrency = hostConcurrency * Math.Min(portCount, maxPortThreads);

                var portStartScheduler = new StartScheduler(StartScheduler.GetInterval(MaxPortStartInterval,
                    TimeSpan.FromMilliseconds(_options.Timeout * _options.Attempts), portConcurrency), portConcurrency);

                // Only hosts that answer a ping are scanned (option). Adjusted automatically, the pings run ahead of
                // the port scan with their own concurrency - otherwise an unused address would take one of the few
                // hosts of the port scan (e.g. 4 for 1-1024) until its ping timed out. With the threads of the
                // settings, every host is pinged in its slot of the port scan, so Max. concurrent host threads also
                // limits the pings (e.g. to slow down a scan).
                var pingAhead = _options.PingHostsBeforeScan && _options.AutoAdjustThreads;
                var pingInSlot = _options.PingHostsBeforeScan && !pingAhead;

                if (pingAhead)
                    await ScanReachableHostsAsync(hosts, getPorts, hostParallelOptions, ScanHostAsync)
                        .ConfigureAwait(false);
                else
                    await Parallel.ForEachAsync(hosts, hostParallelOptions, ScanHostAsync).ConfigureAwait(false);

                async ValueTask ScanHostAsync((IPAddress ipAddress, string hostname) host, CancellationToken hostCt)
                {
                    var ports = getPorts(host.ipAddress);

                    if (pingInSlot &&
                        !await IsReachableAsync(host.ipAddress, ports.Count, hostCt).ConfigureAwait(false))
                        return;

                    // Resolve Hostname (PTR) while the ports are scanned. Started with the first port probe of the
                    // host, so the lookups of many hosts are spread over time like the probes instead of all at once
                    var hostnameTask = new Lazy<Task<string>>(() => _options.ResolveHostname
                        ? DNSClient.GetInstance().ResolvePtrOrEmptyAsync(host.ipAddress, hostCt)
                        : Task.FromResult(string.Empty));

                    // Check each port
                    await Parallel.ForEachAsync(ports, portParallelOptions, async (port, portCt) =>
                    {
                        // Spread the start of the port probes over time instead of starting all at once
                        await portStartScheduler.WaitAsync(portCt).ConfigureAwait(false);

                        _ = hostnameTask.Value;

                        var portState = await PortProbe.ProbeAsync(host.ipAddress, port, _options.Timeout,
                                _options.Attempts, portCt)
                            .ConfigureAwait(false);

                        // Every result needs the hostname (results are grouped by it) - usually resolved
                        // long before the first port is finished
                        var hostname = await hostnameTask.Value.ConfigureAwait(false);

                        // Count open/closed/timed out ports unconditionally, since ShowAllResults (below)
                        // may prevent closed ports from ever reaching PortScanned
                        if (portState == PortState.Open)
                        {
                            _portsOpen.Increment();
                        }
                        else
                        {
                            var portsOfHost = _portsPerHost.GetOrAdd(host.ipAddress,
                                static _ => (new InterlockedCounter(), new InterlockedCounter()));

                            if (portState == PortState.Closed)
                            {
                                _portsClosed.Increment();
                                portsOfHost.closed.Increment();
                            }
                            else
                            {
                                _portsTimedOut.Increment();
                                portsOfHost.timedOut.Increment();
                            }
                        }

                        if (_options.ShowAllResults || portState == PortState.Open)
                            OnPortScanned(new PortScannerPortScannedArgs(
                                new PortScannerPortInfo(host.ipAddress, hostname, port,
                                    PortLookup.LookupByPortAndProtocol(port), portState)));

                        IncreaseProgress();
                    }).ConfigureAwait(false);
                }
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
    ///     Pings the hosts with <see cref="AutoMaxPingThreads" /> at the same time and scans the hosts that answer (as
    ///     soon as they answer) with <paramref name="scanHost" />. Returns when all pings and scans are finished, also on
    ///     cancellation or an error.
    /// </summary>
    /// <param name="hosts">Hosts to ping and scan.</param>
    /// <param name="getPorts">Gets the ports of a host (counted as scanned for a host that doesn't answer).</param>
    /// <param name="hostParallelOptions">
    ///     Options for the hosts that are scanned at the same time, with the token to monitor for cancellation requests.
    /// </param>
    /// <param name="scanHost">Scans the ports of a host.</param>
    private async Task ScanReachableHostsAsync(IEnumerable<(IPAddress ipAddress, string hostname)> hosts,
        Func<IPAddress, IReadOnlyCollection<int>> getPorts, ParallelOptions hostParallelOptions,
        Func<(IPAddress ipAddress, string hostname), CancellationToken, ValueTask> scanHost)
    {
        var reachableHosts = Channel.CreateUnbounded<(IPAddress ipAddress, string hostname)>();

        // Also stops the pings if the port scan fails
        using var pingCts = CancellationTokenSource.CreateLinkedTokenSource(hostParallelOptions.CancellationToken);

        var pingTask = PingHostsAsync();

        try
        {
            await Parallel.ForEachAsync(reachableHosts.Reader.ReadAllAsync(hostParallelOptions.CancellationToken),
                hostParallelOptions, scanHost).ConfigureAwait(false);
        }
        finally
        {
            // Never return while pings are still running (e.g. the port scan failed or was canceled), they would
            // change the progress after ScanComplete
            await pingCts.CancelAsync().ConfigureAwait(false);

            try
            {
                await pingTask.ConfigureAwait(false);
            }
            catch
            {
                // Raised below if the port scan was finished
            }
        }

        // Error of the pings
        await pingTask.ConfigureAwait(false);

        // Writes the hosts that answer to the channel, the port scan reads them while the other hosts are pinged
        async Task PingHostsAsync()
        {
            try
            {
                var pingParallelOptions = new ParallelOptions
                {
                    CancellationToken = pingCts.Token,
                    MaxDegreeOfParallelism = AutoMaxPingThreads
                };

                await Parallel.ForEachAsync(hosts, pingParallelOptions, async (host, ct) =>
                {
                    if (await IsReachableAsync(host.ipAddress, getPorts(host.ipAddress).Count, ct)
                            .ConfigureAwait(false))
                        reachableHosts.Writer.TryWrite(host); // Unbounded, always written
                }).ConfigureAwait(false);
            }
            finally
            {
                // Also on cancellation or an error (raised above), the port scan ends with the hosts written so far
                reachableHosts.Writer.TryComplete();
            }
        }
    }

    /// <summary>
    ///     Pings a host before the scan (see <see cref="PortScannerOptions.PingHostsBeforeScan" />). If it doesn't
    ///     answer, it is counted as skipped and its ports as scanned, so the progress is complete.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="portCount">Number of ports per host.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>True if the host answered the ping.</returns>
    private async Task<bool> IsReachableAsync(IPAddress ipAddress, int portCount, CancellationToken cancellationToken)
    {
        var pingInfo = await PingProbe.PingAsync(ipAddress, _options.ICMPTimeout, _options.ICMPAttempts,
            _icmpBuffer, cancellationToken).ConfigureAwait(false);

        if (pingInfo.Status == IPStatus.Success)
            return true;

        _hostsSkipped.Increment();
        IncreaseProgress(portCount);

        return false;
    }

    /// <summary>
    ///     Gets the number of hosts and the number of ports per host scanned at the same time: the settings, or -
    ///     adjusted automatically - <see cref="AutoMaxConnections" /> connections spread over as many hosts as needed
    ///     (at most <see cref="AutoMaxHostThreads" />) with up to <see cref="AutoMaxPortThreads" /> ports each. E.g.
    ///     4 hosts x 128 ports for 1-1024, 128 hosts x 1 port for one port (with the fixed 4 host threads only 4
    ///     connections at a time).
    /// </summary>
    /// <param name="portCount">Number of ports per host.</param>
    /// <returns>Hosts and ports per host at the same time.</returns>
    private (int hostThreads, int portThreads) GetThreads(int portCount)
    {
        if (!_options.AutoAdjustThreads)
            return (_options.MaxHostThreads, _options.MaxPortThreads);

        var portsAtOnce = Math.Max(1, Math.Min(portCount, AutoMaxPortThreads));

        return (Math.Clamp(AutoMaxConnections / portsAtOnce, 1, AutoMaxHostThreads), AutoMaxPortThreads);
    }

    private void IncreaseProgress(int count = 1)
    {
        // Increase the progress
        Interlocked.Add(ref _progressValue, count);
        OnProgressChanged();
    }

    #endregion
}