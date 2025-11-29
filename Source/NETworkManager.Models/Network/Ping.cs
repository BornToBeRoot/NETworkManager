using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Provides functionality to ping a network host.
/// </summary>
public sealed class Ping
{
    #region Variables

    /// <summary>
    ///     The time in milliseconds to wait between ping requests. Default is 1000ms.
    /// </summary>
    public int WaitTime = 1000;

    /// <summary>
    ///     The time in milliseconds to wait for a reply. Default is 4000ms.
    /// </summary>
    public int Timeout = 4000;

    /// <summary>
    ///     The buffer to send with the ping request. Default is 32 bytes.
    /// </summary>
    public byte[] Buffer = new byte[32];

    /// <summary>
    ///     The Time to Live (TTL) value for the ping request. Default is 64.
    /// </summary>
    public int TTL = 64;

    /// <summary>
    ///     Indicates whether to prevent fragmentation of the data packets. Default is true.
    /// </summary>
    public bool DontFragment = true;

    private const int ExceptionCancelCount = 3;

    #endregion

    #region Events

    /// <summary>
    ///     Occurs when a ping reply is received.
    /// </summary>
    public event EventHandler<PingReceivedArgs> PingReceived;

    private void OnPingReceived(PingReceivedArgs e)
    {
        PingReceived?.Invoke(this, e);
    }

    /// <summary>
    ///     Occurs when the ping operation is completed.
    /// </summary>
    public event EventHandler PingCompleted;

    private void OnPingCompleted()
    {
        PingCompleted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Occurs when a ping exception is thrown.
    /// </summary>
    public event EventHandler<PingExceptionArgs> PingException;

    private void OnPingException(PingExceptionArgs e)
    {
        PingException?.Invoke(this, e);
    }

    /// <summary>
    ///     Occurs when the hostname is resolved.
    /// </summary>
    public event EventHandler<HostnameArgs> HostnameResolved;

    private void OnHostnameResolved(HostnameArgs e)
    {
        HostnameResolved?.Invoke(this, e);
    }

    /// <summary>
    ///     Occurs when the user has canceled the operation.
    /// </summary>
    public event EventHandler UserHasCanceled;

    private void OnUserHasCanceled()
    {
        UserHasCanceled?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Sends ping requests to the specified IP address asynchronously.
    /// </summary>
    /// <param name="ipAddress">The IP address to ping.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
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
                                        pingReply.Buffer.Length, pingReply.RoundtripTime, pingReply.Options!.Ttl,
                                        pingReply.Status)));
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
    /// <summary>
    ///     Converts the ping time to a string representation.
    /// </summary>
    /// <param name="status">The IP status of the ping reply.</param>
    /// <param name="time">The round-trip time in milliseconds.</param>
    /// <param name="disableSpecialChar">If true, disables special characters like '&lt;' in the output (e.g., for XML export).</param>
    /// <returns>The formatted time string.</returns>
    public static string TimeToString(IPStatus status, long time, bool disableSpecialChar = false)
    {
        if (status != IPStatus.Success && status != IPStatus.TtlExpired)
            return "-/-";

        _ = long.TryParse(time.ToString(), out var t);

        return disableSpecialChar ? $"{t} ms" : t == 0 ? "<1 ms" : $"{t} ms";
    }

    #endregion
}