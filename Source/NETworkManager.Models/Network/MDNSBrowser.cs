using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

/// <summary>A selectable link. Discovery is kept on one interface, so equal .local names on separate links never mix.</summary>
public sealed record MDNSInterfaceInfo(string Id, string Name, IPAddress IPv4Address, int IPv4Index, int IPv6Index)
{
    public override string ToString() => Name;
}

/// <summary>Local-link multicast DNS service discovery over IPv4 and IPv6. No responder or service registration.</summary>
public static class MDNSBrowser
{
    public static IReadOnlyList<MDNSInterfaceInfo> GetInterfaces() => System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
        .Where(x => x.OperationalStatus == OperationalStatus.Up && x.SupportsMulticast &&
                    x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .Select(x =>
        {
            var properties = x.GetIPProperties();
            var ipv4 = properties.UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;
            var ipv6 = properties.UnicastAddresses.Any(a => a.Address.AddressFamily == AddressFamily.InterNetworkV6);
            return new MDNSInterfaceInfo(x.Id, x.Name, ipv4, ipv4 == null ? 0 : properties.GetIPv4Properties().Index,
                ipv6 ? properties.GetIPv6Properties().Index : 0);
        }).Where(x => x.IPv4Address != null || x.IPv6Index != 0).OrderBy(x => x.Name).ToArray();

    /// <summary>Browses until cancellation; reports detached snapshots from a worker thread every 250 ms.</summary>
    public static async Task BrowseAsync(MDNSInterfaceInfo networkInterface, string serviceType,
        Action<MDNSDiscoverySnapshot> update, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(networkInterface);
        var cache = new MDNSServiceCache(serviceType, networkInterface.Name);
        var sockets = new List<(Socket Socket, IPEndPoint Destination, int Index)>();
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var receivers = new List<Task>();
        try
        {
            if (networkInterface.IPv4Address != null)
                sockets.Add(CreateSocket(networkInterface, false));
            if (networkInterface.IPv6Index != 0)
                sockets.Add(CreateSocket(networkInterface, true));
            if (sockets.Count == 0)
                throw new NetworkInformationException();

            var packets = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(512)
            {
                SingleReader = true,
                FullMode = BoundedChannelFullMode.DropOldest
            });
            receivers.AddRange(sockets.Select(x => ReceiveAsync(x.Socket, x.Index, packets.Writer, lifetime.Token)));
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(250));
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var receiver in receivers.Where(x => x.IsCompleted))
                    await receiver.ConfigureAwait(false); // Surface interface/socket failures, never silently show an empty scan.
                var now = DateTimeOffset.UtcNow;
                // Bound work per tick, even if the link is very busy.
                for (var i = 0; i < 512 && packets.Reader.TryRead(out var message); i++)
                    cache.ProcessResponse(message, now, networkInterface.IPv6Index);
                foreach (var question in cache.GetQuestions(now))
                {
                    var data = MDNSMessage.CreateQuery(question.Name, question.Type);
                    foreach (var socket in sockets)
                        await socket.Socket.SendToAsync(data, SocketFlags.None, socket.Destination, cancellationToken)
                            .ConfigureAwait(false);
                }
                update(new MDNSDiscoverySnapshot(cache.GetServices(now), cache.GetRecords(now)));
            } while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false));
        }
        finally
        {
            await lifetime.CancelAsync().ConfigureAwait(false);
            foreach (var socket in sockets)
                socket.Socket.Dispose();
            try { await Task.WhenAll(receivers).ConfigureAwait(false); }
            catch (OperationCanceledException) { }
            catch (SocketException) when (lifetime.IsCancellationRequested) { }
            catch (ObjectDisposedException) when (lifetime.IsCancellationRequested) { }
        }
    }

    private static (Socket, IPEndPoint, int) CreateSocket(MDNSInterfaceInfo link, bool ipv6)
    {
        var socket = new Socket(ipv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork,
            SocketType.Dgram, ProtocolType.Udp);
        try
        {
            socket.ExclusiveAddressUse = false;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            if (ipv6)
            {
                socket.DualMode = false;
                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.PacketInformation, true);
                socket.Bind(new IPEndPoint(IPAddress.IPv6Any, 5353));
                var group = IPAddress.Parse("ff02::fb");
                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership,
                    new IPv6MulticastOption(group, link.IPv6Index));
                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastInterface, link.IPv6Index);
                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastTimeToLive, 255);
                return (socket, new IPEndPoint(new IPAddress(group.GetAddressBytes(), link.IPv6Index), 5353), link.IPv6Index);
            }
            else
            {
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
                socket.Bind(new IPEndPoint(IPAddress.Any, 5353));
                var group = IPAddress.Parse("224.0.0.251");
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                    new MulticastOption(group, link.IPv4Address));
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, link.IPv4Address.GetAddressBytes());
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 255);
                return (socket, new IPEndPoint(group, 5353), link.IPv4Index);
            }
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    private static async Task ReceiveAsync(Socket socket, int index, ChannelWriter<byte[]> writer, CancellationToken token)
    {
        var buffer = new byte[65535];
        EndPoint any = new IPEndPoint(socket.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any, 0);
        while (true)
        {
            var result = await socket.ReceiveMessageFromAsync(buffer.AsMemory(), SocketFlags.None, any, token).ConfigureAwait(false);
            if (result.RemoteEndPoint is IPEndPoint { Port: 5353 } && result.PacketInformation.Interface == index &&
                (result.SocketFlags & SocketFlags.Truncated) == 0)
                writer.TryWrite(buffer[..result.ReceivedBytes]);
        }
    }
}
