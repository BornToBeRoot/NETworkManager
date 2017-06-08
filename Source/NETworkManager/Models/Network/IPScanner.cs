using NETworkManager.Helpers;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
namespace NETworkManager.Models.Network
{
    public class IPScanner
    {
        #region Variables
        int progressValue;
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
            ProgressChanged?.Invoke(this, new ProgressChangedArgs(progressValue));
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
            progressValue = 0;

            // Modify the ThreadPool for better performance
            int workerThreads;
            int completionPortThreads;

            ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);
            ThreadPool.SetMinThreads(workerThreads + ipScannerOptions.Threads, completionPortThreads + ipScannerOptions.Threads);

            // Start the scan in a separat task
            Task.Run(() =>
            {
                try
                {
                    ParallelOptions parallelOptions = new ParallelOptions();
                    parallelOptions.CancellationToken = cancellationToken;
                    parallelOptions.MaxDegreeOfParallelism = ipScannerOptions.Threads;

                    Parallel.ForEach(ipAddresses, parallelOptions, ipAddress =>
                    {
                        using (System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping())
                        {
                            for (int i = 0; i < ipScannerOptions.Attempts; i++)
                            {
                                try
                                {
                                    // PING
                                    PingReply pingReply = ping.Send(ipAddress, ipScannerOptions.Timeout, ipScannerOptions.Buffer);

                                    if (IPStatus.Success == pingReply.Status)
                                    {
                                        PingInfo pingInfo = new PingInfo(pingReply.Address, pingReply.Buffer.Count(), pingReply.RoundtripTime, pingReply.Options.Ttl, pingReply.Status);

                                        // DNS
                                        string hostname = string.Empty;

                                        if (ipScannerOptions.ResolveHostname)
                                        {
                                            if (pingInfo.Status == IPStatus.Success)
                                            {
                                                try
                                                {
                                                    hostname = Dns.GetHostEntry(ipAddress).HostName;
                                                }
                                                catch (SocketException) { } // Couldn't resolve hostname
                                            }
                                        }

                                        // ARP
                                        PhysicalAddress macAddress = null;
                                        string vendor = string.Empty;

                                        if (ipScannerOptions.ResolveMACAddress)
                                        {
                                            try
                                            {
                                                // Get mac
                                                if (Dns.GetHostName() == hostname.Substring(0, hostname.IndexOf('.')))
                                                    macAddress = NetworkInterface.GetNetworkInterfaces().Where(p => p.IPv4Address.Contains(ipAddress)).First().PhysicalAddress;
                                                else
                                                    macAddress = IPNetTableHelper.GetAllDevicesOnLAN().Where(p => p.Key.ToString() == ipAddress.ToString()).ToDictionary(p => p.Key, p => p.Value).First().Value;

                                                // Vendor lookup
                                                vendor = OUILookup.Lookup(macAddress.ToString()).FirstOrDefault().Vendor;
                                            }
                                            catch { }
                                        }

                                        OnHostFound(new HostFoundArgs(pingInfo, hostname, macAddress, vendor));

                                        break;
                                    }
                                }
                                catch { }

                                // Don't scan again, if the user has canceled (when more than 1 attempt)
                                if (cancellationToken.IsCancellationRequested)
                                    break;
                            }

                        }

                        // Increase the progress                        
                        Interlocked.Increment(ref progressValue);
                        OnProgressChanged();
                    });

                    OnScanComplete();
                }
                catch (OperationCanceledException)  // If user has canceled
                {
                    OnUserHasCanceled();
                }
            });

            // Reset the ThreadPool to default
            ThreadPool.SetMinThreads(workerThreads, completionPortThreads);
        }
        #endregion
    }
}
