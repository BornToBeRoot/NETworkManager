using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

/// <summary>
///     Provides a shared, fully asynchronous TCP connect probe used by <see cref="IPScanner" /> and
///     <see cref="PortScanner" />.
/// </summary>
internal static class PortProbe
{
    /// <summary>
    ///     Attempts a TCP connect to the given <paramref name="ipAddress" /> and <paramref name="port" /> and
    ///     classifies the result. Never throws for expected connect failures.
    /// </summary>
    /// <param name="ipAddress">IP address to connect to.</param>
    /// <param name="port">Port to connect to.</param>
    /// <param name="timeoutMs">Timeout in milliseconds after which the port is considered timed out.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The <see cref="PortState" /> of the probed port.</returns>
    public static async Task<PortState> ProbeAsync(IPAddress ipAddress, int port, int timeoutMs,
        CancellationToken cancellationToken)
    {
        using var tcpClient = new TcpClient(ipAddress.AddressFamily);
        using var timeoutCts = new CancellationTokenSource(timeoutMs);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            await tcpClient.ConnectAsync(ipAddress, port, linkedCts.Token).ConfigureAwait(false);

            return tcpClient.Connected ? PortState.Open : PortState.Closed;
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested &&
                                                   !cancellationToken.IsCancellationRequested)
        {
            // Only our own timeout fired, not the caller's cancellation -> timed out
            return PortState.TimedOut;
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // Connection refused, host unreachable, etc.
            return PortState.Closed;
        }
        finally
        {
            tcpClient.Close();
        }
    }
}
