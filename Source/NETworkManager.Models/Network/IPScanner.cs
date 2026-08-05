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

    private int _progressValue;

    #endregion

    #region Events

    /// <summary>
    ///     Occurs when a host has been scanned.
    /// </summary>
    public event EventHandler<IPScannerHostScannedArgs> HostScanned;

    private void OnHostScanned(IPScannerHostScannedArgs e)
    {
        HostScanned?.Invoke(this, e);
    }

    /// <summary>
    ///     Occurs when the scan is complete.
    /// </summary>
    public event EventHandler ScanComplete;

    private void OnScanComplete()
    {
        ScanComplete?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Occurs when the scan progress has changed.
    /// </summary>
    public event EventHandler<ProgressChangedArgs> ProgressChanged;

    private void OnProgressChanged()
    {
        ProgressChanged?.Invoke(this, new ProgressChangedArgs(_progressValue));
    }

    /// <summary>
    ///     Occurs when the user has canceled the scan.
    /// </summary>
    public event EventHandler UserHasCanceled;

    private void OnUserHasCanceled()
    {
        UserHasCanceled?.Invoke(this, EventArgs.Empty);
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
        // Start the scan in a separate task
        Task.Run(async () =>
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
                await Parallel.ForEachAsync(hosts, hostParallelOptions, async (host, ct) =>
                {
                    // Start ping, port scan and netbios lookup concurrently - none of these block a thread anymore
                    var pingTask = PingAsync(host.ipAddress, ct);

                    var portScanTask = options.PortScanEnabled
                        ? PortScanAsync(host.ipAddress, portScanParallelOptions, ct)
                        : Task.FromResult(new List<PortInfo>());

                    var netbiosTask = options.NetBIOSEnabled
                        ? NetBIOSResolver.ResolveAsync(host.ipAddress, options.NetBIOSTimeout, ct)
                        : Task.FromResult(new NetBIOSInfo(host.ipAddress));

                    await Task.WhenAll(pingTask, portScanTask, netbiosTask).ConfigureAwait(false);

                    var pingInfo = pingTask.Result;
                    var portScanResults = portScanTask.Result;
                    var netBIOSInfo = netbiosTask.Result;

                    // Cancel if the user has canceled
                    ct.ThrowIfCancellationRequested();

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
                            var dnsResult = await DNSClient.GetInstance().ResolvePtrAsync(host.ipAddress)
                                .WaitAsync(ct).ConfigureAwait(false);

                            if (!dnsResult.HasError)
                                dnsHostname = dnsResult.Value;
                        }

                        // ARP
                        var arpMACAddress = string.Empty;
                        var arpVendor = string.Empty;

                        if (options.ResolveMACAddress)
                        {
                            // Get info from neighbor table
                            arpMACAddress = NeighborTable.GetMACAddress(host.ipAddress);

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
                                    // ARP/NDP is preferred, fallback to NetBIOS
                                    !string.IsNullOrEmpty(arpMACAddress)
                                        ? arpMACAddress
                                        : netBIOSInfo?.MACAddress ?? string.Empty,
                                    !string.IsNullOrEmpty(arpMACAddress)
                                        ? arpVendor
                                        : netBIOSInfo?.Vendor ?? string.Empty
                                )
                            )
                        );
                    }

                    IncreaseProgress();
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

    private async Task<PingInfo> PingAsync(IPAddress ipAddress, CancellationToken cancellationToken)
    {
        using var ping = new System.Net.NetworkInformation.Ping();

        for (var i = 0; i < options.ICMPAttempts; i++)
        {
            // Get timestamp
            var timestamp = DateTime.Now;

            try
            {
                // Note: the CancellationToken-accepting overload requires a TimeSpan timeout,
                // unlike the legacy int-based overloads used elsewhere in .NET's Ping API.
                var pingReply = await ping.SendPingAsync(ipAddress, TimeSpan.FromMilliseconds(options.ICMPTimeout),
                    options.ICMPBuffer, cancellationToken: cancellationToken).ConfigureAwait(false);

                // Success
                if (pingReply is { Status: IPStatus.Success })
                {
                    switch (ipAddress.AddressFamily)
                    {
                        case AddressFamily.InterNetwork:
                            return new PingInfo(
                                                         timestamp,
                                                         pingReply.Address,
                                                         pingReply.Buffer.Length,
                                                         pingReply.RoundtripTime,
                                                         pingReply.Options!.Ttl,
                                                         pingReply.Status);
                        case AddressFamily.InterNetworkV6:
                            return new PingInfo(
                                                     timestamp,
                                                     pingReply.Address,
                                                     pingReply.Buffer.Length,
                                                     pingReply.RoundtripTime,
                                                     pingReply.Status);
                    }
                }

                // Failed
                if (pingReply != null)
                    return new PingInfo(timestamp, ipAddress, pingReply.Status);
            }
            catch (PingException)
            {
                // Ping failed with unknown status
                return new PingInfo(timestamp, ipAddress, IPStatus.Unknown);
            }

            // Don't scan again, if the user has canceled (when more than 1 attempt)
            if (cancellationToken.IsCancellationRequested)
                break;
        }

        // Fall back to unknown status
        return new PingInfo(DateTime.Now, ipAddress, IPStatus.Unknown);
    }

    private async Task<List<PortInfo>> PortScanAsync(IPAddress ipAddress, ParallelOptions parallelOptions,
        CancellationToken cancellationToken)
    {
        ConcurrentBag<PortInfo> results = [];

        await Parallel.ForEachAsync(options.PortScanPorts, parallelOptions, async (port, ct) =>
        {
            var portState = await PortProbe.ProbeAsync(ipAddress, port, options.PortScanTimeout, ct)
                .ConfigureAwait(false);

            if (portState == PortState.Open || options.ShowAllResults)
                results.Add(new PortInfo(port, PortLookup.LookupByPortAndProtocol(port), portState));
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