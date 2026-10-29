using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

/// <summary>
///     Provides a shared ICMP echo (ping) probe used by <see cref="IPScanner" /> and <see cref="PortScanner" />.
/// </summary>
internal static class PingProbe
{
    /// <summary>
    ///     Pings the host. Another echo request is only sent if the previous one timed out (e.g. a lost packet),
    ///     other states like "destination host unreachable" are a definitive answer.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="timeoutMs">Timeout in milliseconds of one echo request.</param>
    /// <param name="attempts">Number of attempts.</param>
    /// <param name="buffer">Data sent with the echo request.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Result of the successful attempt, otherwise of the last attempt.</returns>
    public static async Task<PingInfo> PingAsync(IPAddress ipAddress, int timeoutMs, int attempts, byte[] buffer,
        CancellationToken cancellationToken)
    {
        using var ping = new System.Net.NetworkInformation.Ping();

        PingInfo pingInfo = null;

        for (var i = 0; i < attempts; i++)
        {
            // Get timestamp
            var timestamp = DateTime.Now;

            try
            {
                // Note: the CancellationToken-accepting overload requires a TimeSpan timeout,
                // unlike the legacy int-based overloads used elsewhere in .NET's Ping API.
                var pingReply = await ping.SendPingAsync(ipAddress, TimeSpan.FromMilliseconds(timeoutMs),
                    buffer, cancellationToken: cancellationToken).ConfigureAwait(false);

                // Success
                if (pingReply is { Status: IPStatus.Success })
                    return PingInfo.FromSuccessfulReply(timestamp, pingReply);

                // Failed
                pingInfo = new PingInfo(timestamp, ipAddress, pingReply.Status);

                // Only try again if the request timed out (e.g. a lost packet). Other states like
                // "destination host unreachable" are a definitive answer.
                if (pingReply.Status != IPStatus.TimedOut)
                    break;
            }
            catch (PingException)
            {
                // Ping failed with unknown status
                return new PingInfo(timestamp, ipAddress, IPStatus.Unknown);
            }
        }

        // Result of the last attempt (falls back to unknown status if there was no attempt)
        return pingInfo ?? new PingInfo(DateTime.Now, ipAddress, IPStatus.Unknown);
    }
}
