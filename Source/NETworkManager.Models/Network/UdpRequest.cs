using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Sends a UDP request to a host and waits for its answer (e.g. NetBIOS, mDNS). No exception if the host doesn't
///     answer, see <see cref="SocketOperation" />.
/// </summary>
public static class UdpRequest
{
    /// <summary>
    ///     Maximum size of an answer.
    /// </summary>
    private const int ResponseBufferSize = 8192;

    /// <summary>
    ///     Sends <paramref name="request" /> to <paramref name="port" /> of the host and returns the first datagram the
    ///     host answers with. Datagrams from other addresses (e.g. a stray or spoofed one) are ignored.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="port">UDP port of the host.</param>
    /// <param name="request">Data to send.</param>
    /// <param name="timeout">Timeout in milliseconds to wait for the answer.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Answer of the host, null if it didn't answer within the timeout (or port unreachable, etc.).</returns>
    public static async Task<byte[]> SendAsync(IPAddress ipAddress, int port, byte[] request, int timeout,
        CancellationToken cancellationToken)
    {
        using var socket = new Socket(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

        // Closing the socket on timeout or cancellation aborts a pending send / receive, it completes with
        // OperationAborted. The results are reported as SocketError, no exception if the host doesn't
        // answer (see SocketOperation).
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        await using var registration = linkedCts.Token.Register(socket.Dispose);

        using var sendArgs = new SocketAsyncEventArgs();
        sendArgs.RemoteEndPoint = new IPEndPoint(ipAddress, port);
        sendArgs.SetBuffer(request);

        if (await SocketOperation.RunAsync(sendArgs, socket.SendToAsync).ConfigureAwait(false) != SocketError.Success)
            return null;

        var buffer = new byte[ResponseBufferSize];

        // Wait for the answer of the host - a datagram from another address (e.g. a stray or spoofed one) is
        // ignored, until the timeout
        while (true)
        {
            using var receiveArgs = new SocketAsyncEventArgs();
            receiveArgs.RemoteEndPoint = new IPEndPoint(
                ipAddress.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any, 0);
            receiveArgs.SetBuffer(buffer);

            // No answer (timeout), port unreachable, etc.
            if (await SocketOperation.RunAsync(receiveArgs, socket.ReceiveFromAsync).ConfigureAwait(false) !=
                SocketError.Success)
                return null;

            if (receiveArgs.RemoteEndPoint is not IPEndPoint remoteEndPoint ||
                IPAddressHelper.CompareIPAddresses(remoteEndPoint.Address, ipAddress) != 0)
                continue;

            return buffer[..receiveArgs.BytesTransferred];
        }
    }
}
