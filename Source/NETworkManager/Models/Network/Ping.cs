using System;
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
                var hostname = pingOptions.Hostname;

                // Try to resolve PTR
                if (string.IsNullOrEmpty(hostname))
                {
                    try
                    {
                        Task.Run(() =>
                        {
                            hostname = Dns.GetHostEntryAsync(ipAddress).Result.HostName;
                        }, cancellationToken);
                    }
                    catch (SocketException) { }
                }

                var pingTotal = 0;
                var errorCount = 0;

                var options = new System.Net.NetworkInformation.PingOptions
                {
                    Ttl = pingOptions.TTL,
                    DontFragment = pingOptions.DontFragment
                };

                using (var ping = new System.Net.NetworkInformation.Ping())
                {
                    do
                    {
                        try
                        {
                            // Get timestamp 
                            var timestamp = DateTime.Now;

                            // Send ping
                            var pingReply = ping.Send(ipAddress, pingOptions.Timeout, pingOptions.Buffer, options);

                            // Reset the error count (if no exception was thrown)
                            errorCount = 0;

                            if (pingReply == null || pingReply.Status != IPStatus.Success)
                            {
                                if (pingReply != null && pingReply.Address == null)
                                    OnPingReceived(new PingReceivedArgs(timestamp, ipAddress, hostname, pingReply.Status));
                                else if (pingReply != null)
                                    OnPingReceived(new PingReceivedArgs(timestamp, pingReply.Address, hostname,pingReply.Status));
                            }
                            else
                            {
                                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                                    OnPingReceived(new PingReceivedArgs(timestamp, pingReply.Address, hostname,
                                        pingReply.Buffer.Length, pingReply.RoundtripTime, pingReply.Options.Ttl, pingReply.Status));
                                else
                                    OnPingReceived(new PingReceivedArgs(timestamp, pingReply.Address, hostname,
                                        pingReply.Buffer.Length, pingReply.RoundtripTime, pingReply.Status));
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
                        for (var i = 0; i < pingOptions.WaitTime; i += 100)
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
            }, cancellationToken);
        }
    }
}

