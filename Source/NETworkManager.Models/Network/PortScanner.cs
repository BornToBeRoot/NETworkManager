using NETworkManager.Models.Lookup;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

public sealed class PortScanner
{
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

    #region Constructor
    public PortScanner(PortScannerOptions options)
    {
        _options = options;
    }
    #endregion

    #region Methods
    public void ScanAsync(IEnumerable<(IPAddress ipAddress, string hostname)> hosts, IEnumerable<int> ports, CancellationToken cancellationToken)
    {
        _progressValue = 0;

        Task.Run(() =>
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

                Parallel.ForEach(hosts, hostParallelOptions, host =>
                {
                    // Resolve Hostname (PTR)
                    var hostname = string.Empty;

                    if (_options.ResolveHostname)
                    {
                        // Don't use await in Parallel.ForEach, this will break
                        var dnsResolverTask = DNSClient.GetInstance().ResolvePtrAsync(host.ipAddress);

                        // Wait for task inside a Parallel.Foreach
                        dnsResolverTask.Wait(cancellationToken);

                        if (!dnsResolverTask.Result.HasError)
                            hostname = dnsResolverTask.Result.Value;
                    }

                    // Check each port
                    Parallel.ForEach(ports, portParallelOptions, port =>
                    {
                        // Test if port is open
                        using (var tcpClient = new TcpClient(host.ipAddress.AddressFamily))
                        {
                            var portState = PortState.None;

                            try
                            {
                                var task = tcpClient.ConnectAsync(host.ipAddress, port);

                                if (task.Wait(_options.Timeout))
                                    portState = tcpClient.Connected ? PortState.Open : PortState.Closed;
                                else
                                    portState = PortState.TimedOut;
                            }
                            catch
                            {
                                portState = PortState.Closed;
                            }
                            finally
                            {
                                tcpClient.Close();

                                if (_options.ShowAllResults || portState == PortState.Open)
                                    OnPortScanned(new PortScannerPortScannedArgs(
                                        new PortScannerPortInfo(host.ipAddress, hostname, port, PortLookup.LookupByPortAndProtocol(port), portState)));
                            }
                        }

                        IncreaseProgress();
                    });
                });
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
