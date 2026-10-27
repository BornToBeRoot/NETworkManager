using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
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

    private int _progressValue;
    private readonly InterlockedCounter _portsOpen = new();
    private readonly InterlockedCounter _portsClosed = new();
    private readonly InterlockedCounter _portsTimedOut = new();

    // Closed and timed out ports per host, see GetClosedAndTimedOutPortsPerHost
    private readonly ConcurrentDictionary<IPAddress, (InterlockedCounter closed, InterlockedCounter timedOut)>
        _portsPerHost = new();

    private readonly PortScannerOptions _options;

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

    public void ScanAsync(IEnumerable<(IPAddress ipAddress, string hostname)> hosts, IEnumerable<int> ports,
        CancellationToken cancellationToken)
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
                var portCount = ports.Count();
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

                await Parallel.ForEachAsync(hosts, hostParallelOptions, async (host, hostCt) =>
                {
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

    private void IncreaseProgress()
    {
        // Increase the progress                        
        Interlocked.Increment(ref _progressValue);
        OnProgressChanged();
    }

    #endregion
}