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

    private readonly PortScannerOptions _options;

    #endregion

    #region Events

    public event EventHandler<PortScannerPortScannedArgs> PortScanned;

    private void OnPortScanned(PortScannerPortScannedArgs e)
    {
        PortScanned?.Invoke(this, e);
    }

    public event EventHandler ScanComplete;

    private void OnScanComplete()
    {
        ScanComplete?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<ProgressChangedArgs> ProgressChanged;

    private void OnProgressChanged()
    {
        ProgressChanged?.Invoke(this, new ProgressChangedArgs(_progressValue));
    }

    public event EventHandler UserHasCanceled;

    private void OnUserHasCanceled()
    {
        UserHasCanceled?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Methods

    public void ScanAsync(IEnumerable<(IPAddress ipAddress, string hostname)> hosts, IEnumerable<int> ports,
        CancellationToken cancellationToken)
    {
        _progressValue = 0;

        Task.Run(async () =>
        {
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
                    // Resolve Hostname (PTR)
                    var hostname = string.Empty;

                    if (_options.ResolveHostname)
                    {
                        var dnsResult = await DNSClient.GetInstance().ResolvePtrAsync(host.ipAddress)
                            .ConfigureAwait(false);

                        if (!dnsResult.HasError)
                            hostname = dnsResult.Value;
                    }

                    // Check each port
                    await Parallel.ForEachAsync(ports, portParallelOptions, async (port, portCt) =>
                    {
                        var portState = await PortProbe.ProbeAsync(host.ipAddress, port, _options.Timeout, portCt)
                            .ConfigureAwait(false);

                        if (_options.ShowAllResults || portState == PortState.Open)
                            OnPortScanned(new PortScannerPortScannedArgs(
                                new PortScannerPortInfo(host.ipAddress, hostname, port,
                                    PortLookup.LookupByPortAndProtocol(port), portState)));

                        IncreaseProgress();
                    }).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                OnUserHasCanceled();
            }
            finally
            {
                OnScanComplete();
            }
        }, cancellationToken);
    }

    private void IncreaseProgress()
    {
        // Increase the progress                        
        Interlocked.Increment(ref _progressValue);
        OnProgressChanged();
    }

    #endregion
}