using NETworkManager.Models.Lookup;
using NETworkManager.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

public sealed class IPScanner
{
    #region Variables

    private int _progressValue;

    private readonly IPScannerOptions _options;

    #endregion

    #region Events

    public event EventHandler<IPScannerHostScannedArgs> HostFound;

    private void OnHostFound(IPScannerHostScannedArgs e)
    {
        HostFound?.Invoke(this, e);
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

    public IPScanner(IPScannerOptions options)
    {
        _options = options;
    }

    #endregion

    #region Methods

    public void ScanAsync(IPAddress[] ipAddresses, CancellationToken cancellationToken)
    {
        // Start the scan in a separate task
        Task.Run(() =>
        {
            _progressValue = 0;

            // Get all network interfaces
            var networkInterfaces = _options.ResolveMACAddress
                ? NetworkInterface.GetNetworkInterfaces()
                : new List<NetworkInterfaceInfo>();

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

                // Start scan
                Parallel.ForEach(ipAddresses, hostParallelOptions, ipAddress =>
                {
                    // Start ping async
                    var pingTask = PingAsync(ipAddress, cancellationToken);

                    // Start port scan async
                    ConcurrentBag<PortInfo> portResults = new();

                    if (_options.PortScanEnabled)
                    {
                        Parallel.ForEach(_options.PortScanPorts, portParallelOptions, port =>
                        {
                            // Test if port is open
                            using var tcpClient = new TcpClient(ipAddress.AddressFamily);

                            var portState = PortState.None;

                            try
                            {
                                var task = tcpClient.ConnectAsync(ipAddress, port);

                                if (task.Wait(_options.PortScanTimeout))
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

                                if (portState == PortState.Open || _options.ShowAllResults)
                                    portResults.Add(
                                        new PortInfo(port, PortLookup.LookupByPortAndProtocol(port), portState));
                            }
                        });
                    }

                    // Get ping result
                    pingTask.Wait();
                    
                    cancellationToken.ThrowIfCancellationRequested();

                    var pingInfo = pingTask.Result;

                    // Check if host is up
                    var isAnyPortOpen = portResults.Any(x => x.State == PortState.Open);
                    var isReachable = pingInfo.Status == IPStatus.Success || isAnyPortOpen;

                    // DNS & ARP
                    if (isReachable || _options.ShowAllResults)
                    {
                        // DNS
                        var hostname = string.Empty;

                        if (_options.ResolveHostname)
                        {
                            // Don't use await in Parallel.ForEach, this will break
                            var dnsResolverTask = DNSClient.GetInstance().ResolvePtrAsync(ipAddress);

                            // Wait for task inside a Parallel.Foreach
                            dnsResolverTask.Wait();

                            if (!dnsResolverTask.Result.HasError)
                                hostname = dnsResolverTask.Result.Value;
                            else
                                hostname = _options.DNSShowErrorMessage
                                    ? dnsResolverTask.Result.ErrorMessage
                                    : string.Empty;
                        }

                        // ARP
                        PhysicalAddress macAddress = null;
                        var vendor = string.Empty;

                        if (_options.ResolveMACAddress)
                        {
                            // Get info from arp table
                            var arpTableInfo = ARP.GetTable()
                                .FirstOrDefault(p => p.IPAddress.ToString() == ipAddress.ToString());

                            if (arpTableInfo != null)
                                macAddress = arpTableInfo.MACAddress;

                            // Check if it is the local mac
                            if (macAddress == null)
                            {
                                var networkInterfaceInfo = networkInterfaces.FirstOrDefault(p =>
                                    p.IPv4Address.Any(x => x.Item1.Equals(ipAddress)));

                                if (networkInterfaceInfo != null)
                                    macAddress = networkInterfaceInfo.PhysicalAddress;
                            }

                            // Vendor lookup
                            if (macAddress != null)
                            {
                                var info = OUILookup.LookupByMacAddress(macAddress.ToString()).FirstOrDefault();

                                if (info != null)
                                    vendor = info.Vendor;
                            }
                        }

                        OnHostFound(new IPScannerHostScannedArgs(
                            new IPScannerHostInfo(
                                isReachable, pingInfo, isAnyPortOpen, portResults.OrderBy(x => x.Port).ToList(),
                                hostname, macAddress, vendor)));
                    }

                    IncreaseProgress();
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

    private Task<PingInfo> PingAsync(IPAddress ipAddress, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            using var ping = new System.Net.NetworkInformation.Ping();

            for (var i = 0; i < _options.ICMPAttempts; i++)
            {
                try
                {
                    // Get timestamp 
                    var timestamp = DateTime.Now;
                    
                    var pingReply = ping.Send(ipAddress, _options.ICMPTimeout, _options.ICMPBuffer);

                    // Success
                    if (pingReply is { Status: IPStatus.Success })
                    {
                        // IPv4
                        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                            return new PingInfo(timestamp, pingReply.Address, pingReply.Buffer.Length, pingReply.RoundtripTime,
                                pingReply.Options!.Ttl, pingReply.Status);
                        
                        // IPv6
                        return new PingInfo(timestamp, pingReply.Address, pingReply.Buffer.Length, pingReply.RoundtripTime,
                            pingReply.Status);
                    }

                    // Failed
                    if (pingReply != null)
                        return new PingInfo(timestamp, ipAddress, pingReply.Status);
                }
                catch (PingException)
                {
                    
                }

                // Don't scan again, if the user has canceled (when more than 1 attempt)
                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            return new PingInfo();
        });
    }

    private void IncreaseProgress()
    {
        // Increase the progress                        
        Interlocked.Increment(ref _progressValue);
        OnProgressChanged();
    }

    #endregion
}