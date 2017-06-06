using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
        public void ScanAsync(IPAddress[] ipAddresses, int[] ports, PortScannerOptions portScannerOptions, CancellationToken cancellationToken)
        {
            progressValue = 0;

            // Modify the ThreadPool for better performance
            int workerThreads;
            int completionPortThreads;

            ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);
            ThreadPool.SetMinThreads(workerThreads + portScannerOptions.Threads, completionPortThreads + portScannerOptions.Threads);

            Task.Run(() =>
            {
                try
                {
                    ParallelOptions parallelOptions = new ParallelOptions();
                    parallelOptions.CancellationToken = cancellationToken;
                    parallelOptions.MaxDegreeOfParallelism = portScannerOptions.Threads;

                    // foreach ip, Parallel.ForEach port...
                    foreach(IPAddress ipAddress in ipAddresses)
                    {
                        Parallel.ForEach(ports, parallelOptions, port =>
                        {
                            // Do some shit...
                        });
                    }
                }
                catch(OperationCanceledException) // If user has canceled
                {
                    OnUserHasCanceled();
                }
            });

            // Reset the ThreadPool to defaul
            ThreadPool.SetMinThreads(workerThreads, completionPortThreads);
        }
        #endregion
    }
}
