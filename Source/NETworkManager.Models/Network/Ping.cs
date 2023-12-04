using NETworkManager.Utilities;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

public sealed class Ping
{
    #region Varaibles        
    public int WaitTime = 1000;
    public int Timeout = 4000;
    public byte[] Buffer = new byte[32];
    public int TTL = 64;
    public bool DontFragment = true;
        
    private const int ExceptionCancelCount = 3;
    #endregion

    #region Events
    public event EventHandler<PingReceivedArgs> PingReceived;

    private void OnPingReceived(PingReceivedArgs e)
    {
        PingReceived?.Invoke(this, e);
    }

    public event EventHandler PingCompleted;

    private void OnPingCompleted()
    {
        PingCompleted?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<PingExceptionArgs> PingException;

    private void OnPingException(PingExceptionArgs e)
    {
        PingException?.Invoke(this, e);
    }

    public event EventHandler<HostnameArgs> HostnameResolved;

    private void OnHostnameResolved(HostnameArgs e)
    {
        HostnameResolved?.Invoke(this, e);
    }

    public event EventHandler UserHasCanceled;

    private void OnUserHasCanceled()
    {
        UserHasCanceled?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Methods
    public void SendAsync(IPAddress ipAddress, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            var hostname = string.Empty;

            // Try to resolve PTR
            var dnsResult = await DNSClient.GetInstance().ResolvePtrAsync(ipAddress);

            if (!dnsResult.HasError)
            {
                hostname = dnsResult.Value;
                    
                OnHostnameResolved(new HostnameArgs(hostname));
            }
            
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

                        if (pingReply is not { Status: IPStatus.Success })
                        {
                            if (pingReply != null)
                                OnPingReceived(new PingReceivedArgs(
                                    new PingInfo(timestamp, pingReply.Address, hostname, pingReply.Status)));
                        }
                        else
                        {
                            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                                OnPingReceived(new PingReceivedArgs(
                                    new PingInfo(timestamp, pingReply.Address, hostname,
                                    pingReply.Buffer.Length, pingReply.RoundtripTime, pingReply.Options!.Ttl, pingReply.Status)));
                            else
                                OnPingReceived(new PingReceivedArgs(
                                    new PingInfo(timestamp, pingReply.Address, hostname,
                                    pingReply.Buffer.Length, pingReply.RoundtripTime, pingReply.Status)));
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
            // Currently not used (ping will run until the user cancels it)
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

