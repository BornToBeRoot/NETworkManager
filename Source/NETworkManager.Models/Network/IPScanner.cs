using NETworkManager.Models.Lookup;
using NETworkManager.Utilities;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

public class IPScanner
{
    #region Variables
    private int _progressValue;

    private readonly IPScannerOptions _options;       
    #endregion

    #region Events
    public event EventHandler<HostFoundArgs> HostFound;

    protected virtual void OnHostFound(HostFoundArgs e)
    {
        HostFound?.Invoke(this, e);
    }

    public event EventHandler ScanComplete;

    protected virtual void OnScanComplete()
    {
        ScanComplete?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<ProgressChangedArgs> ProgressChanged;

    protected virtual void OnProgressChanged()
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
    public IPScanner(IPScannerOptions options)
    {
        _options = options;
    }
    #endregion

    #region Methods
    public void ScanAsync(IPAddress[] ipAddresses, CancellationToken cancellationToken)
    {
        // Start the scan in a separat task
        Task.Run(() =>
        {
            _progressValue = 0;

            try
            {
                var parallelOptions = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = _options.MaxHostThreads
                };

                Parallel.ForEach(ipAddresses, parallelOptions, ipAddress =>
                 {
                     var isReachable = false;

                     var pingInfo = new PingInfo();

                     // PING
                     using (var ping = new System.Net.NetworkInformation.Ping())
                     {
                         for (var i = 0; i < _options.ICMPAttempts; i++)
                         {
                             try
                             {
                                 var pingReply = ping.Send(ipAddress, _options.ICMPTimeout, _options.ICMPBuffer);

                                 if (pingReply != null && IPStatus.Success == pingReply.Status)
                                 {
                                     if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                         pingInfo = new PingInfo(pingReply.Address, pingReply.Buffer.Length, pingReply.RoundtripTime, pingReply.Options.Ttl, pingReply.Status);
                                     else
                                         pingInfo = new PingInfo(pingReply.Address, pingReply.Buffer.Length, pingReply.RoundtripTime, pingReply.Status);

                                     isReachable = true;
                                     break; // Continue with the next checks...
                                 }

                                 if (pingReply != null)
                                     pingInfo = new PingInfo(ipAddress, pingReply.Status);
                             }
                             catch (PingException)
                             { }

                             // Don't scan again, if the user has canceled (when more than 1 attempt)
                             if (cancellationToken.IsCancellationRequested)
                                 break;
                         }
                     }

                     // Port scan



                     // DNS & ARP
                     if (isReachable || _options.ShowAllResults)
                     {
                         // DNS
                         var hostname = string.Empty;

                         if (_options.ResolveHostname)
                         {
                             // Don't use await in Paralle.ForEach, this will break
                             var dnsResolverTask = DNSClient.GetInstance().ResolvePtrAsync(ipAddress);

                             // Wait for task inside a Parallel.Foreach
                             dnsResolverTask.Wait();

                             if (!dnsResolverTask.Result.HasError)
                                 hostname = dnsResolverTask.Result.Value;
                             else
                                 hostname = _options.DNSShowErrorMessage ? dnsResolverTask.Result.ErrorMessage : string.Empty;
                         }

                         // ARP
                         PhysicalAddress macAddress = null;
                         var vendor = string.Empty;

                         if (_options.ResolveMACAddress)
                         {
                             // Get info from arp table
                             var arpTableInfo = ARP.GetTable().FirstOrDefault(p => p.IPAddress.ToString() == ipAddress.ToString());

                             if (arpTableInfo != null)
                                 macAddress = arpTableInfo.MACAddress;

                             // Check if it is the local mac
                             if (macAddress == null)
                             {
                                 var networkInferfaceInfo = NetworkInterface.GetNetworkInterfaces().FirstOrDefault(p => p.IPv4Address.Any(x => x.Item1.Equals(ipAddress)));

                                 if (networkInferfaceInfo != null)
                                     macAddress = networkInferfaceInfo.PhysicalAddress;
                             }

                             // Vendor lookup
                             if (macAddress != null)
                             {
                                 var info = OUILookup.Lookup(macAddress.ToString()).FirstOrDefault();

                                 if (info != null)
                                     vendor = info.Vendor;
                             }
                         }

                         OnHostFound(new HostFoundArgs(isReachable, pingInfo, hostname, macAddress, vendor));
                     }

                     IncreaseProgess();
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
