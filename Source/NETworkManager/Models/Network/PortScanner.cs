using NETworkManager.Models.Lookup;
using System;
using System.Collections.Generic;
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
        private int _progressValue;
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
            ProgressChanged?.Invoke(this, new ProgressChangedArgs(_progressValue));
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
            _progressValue = 0;

            // Modify the ThreadPool for better performance
            ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);
            ThreadPool.SetMinThreads(workerThreads + portScannerOptions.Threads, completionPortThreads + portScannerOptions.Threads);

            Task.Run(() =>
            {

                try
                {
                    var parallelOptions = new ParallelOptions()
                    {
                        CancellationToken = cancellationToken,
                        MaxDegreeOfParallelism = portScannerOptions.Threads
                    };

                    foreach (var host in hostData)
                    {
                        // foreach ip, Parallel.ForEach port...
                        Parallel.ForEach(ports, parallelOptions, port =>
                        {
                            // Test if port is open
                            using (var tcpClient = host.Item1.AddressFamily == AddressFamily.InterNetworkV6 ? new TcpClient(AddressFamily.InterNetworkV6) : new TcpClient(AddressFamily.InterNetwork))
                            {
                                var tcpClientConnection = tcpClient.BeginConnect(host.Item1, port, null, null);

                                if (tcpClientConnection.AsyncWaitHandle.WaitOne(portScannerOptions.Timeout, false))
                                {
                                    try
                                    {
                                        tcpClient.EndConnect(tcpClientConnection);

                                        OnPortScanned(new PortScannedArgs(host, port, PortLookup.Lookup(port).FirstOrDefault(x => x.Protocol == PortLookup.Protocol.Tcp), PortInfo.PortStatus.Open));
                                    }
                                    catch
                                    {
                                        if (portScannerOptions.ShowClosed)
                                            OnPortScanned(new PortScannedArgs(host, port, PortLookup.Lookup(port).FirstOrDefault(x => x.Protocol == PortLookup.Protocol.Tcp), PortInfo.PortStatus.Closed));
                                    }
                                }
                                else
                                {
                                    if (portScannerOptions.ShowClosed)
                                        OnPortScanned(new PortScannedArgs(host, port, PortLookup.Lookup(port).FirstOrDefault(x => x.Protocol == PortLookup.Protocol.Tcp), PortInfo.PortStatus.Closed));
                                }
                            }

                            // Increase the progress                        
                            Interlocked.Increment(ref _progressValue);
                            OnProgressChanged();
                        });
                    }

                    OnScanComplete();
                }
                catch (OperationCanceledException) // If user has canceled
                {
                    // Check if the scan is already complete...
                    if ((ports.Length * hostData.Count) == _progressValue)
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
            }, cancellationToken);
        }
        #endregion
    }
}
