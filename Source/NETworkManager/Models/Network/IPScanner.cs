using NETworkManager.Models.Lookup;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
namespace NETworkManager.Models.Network
{
    public class IPScanner
    {
        #region Variables
        private int _progressValue;
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

        #region Methods
        public void ScanAsync(IPAddress[] ipAddresses, IPScannerOptions ipScannerOptions, CancellationToken cancellationToken)
        {
            // Start the scan in a separat task
            Task.Run(() =>
            {
                _progressValue = 0;

                // Modify the ThreadPool for better performance
                ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
                ThreadPool.SetMinThreads(workerThreads + ipScannerOptions.Threads, completionPortThreads + ipScannerOptions.Threads);

                try
                {
                    var parallelOptions = new ParallelOptions
                    {
                        CancellationToken = cancellationToken,
                        MaxDegreeOfParallelism = ipScannerOptions.Threads
                    };

                    Parallel.ForEach(ipAddresses, parallelOptions, ipAddress =>
                    {
                        var pingInfo = new PingInfo();
                        var pingable = false;

                        // PING
                        using (var ping = new System.Net.NetworkInformation.Ping())
                        {
                            for (int i = 0; i < ipScannerOptions.ICMPAttempts; i++)
                            {
                                try
                                {
                                    var pingReply = ping.Send(ipAddress, ipScannerOptions.ICMPTimeout, ipScannerOptions.ICMPBuffer);

                                    if (pingReply != null && IPStatus.Success == pingReply.Status)
                                    {
                                        pingInfo = new PingInfo(pingReply.Address, pingReply.Buffer.Count(), pingReply.RoundtripTime, pingReply.Options.Ttl, pingReply.Status);

                                        pingable = true;
                                        break; // Continue with the next checks...
                                    }

                                    if (pingReply != null)
                                        pingInfo = new PingInfo(ipAddress, pingReply.Status);
                                }
                                catch (PingException)
                                {

                                }

                                // Don't scan again, if the user has canceled (when more than 1 attempt)
                                if (cancellationToken.IsCancellationRequested)
                                    break;
                            }
                        }

                        if (pingable || ipScannerOptions.ShowScanResultForAllIPAddresses)
                        {
                            // DNS
                            var hostname = string.Empty;

                            if (ipScannerOptions.ResolveHostname)
                            {
                                var options = new DNSLookupOptions()
                                {
                                    UseCustomDNSServer = ipScannerOptions.UseCustomDNSServer,
                                    CustomDNSServers = ipScannerOptions.CustomDNSServer,
                                    Port = ipScannerOptions.DNSPort,
                                    Attempts = ipScannerOptions.DNSAttempts,
                                    Timeout = ipScannerOptions.DNSTimeout,
                                    TransportType = ipScannerOptions.DNSTransportType,
                                    UseResolverCache = ipScannerOptions.DNSUseResolverCache,
                                    Recursion = ipScannerOptions.DNSRecursion,
                                };

                                hostname = DNSLookup.ResolvePTR(ipAddress, options).Item2.FirstOrDefault();
                            }

                            // ARP
                            PhysicalAddress macAddress = null;
                            var vendor = string.Empty;

                            if (ipScannerOptions.ResolveMACAddress)
                            {
                                // Get info from arp table
                                var arpTableInfo = ARPTable.GetTable().FirstOrDefault(p => p.IPAddress.ToString() == ipAddress.ToString());

                                if (arpTableInfo != null)
                                    macAddress = arpTableInfo.MACAddress;

                                // Check if it is the local mac
                                if (macAddress == null)
                                {
                                    var networkInferfaceInfo = NetworkInterface.GetNetworkInterfaces().FirstOrDefault(p => p.IPv4Address.Contains(ipAddress));

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

                            OnHostFound(new HostFoundArgs(pingInfo, hostname, macAddress, vendor));
                        }

                        IncreaseProcess();
                    });

                    OnScanComplete();
                }
                catch (OperationCanceledException)  // If user has canceled
                {
                    // Check if the scan is already complete...
                    if (ipAddresses.Length == _progressValue)
                        OnScanComplete();
                    else
                        OnUserHasCanceled();
                }
                finally
                {
                    // Reset the ThreadPool to default
                    ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);
                    ThreadPool.SetMinThreads(workerThreads - ipScannerOptions.Threads, completionPortThreads - ipScannerOptions.Threads);
                }
            }, cancellationToken);
        }

        private void IncreaseProcess()
        {
            // Increase the progress                        
            Interlocked.Increment(ref _progressValue);
            OnProgressChanged();
        }
        #endregion
    }
}
