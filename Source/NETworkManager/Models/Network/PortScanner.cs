using NETworkManager.Models.Lookup;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class PortScanner
    {
        #region Variables
        int progressValue;
        #endregion

        #region Events
        public event EventHandler<PortScannedArgs> PortScanned;

        protected virtual void OnPortScanned(PortScannedArgs e)
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
            ProgressChanged?.Invoke(this, new ProgressChangedArgs(progressValue));
        }

        public event EventHandler UserHasCanceled;

        protected virtual void OnUserHasCanceled()
        {
            UserHasCanceled?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Methods
        public void ScanAsync(List<Tuple<IPAddress, string>> hostData, int[] ports, PortScannerOptions portScannerOptions, CancellationToken cancellationToken)
        {
            progressValue = 0;

            // Modify the ThreadPool for better performance
            ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
            ThreadPool.SetMinThreads(workerThreads + portScannerOptions.Threads, completionPortThreads + portScannerOptions.Threads);

            Task.Run(() =>
            {

                try
                {
                    ParallelOptions parallelOptions = new ParallelOptions()
                    {
                        CancellationToken = cancellationToken,
                        MaxDegreeOfParallelism = portScannerOptions.Threads
                    };

                    foreach (Tuple<IPAddress, string> host in hostData)
                    {
                        // foreach ip, Parallel.ForEach port...
                        Parallel.ForEach(ports, parallelOptions, port =>
                        {
                            // Test if port is open
                            using (TcpClient tcpClient = host.Item1.AddressFamily == AddressFamily.InterNetworkV6 ? new TcpClient(AddressFamily.InterNetworkV6) : new TcpClient(AddressFamily.InterNetwork))
                            {
                                IAsyncResult tcpClientConnection = tcpClient.BeginConnect(host.Item1, port, null, null);

                                if (tcpClientConnection.AsyncWaitHandle.WaitOne(portScannerOptions.Timeout, false))
                                {
                                    try
                                    {
                                        tcpClient.EndConnect(tcpClientConnection);

                                        OnPortScanned(new PortScannedArgs(host, port, PortLookup.Lookup(port).FirstOrDefault(x => x.Protocol == PortLookup.Protocol.tcp), PortInfo.PortStatus.Open));
                                    }
                                    catch
                                    {
                                        if (portScannerOptions.ShowClosed)
                                            OnPortScanned(new PortScannedArgs(host, port, PortLookup.Lookup(port).FirstOrDefault(x => x.Protocol == PortLookup.Protocol.tcp), PortInfo.PortStatus.Closed));
                                    }
                                }
                                else
                                {
                                    if (portScannerOptions.ShowClosed)
                                        OnPortScanned(new PortScannedArgs(host, port, PortLookup.Lookup(port).FirstOrDefault(x => x.Protocol == PortLookup.Protocol.tcp), PortInfo.PortStatus.Closed));
                                }
                            }

                            // Increase the progress                        
                            Interlocked.Increment(ref progressValue);
                            OnProgressChanged();
                        });
                    }

                    OnScanComplete();
                }
                catch (OperationCanceledException) // If user has canceled
                {
                    // Check if the scan is already complete...
                    if ((ports.Length * hostData.Count) == progressValue)
                        OnScanComplete();
                    else
                        OnUserHasCanceled();
                }
                finally
                {
                    // Reset the ThreadPool to defaul
                    ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);
                    ThreadPool.SetMinThreads(workerThreads - portScannerOptions.Threads, completionPortThreads - portScannerOptions.Threads);
                }
            });
        }
        #endregion
    }
}
