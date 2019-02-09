using NETworkManager.Models.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using TransportType = Heijden.DNS.TransportType;

namespace NETworkManager.Models.Network
{
    public class IPScanner
    {
        #region Variables
        private int _progressValue;

        public int Threads = 256;
        public int ICMPTimeout = 4000;
        public byte[] ICMPBuffer = new byte[32];
        public int ICMPAttempts = 2;
        public bool ResolveHostname = true;
        public bool UseCustomDNSServer = false;
        public List<string> CustomDNSServer = new List<string>();
        public int DNSPort = 53;
        public int DNSAttempts = 2;
        public int DNSTimeout = 2000;
        public bool ResolveMACAddress = false;
        public bool ShowScanResultForAllIPAddresses = false;
        public TransportType DNSTransportType = TransportType.Udp;
        public bool DNSUseResolverCache = false;
        public bool DNSRecursion = false;
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
        public void ScanAsync(IPAddress[] ipAddresses, CancellationToken cancellationToken)
        {
            // Start the scan in a separat task
            Task.Run(() =>
            {
                _progressValue = 0;

                // Modify the ThreadPool for better performance
                ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);
                ThreadPool.SetMinThreads(workerThreads + Threads, completionPortThreads + Threads);

                try
                {
                    var parallelOptions = new ParallelOptions
                    {
                        CancellationToken = cancellationToken,
                        MaxDegreeOfParallelism = Threads
                    };

                    Parallel.ForEach(ipAddresses, parallelOptions, ipAddress =>
                    {
                        var pingInfo = new PingInfo();
                        var pingable = false;

                        // PING
                        using (var ping = new System.Net.NetworkInformation.Ping())
                        {
                            for (var i = 0; i < ICMPAttempts; i++)
                            {
                                try
                                {
                                    var pingReply = ping.Send(ipAddress, ICMPTimeout, ICMPBuffer);

                                    if (pingReply != null && IPStatus.Success == pingReply.Status)
                                    {
                                        pingInfo = new PingInfo(pingReply.Address, pingReply.Buffer.Length, pingReply.RoundtripTime, pingReply.Options.Ttl, pingReply.Status);

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

                        if (pingable || ShowScanResultForAllIPAddresses)
                        {
                            // DNS
                            var hostname = string.Empty;

                            if (ResolveHostname)
                            {
                                var dnsLookup = new DNSLookup
                                {
                                    UseCustomDNSServer = UseCustomDNSServer,
                                    CustomDNSServers = CustomDNSServer,
                                    Port = DNSPort,
                                    Attempts = DNSAttempts,
                                    Timeout = DNSTimeout,
                                    TransportType = DNSTransportType,
                                    UseResolverCache = DNSUseResolverCache,
                                    Recursion = DNSRecursion
                                };
                                
                                hostname = dnsLookup.ResolvePTR(ipAddress).Item2.FirstOrDefault();
                            }

                            // ARP
                            PhysicalAddress macAddress = null;
                            var vendor = string.Empty;

                            if (ResolveMACAddress)
                            {
                                // Get info from arp table
                                var arpTableInfo = ARP.GetTable().FirstOrDefault(p => p.IPAddress.ToString() == ipAddress.ToString());

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
                    ThreadPool.SetMinThreads(workerThreads - Threads, completionPortThreads - Threads);
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
