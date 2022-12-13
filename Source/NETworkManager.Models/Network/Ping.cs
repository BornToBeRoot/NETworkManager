using NETworkManager.Utilities;
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
        #region Varaibles        
        public int WaitTime = 1000;
        public int Timeout = 4000;
        public byte[] Buffer = new byte[32];
        public int TTL = 64;
        public bool DontFragment = true;
        public int ExceptionCancelCount = 3;
        public string Hostname = string.Empty;
        #endregion

        #region Events
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
        #endregion

        #region Methods
        public void SendAsync(IPAddress ipAddress, CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                var hostname = Hostname;

                // Try to resolve PTR
                if (string.IsNullOrEmpty(hostname))
                {
                    var dnsResult = await DNS.GetInstance().ResolvePtrAsync(ipAddress);

                    if (!dnsResult.HasError)
                        hostname = dnsResult.Value;
                }

                var pingTotal = 0;
                var errorCount = 0;

                var options = new PingOptions
                {
                    Ttl = TTL,
                    DontFragment = DontFragment
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
                            var pingReply = ping.Send(ipAddress, Timeout, Buffer, options);

                            // Reset the error count (if no exception was thrown)
                            errorCount = 0;

                            if (pingReply == null || pingReply.Status != IPStatus.Success)
                            {
                                if (pingReply != null && pingReply.Address == null)
                                    OnPingReceived(new PingReceivedArgs(timestamp, ipAddress, hostname, pingReply.Status));
                                else if (pingReply != null)
                                    OnPingReceived(new PingReceivedArgs(timestamp, pingReply.Address, hostname, pingReply.Status));
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

                            if (errorCount == ExceptionCancelCount)
                            {
                                OnPingException(new PingExceptionArgs(ex.Message, ex.InnerException));

                                break;
                            }
                        }

                        pingTotal++;

                        // If ping is canceled... dont wait for example 5 seconds
                        for (var i = 0; i < WaitTime; i += 100)
                        {
                            Thread.Sleep(100);

                            if (cancellationToken.IsCancellationRequested)
                                break;
                        }
                    } while (!cancellationToken.IsCancellationRequested);
                }

                if (cancellationToken.IsCancellationRequested)
                    OnUserHasCanceled();
                else
                    OnPingCompleted();
            }, cancellationToken);
        }

        // Param: disableSpecialChar --> ExportManager --> "<" this char cannot be displayed in xml
        public static string TimeToString(IPStatus status, long time, bool disableSpecialChar = false)
        {
            if (status != IPStatus.Success && status != IPStatus.TtlExpired)
                return "-/-";

            _ = long.TryParse(time.ToString(), out var t);

            return disableSpecialChar ? $"{t} ms" : t == 0 ? "<1 ms" : $"{t} ms";
        }
        #endregion
    }
}

