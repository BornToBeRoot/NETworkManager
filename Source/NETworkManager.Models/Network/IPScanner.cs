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

public sealed class IPScanner(IPScannerOptions options)
{
    #region Variables

    private int _progressValue;

    #endregion

    #region Events

    public event EventHandler<IPScannerHostScannedArgs> HostScanned;

    private void OnHostScanned(IPScannerHostScannedArgs e)
    {
        HostScanned?.Invoke(this, e);
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

    public void ScanAsync(IEnumerable<(IPAddress ipAddress, string hostname)> hosts,
        CancellationToken cancellationToken)
    {
        // Start the scan in a separate task
        Task.Run(() =>
        {
            _progressValue = 0;

            // Get all network interfaces (for local mac address lookup)
            var networkInterfaces = options.ResolveMACAddress ? NetworkInterface.GetNetworkInterfaces() : [];

            try
            {
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

                // Start scan
                Parallel.ForEach(hosts, hostParallelOptions, host =>
                {
                    // Start ping async
                    var pingTask = PingAsync(host.ipAddress, cancellationToken);

                    // Start port scan async (if enabled)
                    var portScanTask = options.PortScanEnabled
                        ? PortScanAsync(host.ipAddress, portScanParallelOptions, cancellationToken)
                        : Task.FromResult(Enumerable.Empty<PortInfo>());

                    // Start netbios lookup async (if enabled)
                    var netbiosTask = options.NetBIOSEnabled
                        ? NetBIOSResolver.ResolveAsync(host.ipAddress, options.NetBIOSTimeout, cancellationToken)
                        : Task.FromResult(new NetBIOSInfo());

                    // Get ping result
                    pingTask.Wait(cancellationToken);
                    var pingInfo = pingTask.Result;

                    // Get port scan result
                    portScanTask.Wait(cancellationToken);
                    var portScanResults = portScanTask.Result.ToList();

                    // Get netbios result
                    netbiosTask.Wait(cancellationToken);
                    var netBIOSInfo = netbiosTask.Result;

                    // Cancel if the user has canceled
                    cancellationToken.ThrowIfCancellationRequested();

                    // Check if host is up
                    var isAnyPortOpen = portScanResults.Any(x => x.State == PortState.Open);
                    var isReachable = pingInfo.Status == IPStatus.Success || // ICMP response
                                      isAnyPortOpen || // Any port is open   
                                      netBIOSInfo.IsReachable; // NetBIOS response

                    // DNS & ARP
                    if (isReachable || options.ShowAllResults)
                    {
                        // DNS
                        var dnsHostname = string.Empty;

                        if (options.ResolveHostname)
                        {
                            // Don't use await in Parallel.ForEach, this will break
                            var dnsResolverTask = DNSClient.GetInstance().ResolvePtrAsync(host.ipAddress);

                            // Wait for task inside a Parallel.Foreach
                            dnsResolverTask.Wait(cancellationToken);

                            if (!dnsResolverTask.Result.HasError)
                                dnsHostname = dnsResolverTask.Result.Value;
                        }

                        // ARP
                        var arpMACAddress = string.Empty;
                        var arpVendor = string.Empty;

                        if (options.ResolveMACAddress)
                        {
                            // Get info from arp table
                            arpMACAddress = ARP.GetMACAddress(host.ipAddress);

                            // Check if it is the local mac
                            if (string.IsNullOrEmpty(arpMACAddress))
                            {
                                var networkInterfaceInfo = networkInterfaces.FirstOrDefault(p =>
                                    p.IPv4Address.Any(x => x.Item1.Equals(host.ipAddress)));

                                if (networkInterfaceInfo != null)
                                    arpMACAddress = networkInterfaceInfo.PhysicalAddress.ToString();
                            }

                            // Vendor lookup & default format
                            if (!string.IsNullOrEmpty(arpMACAddress))
                            {
                                var info = OUILookup.LookupByMacAddress(arpMACAddress).FirstOrDefault();

                                if (info != null)
                                    arpVendor = info.Vendor;

                                // Apply default format
                                arpMACAddress = MACAddressHelper.GetDefaultFormat(arpMACAddress);
                            }
                        }

                        OnHostScanned(new IPScannerHostScannedArgs(
                                new IPScannerHostInfo(
                                    isReachable,
                                    pingInfo,
                                    // DNS is default, fallback to netbios
                                    !string.IsNullOrEmpty(dnsHostname)
                                        ? dnsHostname
                                        : netBIOSInfo?.ComputerName ?? string.Empty,
                                    dnsHostname,
                                    isAnyPortOpen,
                                    portScanResults.OrderBy(x => x.Port).ToList(),
                                    netBIOSInfo,
                                    // ARP is default, fallback to netbios
                                    !string.IsNullOrEmpty(arpMACAddress)
                                        ? arpMACAddress
                                        : netBIOSInfo?.MACAddress ?? string.Empty,
                                    !string.IsNullOrEmpty(arpMACAddress)
                                        ? arpVendor
                                        : netBIOSInfo?.Vendor ?? string.Empty,
                                    arpMACAddress,
                                    arpVendor
                                )
                            )
                        );
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

            for (var i = 0; i < options.ICMPAttempts; i++)
            {
                try
                {
                    // Get timestamp 
                    var timestamp = DateTime.Now;

                    var pingReply = ping.Send(ipAddress, options.ICMPTimeout, options.ICMPBuffer);

                    // Success
                    if (pingReply is { Status: IPStatus.Success })
                    {
                        // IPv4
                        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                            return new PingInfo(timestamp, pingReply.Address, pingReply.Buffer.Length,
                                pingReply.RoundtripTime,
                                pingReply.Options!.Ttl, pingReply.Status);

                        // IPv6
                        return new PingInfo(timestamp, pingReply.Address, pingReply.Buffer.Length,
                            pingReply.RoundtripTime,
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
        }, cancellationToken);
    }

    private Task<IEnumerable<PortInfo>> PortScanAsync(IPAddress ipAddress, ParallelOptions parallelOptions,
        CancellationToken cancellationToken)
    {
        ConcurrentBag<PortInfo> results = [];

        Parallel.ForEach(options.PortScanPorts, parallelOptions, port =>
        {
            // Test if port is open
            using var tcpClient = new TcpClient(ipAddress.AddressFamily);

            var portState = PortState.None;

            try
            {
                // ReSharper disable once MethodSupportsCancellation - Wait for timeout
                var task = tcpClient.ConnectAsync(ipAddress, port);

                if (task.Wait(options.PortScanTimeout, cancellationToken))
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

                if (portState == PortState.Open || options.ShowAllResults)
                    results.Add(
                        new PortInfo(port, PortLookup.LookupByPortAndProtocol(port), portState));
            }
        });

        return Task.FromResult(results.AsEnumerable());
    }

    private void IncreaseProgress()
    {
        // Increase the progress                        
        Interlocked.Increment(ref _progressValue);
        OnProgressChanged();
    }

    #endregion
}