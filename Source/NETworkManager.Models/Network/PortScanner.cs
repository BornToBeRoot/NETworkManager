using System;
using System.Collections.Generic;
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

    private int _progressValue;
    private readonly InterlockedCounter _portsOpen = new();
    private readonly InterlockedCounter _portsClosed = new();

    private readonly PortScannerOptions _options;

    /// <summary>
    ///     Gets the number of ports found to be open so far. Thread-safe; may be read from any
    ///     thread while the scan is running.
    /// </summary>
    public int PortsOpen => _portsOpen.Value;

    /// <summary>
    ///     Gets the number of ports found to be closed (or timed out) so far. Thread-safe; may be
    ///     read from any thread while the scan is running.
    /// </summary>
    public int PortsClosed => _portsClosed.Value;

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
                var hostParallelOptions = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = _options.MaxHostThreads
                };

                var portParallelOptions = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = _options.MaxPortThreads
                };

                await Parallel.ForEachAsync(hosts, hostParallelOptions, async (host, hostCt) =>
                {
                    // Resolve Hostname (PTR) while the ports are scanned
                    var hostnameTask = _options.ResolveHostname
                        ? ResolveHostnameAsync(host.ipAddress, hostCt)
                        : Task.FromResult(string.Empty);

                    // Check each port
                    await Parallel.ForEachAsync(ports, portParallelOptions, async (port, portCt) =>
                    {
                        var portState = await PortProbe.ProbeAsync(host.ipAddress, port, _options.Timeout, portCt)
                            .ConfigureAwait(false);

                        // Every result needs the hostname (results are grouped by it) - usually resolved
                        // long before the first port is finished
                        var hostname = await hostnameTask.ConfigureAwait(false);

                        // Count open/closed ports unconditionally, since ShowAllResults (below)
                        // may prevent closed ports from ever reaching PortScanned
                        if (portState == PortState.Open)
                            _portsOpen.Increment();
                        else
                            _portsClosed.Increment();

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

    private static async Task<string> ResolveHostnameAsync(IPAddress ipAddress, CancellationToken cancellationToken)
    {
        var dnsResult = await DNSClient.GetInstance().ResolvePtrAsync(ipAddress)
            .WaitAsync(cancellationToken).ConfigureAwait(false);

        return dnsResult.HasError ? string.Empty : dnsResult.Value;
    }

    private void IncreaseProgress()
    {
        // Increase the progress                        
        Interlocked.Increment(ref _progressValue);
        OnProgressChanged();
    }

    #endregion
}