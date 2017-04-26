using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Model.Network
{
    public class Ping
    {
        public event EventHandler<PingArgs> PingReceived;

        protected virtual void OnPingReceived(PingArgs e)
        {
            PingReceived?.Invoke(this, e);
        }

        public event EventHandler PingCompleted;

        protected virtual void OnPingCompleted()
        {
            PingCompleted?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler UserHasCanceled;

        protected virtual void OnUserHasCanceled()
        {
            UserHasCanceled?.Invoke(this, EventArgs.Empty);
        }

        public void SendAsync(IPAddress ipAddress, PingOptions pingOptions, CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                int pingTotal = 0;

                System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions
                {
                    Ttl = pingOptions.TTL,
                    DontFragment = pingOptions.DontFragement
                };

                using (System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping())
                {
                    do
                    {
                        PingReply pingReply = ping.Send(ipAddress, pingOptions.Timeout, pingOptions.Buffer, options);

                        if (pingReply.Status == IPStatus.Success)
                        {
                            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                                OnPingReceived(new PingArgs(pingReply.Address, pingReply.Buffer.Count(), pingReply.RoundtripTime, pingReply.Options.Ttl, pingReply.Status));
                            else
                                OnPingReceived(new PingArgs(pingReply.Address, pingReply.Buffer.Count(), pingReply.RoundtripTime, pingReply.Status));
                        }
                        else
                        {
                            OnPingReceived(new PingArgs(pingReply.Address, pingReply.Status));
                        }

                        // If ping is canceled... dont wait for example 5 seconds
                        for (int i = 0; i < pingOptions.WaitTime; i += 100)
                        {
                            Thread.Sleep(100);

                            if (cancellationToken.IsCancellationRequested)
                                break;
                        }

                        pingTotal++;
                    } while ((pingOptions.Attempts == 0 || pingTotal < pingOptions.Attempts) && !cancellationToken.IsCancellationRequested);
                }
                               
                if (cancellationToken.IsCancellationRequested)
                    OnUserHasCanceled();
                else
                    OnPingCompleted();
            });
        }
    }
}
