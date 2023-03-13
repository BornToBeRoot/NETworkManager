using NETworkManager.Models.Lookup;
using NETworkManager.Utilities;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

public class PortScanner
{
    #region Variables
    private int _progressValue;

    private readonly PortScannerOptions _options;
    #endregion

    #region Events
    public event EventHandler<PortScannerPortScannedArgs> PortScanned;

    protected virtual void OnPortScanned(PortScannerPortScannedArgs e)
    {
        PortScanned?.Invoke(this, e);
    }

    public event EventHandler ScanComplete;

    protected virtual void OnScanComplete()
    {
        ScanComplete?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<ProgressChangedArgs> ProgressChanged;

    public virtual void OnProgressChanged()
    {
        ProgressChanged?.Invoke(this, new ProgressChangedArgs(_progressValue));
    }

    public event EventHandler UserHasCanceled;

    protected virtual void OnUserHasCanceled()
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
    public void ScanAsync(IPAddress[] ipAddresses, int[] ports, CancellationToken cancellationToken)
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

                Parallel.ForEach(ipAddresses, hostParallelOptions, ipAddress =>
                {
                    // Resolve Hostname (PTR)
                    var hostname = string.Empty;

                    if (_options.ResolveHostname)
                    {
                        // Don't use await in Paralle.ForEach, this will break
                        var dnsResolverTask = DNSClient.GetInstance().ResolvePtrAsync(ipAddress);

                        // Wait for task inside a Parallel.Foreach
                        dnsResolverTask.Wait();

                        if (!dnsResolverTask.Result.HasError)
                            hostname = dnsResolverTask.Result.Value;
                    }

                    // Check each port
                    Parallel.ForEach(ports, portParallelOptions, port =>
                    {
                        // Test if port is open
                        using (var tcpClient = new TcpClient(ipAddress.AddressFamily))
                        {
                            PortState portState = PortState.None;

                            try
                            {
                                var task = tcpClient.ConnectAsync(ipAddress, port);

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
                                tcpClient?.Close();

                                if (_options.ShowAllResults || portState == PortState.Open)
                                    OnPortScanned(new PortScannerPortScannedArgs(ipAddress, hostname, port, PortLookup.GetByPortAndProtocol(port), portState));
                            }
                        }

                        IncreaseProgess();
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

    private void IncreaseProgess()
    {
        // Increase the progress                        
        Interlocked.Increment(ref _progressValue);
        OnProgressChanged();
    }
    #endregion
}
