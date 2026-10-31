using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace NETworkManager.Models.Network;

/// <summary>
///     Neighbor table (ARP / NDP) of a scan of the <see cref="IPScanner" /> (one instance per scan): a snapshot of the
///     table shared by all hosts and read again only when needed, the state of an entry before the probes of a host,
///     and the check of a host that didn't answer any probe (incl. the verification of an entry from earlier traffic
///     and the proxy ARP check against the default gateways).
/// </summary>
/// <param name="hostRoles">Roles of the hosts for the local computer (default gateways), see
/// <see cref="IPScannerHostRoleHelper.GetRoles" />.</param>
internal sealed class IPScannerNeighborCache(IReadOnlyDictionary<IPAddress, IPScannerHostRole> hostRoles)
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(IPScannerNeighborCache));

    /// <summary>
    ///     Minimum time between two reads of the neighbor table, see <see cref="GetEntryAsync" />. Hosts that
    ///     finish while waiting share the next read - otherwise the table would be read for almost every host when
    ///     unreachable hosts are checked for their neighbor entry (ARP / NDP).
    /// </summary>
    private static readonly TimeSpan MinNeighborTableReadInterval = TimeSpan.FromMilliseconds(100);

    /// <summary>
    ///     Maximum number of neighbor entries that are verified at the same time, see <see cref="IsNeighborAsync" /> -
    ///     each verification blocks a thread for up to about 3 seconds.
    /// </summary>
    private const int MaxConcurrentNeighborVerifications = 4;

    /// <summary>
    ///     Maximum age of the neighbor table snapshot that tells if the entry of a host was already
    ///     <see cref="NeighborState.Reachable" /> before its probes (see
    ///     <see cref="WasReachableBeforeAsync" />): the minimum time Windows keeps an entry reachable (reachable
    ///     time 30 s, randomized to 15 to 45 s).
    /// </summary>
    private static readonly TimeSpan MaxNeighborSnapshotAgeBeforeProbes = TimeSpan.FromSeconds(15);

    // Snapshot of the neighbor table (IP address -> entry), see GetEntryAsync. Replaced, never changed.
    // Snapshot and the time it was read (before reading) in one object, so they are always read together without a
    // lock. Only a read of the table takes the semaphore.
    private sealed record NeighborTableSnapshot(Dictionary<IPAddress, NeighborInfo> Entries, DateTime Timestamp);

    private readonly SemaphoreSlim _neighborTableSemaphore = new(1, 1);
    private volatile NeighborTableSnapshot _neighborTable = new([], DateTime.MinValue);

    // The neighbor table could not be read (logged once per scan), only changed while holding the semaphore
    private bool _neighborTableReadFailed;

    private readonly SemaphoreSlim _neighborVerificationSemaphore =
        new(MaxConcurrentNeighborVerifications, MaxConcurrentNeighborVerifications);

    // Default gateways of the operational network interfaces (without scope id), for the proxy ARP check
    private readonly List<IPAddress> _gateways =
        [.. hostRoles.Where(x => x.Value.HasFlag(IPScannerHostRole.DefaultGateway)).Select(x => x.Key)];

    /// <summary>
    ///     Gets the entry of <paramref name="ipAddress" /> from the neighbor table, e.g. for its MAC address, see
    ///     <see cref="GetEntryAsync(IPAddress, DateTime, bool, CancellationToken)" />.
    /// </summary>
    /// <param name="ipAddress">IP address of the host (in a local subnet of the scanner).</param>
    /// <param name="discoveredAt">Time the host answered, <see cref="DateTime.MinValue" /> if it is unreachable.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Neighbor entry or null.</returns>
    public async Task<NeighborInfo> GetEntryAsync(IPAddress ipAddress, DateTime discoveredAt,
        CancellationToken cancellationToken)
    {
        return (await GetEntryAsync(ipAddress, discoveredAt, false, cancellationToken).ConfigureAwait(false)).entry;
    }

    /// <summary>
    ///     Gets the entry of <paramref name="ipAddress" /> from the neighbor table (ARP / NDP). The table is
    ///     read once and cached for the scan instead of reading it for every host. It is only read again on a miss
    ///     (or, with <paramref name="requireReachable" />, if the cached entry is not
    ///     <see cref="NeighborState.Reachable" />) if the cached snapshot is older than <paramref name="discoveredAt" />
    ///     (the time the host answered, when its entry was already created) - a host is always looked up in a table
    ///     read after its answer, and hosts that finish at the same time share one read (at most one read per
    ///     <see cref="MinNeighborTableReadInterval" />).
    /// </summary>
    /// <param name="ipAddress">IP address of the host (in a local subnet of the scanner).</param>
    /// <param name="discoveredAt">
    ///     Time the host answered (or its probes were finished), <see cref="DateTime.MinValue" /> to use the cache - or
    ///     to read the table if it was not read yet.
    /// </param>
    /// <param name="requireReachable">
    ///     Read the table again if the cached entry is not <see cref="NeighborState.Reachable" /> (e.g. still
    ///     <see cref="NeighborState.Incomplete" /> / <see cref="NeighborState.Stale" /> in an older snapshot).
    /// </param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Neighbor entry or null, and the time the snapshot it is from was read.</returns>
    private async Task<(NeighborInfo entry, DateTime timestamp)> GetEntryAsync(IPAddress ipAddress,
        DateTime discoveredAt, bool requireReachable, CancellationToken cancellationToken)
    {
        // Cached snapshot, no lock needed: an entry, or no newer entry since the snapshot was read after the host
        // answered. With the scope id if it was entered (e.g. "fe80::1%3" and "fe80::1%7" are two hosts on different
        // interfaces), see NeighborTable.GetNeighborMap.
        var snapshot = _neighborTable;

        if (TryGetCachedEntry(snapshot, ipAddress, discoveredAt, requireReachable, out var entry))
            return (entry, snapshot.Timestamp);

        // Hosts that finish in the meantime share the next read - wait without holding the lock, so lookups that are
        // answered from the cache never wait for it
        var wait = snapshot.Timestamp + MinNeighborTableReadInterval - DateTime.UtcNow;

        if (wait > TimeSpan.Zero)
            await Task.Delay(wait, cancellationToken).ConfigureAwait(false);

        // Hosts wait without blocking a thread while another host reads the table
        await _neighborTableSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Read by another host in the meantime
            snapshot = _neighborTable;

            if (TryGetCachedEntry(snapshot, ipAddress, discoveredAt, requireReachable, out entry))
                return (entry, snapshot.Timestamp);

            // Timestamp before reading the table, so entries created while reading are covered by the next read
            var timestamp = DateTime.UtcNow;

            Dictionary<IPAddress, NeighborInfo> entries;

            try
            {
                entries = NeighborTable.GetNeighborMap();
            }
            catch (Exception ex)
            {
                // E.g. an error of the IP helper API: no entry instead of ending the scan (hosts are only found by their
                // probes and get no MAC address from the table), read again on the next miss
                if (!_neighborTableReadFailed)
                {
                    _neighborTableReadFailed = true;

                    Log.Error("GetEntryAsync - Could not read the neighbor table (ARP / NDP).", ex);
                }

                entries = [];
            }

            _neighborTable = new NeighborTableSnapshot(entries, timestamp);

            return (entries.GetValueOrDefault(ipAddress), timestamp);
        }
        finally
        {
            _neighborTableSemaphore.Release();
        }
    }

    /// <summary>
    ///     Checks if the neighbor entry of a host was already <see cref="NeighborState.Reachable" /> before its probes
    ///     are sent (see <see cref="IPScannerNeighborCheck.Check" />), from the cached snapshot. A snapshot older than
    ///     <see cref="MaxNeighborSnapshotAgeBeforeProbes" /> doesn't tell: the entry (e.g.
    ///     <see cref="NeighborState.Stale" /> in the snapshot) may have been confirmed by other traffic since and still
    ///     be Reachable from it, also if the host is gone - it is treated as reachable, so a Reachable entry after the
    ///     probes is verified.
    /// </summary>
    /// <param name="ipAddress">IP address of the host (in a local subnet of the scanner).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>True if the entry was reachable before the probes, or if this is not known.</returns>
    public async Task<bool> WasReachableBeforeAsync(IPAddress ipAddress, CancellationToken cancellationToken)
    {
        var (entry, timestamp) = await GetEntryAsync(ipAddress, DateTime.MinValue, false, cancellationToken)
            .ConfigureAwait(false);

        return entry?.State == NeighborState.Reachable ||
               timestamp < DateTime.UtcNow - MaxNeighborSnapshotAgeBeforeProbes;
    }

    /// <summary>
    ///     Looks up an entry in a snapshot of the neighbor table, see <see cref="GetEntryAsync" />.
    /// </summary>
    /// <param name="snapshot">Snapshot of the neighbor table.</param>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="discoveredAt">Time the host answered (or its probes were finished).</param>
    /// <param name="requireReachable">Only a <see cref="NeighborState.Reachable" /> entry is a hit.</param>
    /// <param name="entry">Entry of the host (also a not reachable one), null if there is none.</param>
    /// <returns>True if the snapshot answers the lookup (no need to read the table again).</returns>
    private static bool TryGetCachedEntry(NeighborTableSnapshot snapshot, IPAddress ipAddress,
        DateTime discoveredAt, bool requireReachable, out NeighborInfo entry)
    {
        if (snapshot.Entries.TryGetValue(ipAddress, out entry) &&
            (!requireReachable || entry.State == NeighborState.Reachable))
            return true;

        // Already read after the host answered -> no (newer) entry
        return snapshot.Timestamp > discoveredAt;
    }

    /// <summary>
    ///     Checks if a host in a local subnet that didn't answer any probe answered the ARP request / neighbor
    ///     solicitation of the probes (see <see cref="IPScannerNeighborCheck" />): its neighbor entry is read after the
    ///     probes were finished; an entry from earlier traffic that was not verified again yet (e.g.
    ///     <see cref="NeighborState.Stale" />) is resolved again (at most
    ///     <see cref="MaxConcurrentNeighborVerifications" /> at the same time).
    /// </summary>
    /// <param name="ipAddress">IP address of the host (in a local subnet of the scanner).</param>
    /// <param name="wasReachableBefore">
    ///     The entry was already reachable before the probes were sent, or this is not known (see
    ///     <see cref="WasReachableBeforeAsync" />).
    /// </param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>True if the host is reachable.</returns>
    public async Task<bool> IsNeighborAsync(IPAddress ipAddress, bool wasReachableBefore,
        CancellationToken cancellationToken)
    {
        var (entry, _) = await GetEntryAsync(ipAddress, DateTime.UtcNow, true, cancellationToken)
            .ConfigureAwait(false);

        // Gateways (from the roles of the hosts, computed once per scan)
        var isGateway = IPScannerHostRoleHelper.GetRole(hostRoles, ipAddress)
            .HasFlag(IPScannerHostRole.DefaultGateway);

        // MAC addresses of the gateways from the current snapshot (replaced, never changed - no lock needed)
        var neighborTable = _neighborTable.Entries;

        var gatewayMACAddresses = _gateways
            .Select(x => neighborTable.GetValueOrDefault(x)?.MACAddress.ToString())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        switch (IPScannerNeighborCheck.Check(entry, wasReachableBefore, isGateway, gatewayMACAddresses))
        {
            case IPScannerNeighborCheckResult.Reachable:
                return true;
            case IPScannerNeighborCheckResult.Unreachable:
                return false;
        }

        // Entry from earlier traffic: resolve it again - blocks a thread until the neighbor answered or the resolution
        // failed, so only a few at the same time
        await _neighborVerificationSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Address of the entry (IPv6 link-local: with the scope id of its interface). Also a canceled scan waits
            // for the resolution (max. ~3 s) - nothing may still run (and change the neighbor table) when the scan
            // is complete.
            var macAddress = await Task.Run(() => NeighborTable.ResolveEntry(entry.IPAddress, entry.InterfaceIndex),
                CancellationToken.None).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            return !string.IsNullOrEmpty(macAddress) &&
                   (isGateway || !IPScannerNeighborCheck.IsProxyARPAnswer(macAddress, gatewayMACAddresses));
        }
        finally
        {
            _neighborVerificationSemaphore.Release();
        }
    }

}
