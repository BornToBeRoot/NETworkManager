using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using log4net;

namespace NETworkManager.Models.Network;

/// <summary>A selectable link. Discovery is kept on one interface, so equal .local names on separate links never mix.</summary>
public sealed record MDNSInterfaceInfo(string Id, string Name, IPAddress IPv4Address, int IPv4Index, int IPv6Index)
{
    public override string ToString() => Name;
}

/// <summary>Local-link multicast DNS service discovery over IPv4 and IPv6. No responder or service registration.</summary>
public static class MDNSBrowser
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(MDNSBrowser));

    /// <summary>
    ///     Durations of a browse in seconds that can be selected (tool and run command), 0 = until stopped.
    /// </summary>
    public static IReadOnlyList<int> Durations { get; } = [10, 30, 60, 120, 300, 0];

    /// <summary>
    ///     Windows reports an ICMP "port unreachable" of an earlier datagram as a connection reset on the next receive
    ///     of an UDP socket (SIO_UDP_CONNRESET). Disabled, it would only end the receiver.
    /// </summary>
    private const int SIO_UDP_CONNRESET = -1744830452;

    /// <summary>
    ///     Gets the active interfaces with multicast support and an IPv4 or IPv6 address (sorted by name).
    /// </summary>
    public static IReadOnlyList<MDNSInterfaceInfo> GetInterfaces() => System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
        .Where(x => x.OperationalStatus == OperationalStatus.Up && x.SupportsMulticast &&
                    x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .Select(TryCreateInterfaceInfo)
        .Where(x => x != null && (x.IPv4Address != null || x.IPv6Index != 0)).OrderBy(x => x.Name).ToArray();

    /// <summary>
    ///     Reads the addresses and indexes of an interface. An interface whose IP properties can't be read (e.g. some
    ///     virtual adapters) is skipped instead of failing the whole list.
    /// </summary>
    private static MDNSInterfaceInfo TryCreateInterfaceInfo(System.Net.NetworkInformation.NetworkInterface networkInterface)
    {
        IPInterfaceProperties properties;
        IPAddress ipv4;
        bool hasIPv6;

        // The properties and the address list (read when enumerated)
        try
        {
            properties = networkInterface.GetIPProperties();
            ipv4 = properties.UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;
            hasIPv6 = properties.UnicastAddresses.Any(a => a.Address.AddressFamily == AddressFamily.InterNetworkV6);
        }
        catch (NetworkInformationException ex)
        {
            Log.Warn($"Cannot read the IP properties of the network interface \"{networkInterface.Name}\", it is skipped.", ex);

            return null;
        }

        // Each address family on its own: one that can't be read (e.g. IPv6 partly configured) doesn't drop the other
        var ipv4Index = ipv4 == null ? 0 : GetIndex(() => properties.GetIPv4Properties().Index, "IPv4");
        var ipv6Index = hasIPv6 ? GetIndex(() => properties.GetIPv6Properties().Index, "IPv6") : 0;

        return new MDNSInterfaceInfo(networkInterface.Id, networkInterface.Name, ipv4Index == 0 ? null : ipv4, ipv4Index,
            ipv6Index);

        int GetIndex(Func<int> getIndex, string addressFamily)
        {
            try
            {
                return getIndex();
            }
            catch (NetworkInformationException ex)
            {
                Log.Warn($"Cannot read the {addressFamily} properties of the network interface \"{networkInterface.Name}\".", ex);

                return 0;
            }
        }
    }

    /// <summary>
    ///     Browses until cancellation. Reports detached snapshots from a worker thread, at most every 250 ms (when the
    ///     cache changed) and once per second (TTL countdown).
    /// </summary>
    /// <param name="networkInterface">Interface to browse on.</param>
    /// <param name="serviceTypes">Service types (e.g. <c>_ipp._tcp</c>), <c>*</c> for all.</param>
    /// <param name="update">Receives the snapshots (worker thread).</param>
    /// <param name="cancellationToken">Ends the browse.</param>
    /// <param name="cache">
    ///     Cache of an earlier browse of the same interface and service type, to continue it (e.g. after a pause): the
    ///     cached records are kept (if not expired meanwhile) and the due questions are sent right away.
    /// </param>
    public static async Task BrowseAsync(MDNSInterfaceInfo networkInterface, IReadOnlyCollection<string> serviceTypes,
        Action<MDNSDiscoverySnapshot> update, CancellationToken cancellationToken, MDNSServiceCache cache = null)
    {
        ArgumentNullException.ThrowIfNull(networkInterface);
        cache ??= new MDNSServiceCache(serviceTypes, networkInterface.Name);
        var sockets = new List<(Socket Socket, IPEndPoint Destination, int Index)>();
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var receivers = new List<Task>();
        long droppedPackets = 0;
        try
        {
            // One address family may fail (e.g. joining the IPv6 group on some adapters), the other is still used
            SocketException socketError = null;
            foreach (var ipv6 in new[] { false, true })
            {
                if (ipv6 ? networkInterface.IPv6Index == 0 : networkInterface.IPv4Address == null)
                    continue;

                try
                {
                    sockets.Add(CreateSocket(networkInterface, ipv6));
                }
                catch (SocketException ex)
                {
                    Log.Warn($"Cannot open the mDNS socket ({(ipv6 ? "IPv6" : "IPv4")}) on \"{networkInterface.Name}\".", ex);
                    socketError ??= ex;
                }
            }

            if (sockets.Count == 0)
                throw socketError ?? (Exception)new NetworkInformationException();

            // A very busy link: the oldest packets are dropped (counted for the log)
            var packets = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(512)
            {
                SingleReader = true,
                FullMode = BoundedChannelFullMode.DropOldest
            }, _ => Interlocked.Increment(ref droppedPackets));

            Log.Info($"mDNS browse on \"{networkInterface.Name}\" for {string.Join(", ", serviceTypes)} started " +
                     $"({string.Join(", ", sockets.Select(x => x.Socket.AddressFamily == AddressFamily.InterNetworkV6 ? "IPv6" : "IPv4"))}).");

            receivers.AddRange(sockets.Select(x => ReceiveAsync(x.Socket, x.Index, packets.Writer, lifetime.Token)));
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(250));
            var lastVersion = -1L;
            var lastUpdate = DateTimeOffset.MinValue;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                // A receiver ends only with an error (not a transient one, see ReceiveAsync). The browse fails if all
                // receivers failed - never silently show an empty result.
                if (receivers.All(x => x.IsCompleted))
                    await Task.WhenAll(receivers).ConfigureAwait(false);
                var now = DateTimeOffset.UtcNow;
                // Bound work per tick, even if the link is very busy.
                for (var i = 0; i < 512 && packets.Reader.TryRead(out var message); i++)
                    cache.ProcessResponse(message, now, networkInterface.IPv6Index);
                // Several questions per packet, with their known answers (RFC 6762 7.1)
                var questions = cache.GetQuestions(now);
                var queries = questions.Count == 0
                    ? []
                    : MDNSMessage.CreateQueries(questions, (name, type) => cache.GetKnownAnswers(name, type, now));
                foreach (var data in queries)
                {
                    foreach (var socket in sockets)
                    {
                        try
                        {
                            await socket.Socket.SendToAsync(data, SocketFlags.None, socket.Destination, cancellationToken)
                                .ConfigureAwait(false);
                        }
                        catch (SocketException ex)
                        {
                            // E.g. the network is (temporarily) unreachable: the questions are asked again later
                            Log.Debug($"Cannot send the mDNS questions ({ex.SocketErrorCode}).");
                        }
                    }
                }
                // Only if something changed, or once per second for the TTL countdown
                cache.Expire(now);

                if (cache.Version != lastVersion || now - lastUpdate >= TimeSpan.FromSeconds(1))
                {
                    lastVersion = cache.Version;
                    lastUpdate = now;
                    update(new MDNSDiscoverySnapshot(cache.GetServices(now), cache.GetRecords(now)));
                }
            } while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false));
        }
        finally
        {
            await lifetime.CancelAsync().ConfigureAwait(false);
            foreach (var socket in sockets)
                socket.Socket.Dispose();

            // Diagnostics for "no services found" on a network: were packets received at all, dropped or not cached?
            if (sockets.Count > 0)
                Log.Info($"mDNS browse on \"{networkInterface.Name}\" ended: {cache.Packets} packets processed " +
                         $"({cache.PacketsWithoutRecords} without records, e.g. queries), " +
                         $"{Interlocked.Read(ref droppedPackets)} dropped (busy), " +
                         $"{cache.RecordsNotCached} records not cached (cache full).");
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
            if (OperatingSystem.IsWindows())
                socket.IOControl(SIO_UDP_CONNRESET, [0, 0, 0, 0], null);

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
            SocketReceiveMessageFromResult result;

            try
            {
                result = await socket.ReceiveMessageFromAsync(buffer.AsMemory(), SocketFlags.None, any, token)
                    .ConfigureAwait(false);
            }
            catch (SocketException ex) when (ex.SocketErrorCode is SocketError.ConnectionReset or
                                                 SocketError.NetworkReset or SocketError.MessageSize)
            {
                // ICMP error of an earlier datagram or a too large datagram: only this datagram is lost
                continue;
            }
            catch (SocketException ex) when (!token.IsCancellationRequested)
            {
                // The other address family may still work (see BrowseAsync)
                Log.Warn($"mDNS receiver ({socket.AddressFamily}) stopped.", ex);
                throw;
            }

            // Larger packets than allowed (RFC 6762 17) are not even copied, see MDNSMessage.ReadResponse
            if (result.RemoteEndPoint is IPEndPoint { Port: 5353 } && result.PacketInformation.Interface == index &&
                (result.SocketFlags & SocketFlags.Truncated) == 0 && result.ReceivedBytes <= MDNSMessage.MaxPacketSize)
                writer.TryWrite(buffer[..result.ReceivedBytes]);
        }
    }
}
