using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class Ping
    {
        public event EventHandler<PingReceivedArgs> PingReceived;

        protected virtual void OnPingReceived(PingReceivedArgs e)
        {
            PingReceived?.Invoke(this, e);
        }

        public event EventHandler PingCompleted;

        protected virtual void OnPingCompleted()
        {
            PingCompleted?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<PingExceptionArgs> PingException;

        protected virtual void OnPingException(PingExceptionArgs e)
        {
            PingException?.Invoke(this, e);
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
                int errorCount = 0;

                System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions
                {
                    Ttl = pingOptions.TTL,
                    DontFragment = pingOptions.DontFragment
                };

                using (System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping())
                {
                    do
                    {
                        PingReply pingReply;

                        try
                        {
                            pingReply = ping.Send(ipAddress, pingOptions.Timeout, pingOptions.Buffer, options);

                            errorCount = 0;  // Reset the error count (if no exception was thrown)

                            if (pingReply.Status == IPStatus.Success)
                            {
                                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                                    OnPingReceived(new PingReceivedArgs(pingReply.Address, pingReply.Buffer.Count(), pingReply.RoundtripTime, pingReply.Options.Ttl, pingReply.Status));
                                else
                                    OnPingReceived(new PingReceivedArgs(pingReply.Address, pingReply.Buffer.Count(), pingReply.RoundtripTime, pingReply.Status));
                            }
                            else
                            {
                                if (pingReply.Address == null)
                                    OnPingReceived(new PingReceivedArgs(ipAddress, pingReply.Status));
                                else
                                    OnPingReceived(new PingReceivedArgs(pingReply.Address, pingReply.Status));
                            }

                        }
                        catch (PingException ex)
                        {
                            errorCount++;

                            if (errorCount == pingOptions.ExceptionCancelCount)
                            {
                                OnPingException(new PingExceptionArgs(ex.Message, ex.InnerException));

                                break;
                            }
                        }

                        pingTotal++;

                        // If ping is canceled... dont wait for example 5 seconds
                        for (int i = 0; i < pingOptions.WaitTime; i += 100)
                        {
                            Thread.Sleep(100);

                            if (cancellationToken.IsCancellationRequested)
                                break;
                        }
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

