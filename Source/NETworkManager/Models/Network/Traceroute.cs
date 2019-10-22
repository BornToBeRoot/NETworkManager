using DnsClient;
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
        #region Variables
        private LookupClient DnsLookupClient = new LookupClient();

        public int Timeout = 4000;
        public byte[] Buffer = new byte[32];
        public int MaximumHops = 30;
        public bool DontFragement = true;
        public bool ResolveHostname = true;
        #endregion

        #region Events
        public event EventHandler<TracerouteHopReceivedArgs> HopReceived;
        protected virtual void OnHopReceived(TracerouteHopReceivedArgs e)
        {
            HopReceived?.Invoke(this, e);
        }

        public event EventHandler TraceComplete;
        protected virtual void OnTraceComplete()
        {
            TraceComplete?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<MaximumHopsReachedArgs> MaximumHopsReached;
        protected virtual void OnMaximumHopsReached(MaximumHopsReachedArgs e)
        {
            MaximumHopsReached?.Invoke(this, e);
        }

        public event EventHandler UserHasCanceled;
        protected virtual void OnUserHasCanceled()
        {
            UserHasCanceled?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Methods
        public void TraceAsync(IPAddress ipAddress, CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                for (var i = 1; i < MaximumHops + 1; i++)
                {
                    var tasks = new List<Task<Tuple<PingReply, long>>>();

                    for (var y = 0; y < 3; y++)
                    {
                        var i1 = i;

                        tasks.Add(Task.Run(() =>
                        {
                            var stopwatch = new Stopwatch();

                            PingReply pingReply;

                            using (var ping = new System.Net.NetworkInformation.Ping())
                            {
                                stopwatch.Start();

                                pingReply = ping.Send(ipAddress, Timeout, Buffer, new PingOptions { Ttl = i1, DontFragment = DontFragement });

                                stopwatch.Stop();
                            }

                            return Tuple.Create(pingReply, stopwatch.ElapsedMilliseconds);
                        }, cancellationToken));
                    }

                    // ReSharper disable once CoVariantArrayConversion, no write operation
                    Task.WaitAll(tasks.ToArray());

                    var ipAddressHop = tasks.FirstOrDefault(x => x.Result.Item1 != null)?.Result.Item1.Address;

                   // Resolve Hostname
                    var hostname = string.Empty;

                    if (ResolveHostname && ipAddressHop != null)
                    {
                        try
                        {
                            var answer = await DnsLookupClient.GetHostNameAsync(ipAddressHop);

                            if (!string.IsNullOrEmpty(answer))
                                hostname = answer;
                        }
                        catch { } // Couldn't resolve hostname
                    }

                    OnHopReceived(new TracerouteHopReceivedArgs(i, tasks[0].Result.Item2, tasks[1].Result.Item2, tasks[2].Result.Item2, ipAddressHop, hostname, tasks[0].Result.Item1.Status, tasks[1].Result.Item1.Status, tasks[2].Result.Item1.Status));

                    // Check if finished
                    if (ipAddressHop != null && ipAddress.ToString() == ipAddressHop.ToString())
                    {
                        OnTraceComplete();
                        return;
                    }

                    // Check for cancel
                    if (!cancellationToken.IsCancellationRequested)
                        continue;

                    OnUserHasCanceled();

                    return;
                }

                // Max hops reached...
                OnMaximumHopsReached(new MaximumHopsReachedArgs(MaximumHops));

            }, cancellationToken);
        }
        #endregion
    }
}