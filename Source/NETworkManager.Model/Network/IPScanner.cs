using NETworkManager.Utilities.Network;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NETworkManager.Model.Common;

namespace NETworkManager.Model.Network
{
    public class IPScanner
    {
        int progressValue;

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
                        // PING
                        PingInfo pingInfo = new PingInfo();

                        for (int i = 0; i < ipScannerOptions.Attempts; i++)
                        {
                            try
                            {
                                throw new Exception();
                               // pingInfo = Network.Ping.Send(ipAddress, ipScannerOptions.Timeout, ipScannerOptions.Buffer);

                                if (IPStatus.Success == pingInfo.Status)
                                    break;
                            }
                            catch { }
                        }

                        if (pingInfo.Status == IPStatus.Success)
                        {
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
                                    catch (SocketException) { } // Couldn't resolve hostname for that address
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

                            OnHostFound(new HostFoundArgs(pingInfo, hostname, macAddress));
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
