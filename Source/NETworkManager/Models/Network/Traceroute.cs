
using System;
using System.Diagnostics;
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
        public event EventHandler<HopReceivedArgs> HopReceived;
        protected virtual void OnHopReceived(HopReceivedArgs e)
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
                int maximumHops = traceOptions.MaximumHops + 1;

                Stopwatch stopwatch = new Stopwatch();

                using (System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping())
                {
                    int hop = 1;

                    do
                    {
                        stopwatch.Start();

                        PingReply pingReply = ping.Send(ipAddress, traceOptions.Timeout, buffer, new System.Net.NetworkInformation.PingOptions() { Ttl = hop, DontFragment = traceOptions.DontFragement });

                        stopwatch.Stop();

                        string hostname = string.Empty;

                        try
                        {
                            if (pingReply.Address != null)
                                hostname = Dns.GetHostEntry(pingReply.Address).HostName;
                        }
                        catch (SocketException) { } // Couldn't resolve hostname

                        OnHopReceived(new HopReceivedArgs(hop, stopwatch.ElapsedMilliseconds, pingReply.Address, hostname, pingReply.Status));

                        if (pingReply.Address != null)
                        {
                            if (pingReply.Address.ToString() == ipAddress.ToString())
                            {
                                OnTraceComplete();
                                return;
                            }

                            stopwatch.Reset();
                        }

                        hop++;
                    } while (hop < maximumHops && !cancellationToken.IsCancellationRequested);

                    if (cancellationToken.IsCancellationRequested)
                        OnUserHasCanceled();
                    else if (hop == maximumHops)
                        OnMaximumHopsReached(new MaximumHopsReachedArgs(traceOptions.MaximumHops));
                }
            }, cancellationToken);
        }
        #endregion
    }
}