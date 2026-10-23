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
    /// <returns>
    ///     <see cref="PortState.Open" /> if the connect succeeded, <see cref="PortState.Closed" /> if the host
    ///     refused the connection (TCP reset - the host is reachable) and <see cref="PortState.TimedOut" /> if
    ///     there was no answer (timeout, host or network unreachable, local socket error).
    /// </returns>
    public static async Task<PortState> ProbeAsync(IPAddress ipAddress, int port, int timeoutMs,
        CancellationToken cancellationToken)
    {
        using var tcpClient = new TcpClient(ipAddress.AddressFamily);

        // Close with a reset instead of a graceful shutdown. Otherwise every open port leaves a socket in
        // TIME_WAIT for minutes, which can use up all local (ephemeral) ports on large scans.
        tcpClient.LingerState = new LingerOption(true, 0);

        using var timeoutCts = new CancellationTokenSource(timeoutMs);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            await tcpClient.ConnectAsync(ipAddress, port, linkedCts.Token).ConfigureAwait(false);

            return tcpClient.Connected ? PortState.Open : PortState.Closed;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // linkedCts was canceled, but not by the caller -> our own timeout fired
            return PortState.TimedOut;
        }
        catch (OperationCanceledException)
        {
            // Caller's token is why this was canceled -> propagate as a real cancellation
            throw;
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
        {
            // The host answered with a reset -> port is closed (and the host is reachable)
            return PortState.Closed;
        }
        catch (Exception)
        {
            // Any other connect failure (host/network unreachable, local socket error, etc.) -> no answer
            // from the host, same as a timeout. Regardless of whether cancellation also happens to be in
            // flight concurrently for an unrelated reason (that used to race against this classification
            // via a token-state check here).
            return PortState.TimedOut;
        }
        finally
        {
            tcpClient.Close();
        }
    }
}
