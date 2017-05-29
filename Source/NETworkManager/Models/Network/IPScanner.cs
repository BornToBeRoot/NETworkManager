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
        public event EventHandler<IPScannerHostFoundArgs> HostFound;

        protected virtual void OnHostFound(IPScannerHostFoundArgs e)
        {
            HostFound?.Invoke(this, e);
        }

        public event EventHandler ScanComplete;

        protected virtual void OnScanComplete()
        {
            ScanComplete?.Invoke(this, System.EventArgs.Empty);
        }

        public event EventHandler<IPScannerProgressChangedArgs> ProgressChanged;

        protected virtual void OnProgressChanged()
        {
            ProgressChanged?.Invoke(this, new IPScannerProgressChangedArgs(progressValue));
        }

        public event EventHandler UserHasCanceled;

        protected virtual void OnUserHasCanceled()
        {
            UserHasCanceled?.Invoke(this, System.EventArgs.Empty);
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

                                        if (ipScannerOptions.ResolveMACAddress)
                                        {
                                            try
                                            {
                                                if (Dns.GetHostName() == hostname.Substring(0, hostname.IndexOf('.')))
                                                    macAddress = NetworkInterface.GetNetworkInterfaces().Where(p => p.IPv4Address.Contains(ipAddress)).FirstOrDefault().PhysicalAddress;
                                                else
                                                    macAddress = IPNetTableHelper.GetAllDevicesOnLAN().Where(p => p.Key.ToString() == ipAddress.ToString()).ToDictionary(p => p.Key, p => p.Value).First().Value;
                                            }
                                            catch { }
                                        }

                                        OnHostFound(new IPScannerHostFoundArgs(pingInfo, hostname, macAddress));

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
