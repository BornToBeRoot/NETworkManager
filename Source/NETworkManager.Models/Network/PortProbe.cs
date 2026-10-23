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
    ///     <c>SIO_TCP_INITIAL_RTO</c>: sets the initial retransmission timeout and the number of SYN
    ///     retransmissions of a socket (Windows 10 1703 and later).
    /// </summary>
    private const int SIO_TCP_INITIAL_RTO = unchecked((int)0x98000011);

    /// <summary>
    ///     <c>TCP_INITIAL_RTO_NO_SYN_RETRANSMISSIONS</c>: don't retransmit the connection request (SYN).
    /// </summary>
    private const byte TCP_INITIAL_RTO_NO_SYN_RETRANSMISSIONS = 0xFE;

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
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using var socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Close with a reset instead of a graceful shutdown. Otherwise every open port leaves a socket in
            // TIME_WAIT for minutes, which can use up all local (ephemeral) ports on large scans.
            socket.LingerState = new LingerOption(true, 0);

            // Windows retries the connection request after a reset like after a lost request, so a closed port
            // is only reported after ~2 s (measured) - as long as the default timeout, so it was shown as timed
            // out. Without SYN retransmissions a reset is reported immediately; the request is sent once and the
            // answer is awaited until the timeout.
            if (OperatingSystem.IsWindows())
                DisableSynRetransmissions(socket, timeoutMs);

            using var connectArgs = new SocketAsyncEventArgs();
            connectArgs.RemoteEndPoint = new IPEndPoint(ipAddress, port);

            using var timeoutCts = new CancellationTokenSource(timeoutMs);
            using var linkedCts =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            // The result is reported as SocketError, no exception for a refused connection or a timeout
            // (see SocketOperation)
            var connectTask = SocketOperation.RunAsync(connectArgs, socket.ConnectAsync);

            SocketError socketError;

            // Timeout or cancellation -> abort the connect, it completes with OperationAborted
            await using (linkedCts.Token.Register(() => Socket.CancelConnectAsync(connectArgs)))
            {
                socketError = await connectTask.ConfigureAwait(false);
            }

            // Caller's token is why this was aborted -> propagate as a real cancellation
            cancellationToken.ThrowIfCancellationRequested();

            return socketError switch
            {
                SocketError.Success => PortState.Open,
                // The host answered with a reset -> port is closed (and the host is reachable)
                SocketError.ConnectionRefused => PortState.Closed,
                // Timeout (aborted), host/network unreachable, local socket error, etc. -> no answer from the host
                _ => PortState.TimedOut
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // Unexpected local error (e.g. no free local port to bind) -> no answer from the host
            return PortState.TimedOut;
        }
    }

    /// <summary>
    ///     Disables the SYN retransmissions of the socket and sets the initial retransmission timeout to
    ///     <paramref name="timeoutMs" />, so the connection request is sent once and Windows waits for the answer
    ///     as long as the probe does. Ignored if not supported.
    /// </summary>
    /// <param name="socket">Socket that is not connected yet.</param>
    /// <param name="timeoutMs">Timeout of the probe in milliseconds.</param>
    private static void DisableSynRetransmissions(Socket socket, int timeoutMs)
    {
        // TCP_INITIAL_RTO_PARAMETERS { USHORT Rtt; UCHAR MaxSynRetransmissions; } - 0xFFFF = unspecified Rtt
        var rtt = (ushort)Math.Clamp(timeoutMs, 1, ushort.MaxValue - 1);

        byte[] parameters = [(byte)rtt, (byte)(rtt >> 8), TCP_INITIAL_RTO_NO_SYN_RETRANSMISSIONS, 0];

        try
        {
            socket.IOControl(SIO_TCP_INITIAL_RTO, parameters, null);
        }
        catch (Exception)
        {
            // Not supported (older Windows) -> Windows default (closed ports are reported after ~2 s)
        }
    }
}
