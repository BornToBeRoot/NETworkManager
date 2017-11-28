using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class Traceroute
    {
        #region Events
        public event EventHandler<TracerouteHopReceivedArgs> HopReceived;
        protected virtual void OnHopReceived(TracerouteHopReceivedArgs e)
        {
            HopReceived?.Invoke(this, e);
        }

        public event EventHandler TraceComplete;
        protected virtual void OnTraceComplete()
        {
            TraceComplete?.Invoke(this, System.EventArgs.Empty);
        }

        public event EventHandler<MaximumHopsReachedArgs> MaximumHopsReached;
        protected virtual void OnMaximumHopsReached(MaximumHopsReachedArgs e)
        {
            MaximumHopsReached?.Invoke(this, e);
        }

        public event EventHandler UserHasCanceled;
        protected virtual void OnUserHasCanceled()
        {
            UserHasCanceled?.Invoke(this, System.EventArgs.Empty);
        }
        #endregion

        #region Methods
        public void TraceAsync(IPAddress ipAddress, TracerouteOptions traceOptions, CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                byte[] buffer = new byte[traceOptions.Buffer];

                int maximumHops = traceOptions.MaximumHops;                
                bool maximumHopsReached = false;

                int pingCount = 3;

                // Get the ttl of the ip
                using (System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping())
                {
                    PingReply pingReply;

                    for (int i = 0; i < pingCount; i++)
                    {
                        pingReply = ping.Send(ipAddress, traceOptions.Timeout, buffer, new System.Net.NetworkInformation.PingOptions() { Ttl = 64, DontFragment = traceOptions.DontFragement });

                        if (pingReply.Status == IPStatus.Success)
                        {
                            int ttl = 64 - pingReply.Options.Ttl;

                            if (ttl < maximumHops)
                                maximumHops = ttl;
                            else
                                maximumHopsReached = true;

                            break;
                        }
                    }
                }

                int threads = (pingCount * maximumHops);

                // Modify the ThreadPool for better performance
                ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
                ThreadPool.SetMinThreads(workerThreads + threads, completionPortThreads + threads);

                // Async check all hops
                try
                {
                    ParallelOptions parallelOptions = new ParallelOptions()
                    {
                        CancellationToken = cancellationToken,
                        MaxDegreeOfParallelism = threads
                    };

                    Parallel.For(1, maximumHops + 1, parallelOptions, i =>
                    {
                        List<Task<Tuple<PingReply, long>>> tasks = new List<Task<Tuple<PingReply, long>>>();

                        for (int y = 0; y < 3; y++)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                Stopwatch stopwatch = new Stopwatch();

                                PingReply pingReply;

                                using (System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping())
                                {
                                    stopwatch.Start();

                                    pingReply = ping.Send(ipAddress, traceOptions.Timeout, buffer, new System.Net.NetworkInformation.PingOptions() { Ttl = i, DontFragment = traceOptions.DontFragement });

                                    stopwatch.Stop();
                                }

                                return Tuple.Create(pingReply, stopwatch.ElapsedMilliseconds);
                            }));
                        }

                        Task.WaitAll(tasks.ToArray());

                        IPAddress ipAddressHop = tasks.FirstOrDefault(x => x.Result.Item1 != null).Result.Item1.Address;

                        string hostname = string.Empty;

                        try
                        {
                            if (ipAddressHop != null)
                                hostname = Dns.GetHostEntry(ipAddressHop).HostName;
                        }
                        catch (SocketException) { } // Couldn't resolve hostname

                        OnHopReceived(new TracerouteHopReceivedArgs(i, tasks[0].Result.Item2, tasks[1].Result.Item2, tasks[2].Result.Item2, ipAddressHop, hostname, tasks[0].Result.Item1.Status, tasks[1].Result.Item1.Status, tasks[2].Result.Item1.Status));
                    });
                }
                catch (OperationCanceledException)
                {
                    OnUserHasCanceled();
                    return;
                }

               if (maximumHopsReached)
                    OnMaximumHopsReached(new MaximumHopsReachedArgs(traceOptions.MaximumHops));
                else
                    OnTraceComplete();

                // Reset the ThreadPool to default
                ThreadPool.SetMinThreads(workerThreads, completionPortThreads);
            }, cancellationToken);
        }
        #endregion
    }
}