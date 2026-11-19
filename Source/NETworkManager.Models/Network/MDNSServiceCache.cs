using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Per-link DNS-SD cache. Keeps independently arriving PTR/SRV/TXT/address records until their TTL expires,
///     observes goodbye/cache-flush records, and schedules follow-up questions with exponential backoff.
///     The browser serializes access; no UI objects are shared with it.
/// </summary>
public sealed class MDNSServiceCache
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(MDNSServiceCache));

    /// <summary>Maximum number of cached records (busy or hostile networks).</summary>
    public const int MaxRecords = 4096;

    public static IReadOnlyList<string> CommonServiceTypes { get; } = Array.AsReadOnly(new[]
    {
        "_ipp._tcp.local.", "_ipps._tcp.local.", "_printer._tcp.local.", "_pdl-datastream._tcp.local.",
        "_scanner._tcp.local.", "_uscan._tcp.local.", "_http._tcp.local.", "_https._tcp.local.",
        "_ssh._tcp.local.", "_smb._tcp.local.", "_airplay._tcp.local.", "_googlecast._tcp.local."
    });

    private sealed record Entry(MDNSRecord Record, DateTimeOffset Received, DateTimeOffset Expires);
    private sealed record Question(DateTimeOffset Next, int Interval);
    private readonly Dictionary<string, Entry> _records = new(StringComparer.Ordinal);

    // Keys of the records per RRset (lowercase name, type), so a cache flush doesn't search the whole cache per packet
    private readonly Dictionary<(string Name, ushort Type), HashSet<string>> _rrsets = new();
    private readonly Dictionary<(string Name, ushort Type), Question> _questions = new();
    // Browsed service types (normalized), empty = all (*)
    private readonly HashSet<string> _serviceTypes = new(StringComparer.Ordinal);
    private readonly string _networkInterface;

    /// <summary>
    ///     Creates the cache of a browse of one service type (or <c>*</c> for all).
    /// </summary>
    public MDNSServiceCache(string serviceType, string networkInterface) : this([serviceType], networkInterface)
    {
    }

    /// <summary>
    ///     Creates the cache of a browse of several service types (e.g. of a service type profile). <c>*</c> browses
    ///     all advertised service types.
    /// </summary>
    public MDNSServiceCache(IEnumerable<string> serviceTypes, string networkInterface)
    {
        ArgumentNullException.ThrowIfNull(serviceTypes);

        foreach (var serviceType in serviceTypes)
        {
            if (!MDNSServiceType.TryNormalize(serviceType, out var normalized))
                throw new ArgumentException($"Invalid service type \"{serviceType}\".", nameof(serviceTypes));

            _serviceTypes.Add(normalized);
        }

        if (_serviceTypes.Count == 0)
            throw new ArgumentException("No service type.", nameof(serviceTypes));

        // * includes all others
        if (_serviceTypes.Contains("*"))
            _serviceTypes.Clear();

        _networkInterface = networkInterface;
    }

    private bool BrowsesAll => _serviceTypes.Count == 0;

    /// <summary>
    ///     Incremented when a record is added, replaced by a different value or removed (not when a record is only
    ///     announced again). The browser creates a new snapshot only if it changed (or for the TTL countdown).
    /// </summary>
    public long Version { get; private set; }

    /// <summary>Number of processed packets (diagnostics, see <see cref="MDNSBrowser" />).</summary>
    public long Packets { get; private set; }

    /// <summary>Number of packets without records: queries of other hosts, other opcodes or invalid packets.</summary>
    public long PacketsWithoutRecords { get; private set; }

    /// <summary>Number of new records that were not cached because the cache was full (<see cref="MaxRecords" />).</summary>
    public long RecordsNotCached { get; private set; }

    private static string Key(MDNSRecord record) => $"{DNSNameComparer.ToLower(record.Name)}|{record.Type}|{record.Identity}";

    private static (string Name, ushort Type) RRset(MDNSRecord record) => (DNSNameComparer.ToLower(record.Name), record.Type);

    // Hosts of the cached SRV records of the browsed services, with the number of SRV records (see IsBrowsed: the
    // addresses of these hosts are cached when the cache is full)
    private readonly Dictionary<string, int> _browsedSrvTargets = new(DNSNameComparer.Instance);

    private void Set(string key, Entry entry)
    {
        if (_records.TryAdd(key, entry))
        {
            var rrset = RRset(entry.Record);

            if (!_rrsets.TryGetValue(rrset, out var keys))
                _rrsets[rrset] = keys = new HashSet<string>(StringComparer.Ordinal);

            keys.Add(key);

            if (entry.Record.Type == MDNSMessage.SRV && IsBrowsedInstance(entry.Record.Name))
                _browsedSrvTargets[entry.Record.Target] = _browsedSrvTargets.GetValueOrDefault(entry.Record.Target) + 1;

            Version++;

            return;
        }

        // Same key = same owner, type and value; the flag can change
        if (_records[key].Record.CacheFlush != entry.Record.CacheFlush)
            Version++;

        _records[key] = entry;
    }

    private void Remove(string key)
    {
        if (!_records.Remove(key, out var entry))
            return;

        _rowValues.Remove(key);

        var rrset = RRset(entry.Record);

        if (_rrsets.TryGetValue(rrset, out var keys) && keys.Remove(key) && keys.Count == 0)
            _rrsets.Remove(rrset);

        // Only the SRV records counted by Set (of browsed instances)
        if (entry.Record.Type == MDNSMessage.SRV && IsBrowsedInstance(entry.Record.Name) &&
            _browsedSrvTargets.TryGetValue(entry.Record.Target, out var count))
        {
            if (count > 1)
                _browsedSrvTargets[entry.Record.Target] = count - 1;
            else
                _browsedSrvTargets.Remove(entry.Record.Target);
        }

        Version++;
    }

    /// <summary>
    ///     Adds the records of a response. Expired records are removed by the Get methods (once per tick of the browser,
    ///     not per packet).
    /// </summary>
    public void ProcessResponse(byte[] message, DateTimeOffset now, long ipv6ScopeId = 0)
    {
        // The Records view also keeps reverse names and other owners seen on the link. Service browsing and
        // follow-up questions remain restricted to the requested local DNS-SD service types.
        var records = MDNSMessage.ReadResponse(message, ipv6ScopeId);

        Packets++;

        if (records.Count == 0)
            PacketsWithoutRecords++;

        // Hosts of the SRV records of browsed services in this packet (an address can come before its SRV record),
        // only read if the cache is full (see IsBrowsed); the cached ones are kept in _browsedSrvTargets
        HashSet<string> packetSrvTargets = null;

        HashSet<string> GetPacketSrvTargets() => packetSrvTargets ??= records
            .Where(x => x.Type == MDNSMessage.SRV && IsBrowsedInstance(x.Name)).Select(x => x.Target)
            .ToHashSet(DNSNameComparer.Instance);

        // RRsets with a cache-flush record of this packet that was cached (not if the cache is full: the old values are
        // kept instead of losing the RRset)
        HashSet<(string Name, ushort Type)> flushedRRsets = null;

        foreach (var record in records)
        {
            var key = Key(record);
            if (record.TTL == 0)
            {
                if (_records.TryGetValue(key, out var entry))
                    _records[key] = entry with { Expires = Min(entry.Expires, now.AddSeconds(1)) };
            }
            // Full: records of the browsed services are still cached (up to twice the limit), so other traffic of the
            // link (reverse names, other services) can't hide them
            else if (_records.ContainsKey(key) || _records.Count < MaxRecords ||
                     _records.Count < MaxRecords * 2 && IsBrowsed(record, GetPacketSrvTargets))
            {
                Set(key, new Entry(record, now, now.AddSeconds(record.TTL)));

                if (record.CacheFlush)
                    (flushedRRsets ??= []).Add(RRset(record));
            }
            else if (RecordsNotCached++ == 0)
                Log.Warn($"mDNS cache on \"{_networkInterface}\" is full ({MaxRecords} records), new records of other services are ignored until others expire.");
        }

        if (flushedRRsets == null)
            return;

        // A cache-flush RRset may span packets. Give older values one second's grace, and preserve values received in
        // the same second (RFC 6762 section 10.2), e.g. the ones of this packet; a goodbye is also held for one second.
        foreach (var rrset in flushedRRsets)
        {
            foreach (var key in _rrsets[rrset])
            {
                var old = _records[key];

                if (now - old.Received >= TimeSpan.FromSeconds(1))
                    _records[key] = old with { Expires = Min(old.Expires, now.AddSeconds(1)) };
            }
        }
    }

    /// <summary>
    ///     Checks if a record belongs to the browsed services: PTR of a browsed type (or of the service type
    ///     enumeration with *), SRV / TXT of an instance of a browsed type, A / AAAA of a host of such an SRV record
    ///     (cached or in the packet).
    /// </summary>
    private bool IsBrowsed(MDNSRecord record, Func<HashSet<string>> getPacketSrvTargets)
    {
        switch (record.Type)
        {
            case MDNSMessage.PTR:
                if (BrowsesAll && DNSNameComparer.Instance.Equals(record.Name, MDNSServiceType.Enumeration))
                    return true;

                return MDNSServiceType.TryNormalize(record.Name, out var type) && type != "*" &&
                       (BrowsesAll || _serviceTypes.Contains(type));
            case MDNSMessage.SRV:
            case MDNSMessage.TXT:
                return IsBrowsedInstance(record.Name);
            case MDNSMessage.A:
            case MDNSMessage.AAAA:
                return _browsedSrvTargets.ContainsKey(record.Name) || getPacketSrvTargets().Contains(record.Name);
            default:
                return false;
        }
    }

    /// <summary>
    ///     Checks if a name is an instance of a browsed type: the instance is the first label, the rest of the name is
    ///     its type.
    /// </summary>
    private bool IsBrowsedInstance(string name)
    {
        var separator = FirstUnescapedDot(name);

        return separator > 0 && MDNSServiceType.TryNormalize(name[(separator + 1)..], out var type) && type != "*" &&
               (BrowsesAll || _serviceTypes.Contains(type));
    }

    /// <summary>
    ///     Gets the index of the first dot that separates labels (not escaped by a backslash), -1 if there is none.
    /// </summary>
    private static int FirstUnescapedDot(string name)
    {
        for (var i = 0; i < name.Length; i++)
        {
            if (name[i] == '\\')
                i++; // escaped character
            else if (name[i] == '.')
                return i;
        }

        return -1;
    }

    private static DateTimeOffset Min(DateTimeOffset first, DateTimeOffset second) => first < second ? first : second;

    // Time of the last Expire: the browser and the Get methods expire with the same time of a tick, only once
    private DateTimeOffset _lastExpire = DateTimeOffset.MinValue;

    /// <summary>
    ///     Removes the expired records (also done by the Get methods). Records added since expire later than their
    ///     time of receipt (at least one second), so a second call with the same time has nothing to do.
    /// </summary>
    public void Expire(DateTimeOffset now)
    {
        if (now == _lastExpire)
            return;

        _lastExpire = now;

        foreach (var key in _records.Where(x => x.Value.Expires <= now).Select(x => x.Key).ToArray())
            Remove(key);
    }

    /// <summary>
    ///     PTR records of the browsed service types that point to an instance of the type, with the normalized type
    ///     (the owner name of the record can differ, e.g. without ".local." or with spaces, see
    ///     <see cref="MDNSServiceType.TryNormalize" />).
    /// </summary>
    private IEnumerable<(Entry Entry, string Type)> Instances => _records.Values
        .Select(x => (Entry: x,
            Type: x.Record.Type == MDNSMessage.PTR && MDNSServiceType.TryNormalize(x.Record.Name, out var type)
                ? type
                : null))
        .Where(x => x.Type != null && x.Type != "*" && (BrowsesAll || _serviceTypes.Contains(x.Type)) &&
                    IsInstanceOf(x.Entry.Record.Target, x.Type));

    /// <summary>
    ///     Checks if a name (escaped presentation format) is an instance of the type: "&lt;instance&gt;.&lt;type&gt;" with an
    ///     unescaped dot before the type - "x\._ipp._tcp.local." is the label "x._ipp" in "_tcp.local.", not an instance
    ///     of "_ipp._tcp.local.".
    /// </summary>
    private static bool IsInstanceOf(string name, string type)
    {
        if (name.Length <= type.Length + 1 || !DNSNameComparer.EndsWith(name, "." + type))
            return false;

        // An even number of backslashes before the dot escape each other, not the dot
        var backslashes = 0;
        for (var i = name.Length - type.Length - 2; i >= 0 && name[i] == '\\'; i--)
            backslashes++;

        return backslashes % 2 == 0;
    }

    /// <summary>Maximum interval of a repeated question in seconds (RFC 6762 5.2: 60 minutes).</summary>
    private const int MaxQuestionInterval = 3600;

    /// <summary>
    ///     Returns at most 256 due questions per tick (RFC 6762 5.2). The interval of a question doubles from 1 second
    ///     up to <see cref="MaxQuestionInterval" />. An answered question is asked again at 80 % of the remaining TTL of
    ///     its records (refresh before they expire): SRV / TXT / A / AAAA (one answer) only then, PTR (several
    ///     answers, new instances can appear) also on its interval.
    /// </summary>
    public IReadOnlyList<(string Name, ushort Type)> GetQuestions(DateTimeOffset now)
    {
        Expire(now);
        var wanted = new Dictionary<(string Name, ushort Type), string>();
        void Add(string name, ushort type) => wanted.TryAdd((DNSNameComparer.ToLower(name), type), name);
        if (BrowsesAll)
        {
            Add(MDNSServiceType.Enumeration, MDNSMessage.PTR);
            foreach (var type in CommonServiceTypes)
                Add(type, MDNSMessage.PTR);
            foreach (var record in _records.Values.Where(x => x.Record.Type == MDNSMessage.PTR &&
                         DNSNameComparer.Instance.Equals(x.Record.Name, MDNSServiceType.Enumeration)))
                if (MDNSServiceType.TryNormalize(record.Record.Target, out var type) && type != "*")
                    Add(type, MDNSMessage.PTR);
        }
        else
            foreach (var type in _serviceTypes)
                Add(type, MDNSMessage.PTR);

        var instances = Instances.ToArray();
        var instanceNames = instances.Select(x => x.Entry.Record.Target).ToHashSet(DNSNameComparer.Instance);
        foreach (var (instance, _) in instances)
        {
            Add(instance.Record.Target, MDNSMessage.SRV);
            Add(instance.Record.Target, MDNSMessage.TXT);
        }
        foreach (var record in _records.Values.Where(x => x.Record.Type == MDNSMessage.SRV && instanceNames.Contains(x.Record.Name)))
        {
            // In .local (an unescaped dot before "local.": "host\\.local." is one label under the root)
            if (!IsInstanceOf(record.Record.Target, "local."))
                continue;
            Add(record.Record.Target, MDNSMessage.A);
            Add(record.Record.Target, MDNSMessage.AAAA);
        }

        foreach (var key in _questions.Keys.Where(x => !wanted.ContainsKey(x)).ToArray())
            _questions.Remove(key);
        var lifetimes = _records.Values.ToLookup(x => (DNSNameComparer.ToLower(x.Record.Name), x.Record.Type));
        var due = new List<(string Name, ushort Type)>();
        foreach (var key in wanted.Keys)
        {
            // An answered SRV / TXT / A / AAAA question only from 80 % of the TTL of its records (also if it was
            // scheduled earlier, before the answer arrived)
            if (key.Type != MDNSMessage.PTR && lifetimes[key].Any() &&
                lifetimes[key].Min(x => x.Received + (x.Expires - x.Received) * 0.8) > now)
                continue;

            if (_questions.TryGetValue(key, out var question) && question.Next > now)
                continue;
            if (due.Count == 256)
                break;
            due.Add((wanted[key], key.Type));
            var interval = Math.Min(MaxQuestionInterval, question == null ? 1 : question.Interval * 2);
            var cached = lifetimes[key].ToArray();
            double next = interval;

            if (cached.Length > 0)
            {
                // Refresh the records before they expire (also when the interval is longer)
                var refresh = cached.Min(x => (x.Expires - now).TotalSeconds) * 0.8;

                next = key.Type == MDNSMessage.PTR ? Math.Min(interval, refresh) : refresh;
            }

            _questions[key] = new Question(now.AddSeconds(Math.Max(1, next)), interval);
        }
        return due;
    }

    /// <summary>
    ///     Gets the known answers of a question (RFC 6762 7.1): the cached PTR records of the name with more than half of
    ///     their TTL remaining, so the responders don't send them again. Other questions are only asked when their
    ///     records are about to expire (see <see cref="GetQuestions" />), so they have none.
    /// </summary>
    /// <param name="name">Name of the question.</param>
    /// <param name="type">Type of the question.</param>
    /// <param name="now">Current time.</param>
    /// <returns>Targets and remaining TTLs (seconds).</returns>
    public IReadOnlyList<(string Target, uint TTL)> GetKnownAnswers(string name, ushort type, DateTimeOffset now)
    {
        if (type != MDNSMessage.PTR || !_rrsets.TryGetValue((DNSNameComparer.ToLower(name), type), out var keys))
            return [];

        return keys.Select(x => _records[x])
            .Where(x => (x.Expires - now).TotalSeconds > x.Record.TTL / 2.0)
            .Select(x => (x.Record.Target, (uint)(x.Expires - now).TotalSeconds)).ToArray();
    }

    /// <summary>Creates detached snapshots, including instances whose SRV/address resolution is still pending.</summary>
    public IReadOnlyList<MDNSServiceInfo> GetServices(DateTimeOffset now)
    {
        Expire(now);
        var result = new List<MDNSServiceInfo>();
        var byName = _records.Values.ToLookup(x => x.Record.Name, DNSNameComparer.Instance);
        foreach (var (instance, type) in Instances)
        {
            var fullName = instance.Record.Target;
            var srv = byName[fullName].Where(x => x.Record.Type == MDNSMessage.SRV)
                .OrderByDescending(x => x.Received).FirstOrDefault();
            var txt = byName[fullName].Where(x => x.Record.Type == MDNSMessage.TXT)
                .OrderByDescending(x => x.Received).FirstOrDefault();
            var addresses = srv == null ? [] : byName[srv.Record.Target].Where(x => x.Record.Address != null)
                .Select(x => x.Record.Address.ToString()).Distinct().Order().ToArray();
            // Without the type (its normalized length, matched by Instances), as text (escapes decoded)
            var name = MDNSMessage.ToDisplayText(fullName[..^(type.Length + 1)]);
            result.Add(new MDNSServiceInfo(fullName, name, type, _networkInterface,
                srv?.Record.Target ?? string.Empty, srv?.Record.Port, srv?.Record.Priority, srv?.Record.Weight,
                string.Join("; ", addresses), string.Join(Environment.NewLine, txt?.Record.Text ?? []),
                (uint)Math.Max(0, Math.Ceiling((instance.Expires - now).TotalSeconds))));
        }
        return result;
    }

    /// <summary>
    ///     Maximum number of rows of <see cref="GetRecords" />: as many as the cache can hold (see
    ///     <see cref="ProcessResponse" />), TXT strings are rows of their own.
    /// </summary>
    public const int MaxRecordRows = MaxRecords * 2;

    /// <summary>
    ///     One row per resource record, with each character string of a TXT record on its own row, in the order of the
    ///     keys. At most <see cref="MaxRecordRows" /> rows: above, the records of the browsed services are kept first, so
    ///     every service has its records. The same rows are shown in every snapshot.
    /// </summary>
    public IReadOnlyList<MDNSRecordInfo> GetRecords(DateTimeOffset now)
    {
        Expire(now);

        var records = _records.OrderBy(x => x.Key, StringComparer.Ordinal).ToArray();
        var rows = records.SelectMany(ToRows).ToArray();

        if (rows.Length <= MaxRecordRows)
            return rows;

        var noPacketSrvTargets = new HashSet<string>();

        return records.Where(x => IsBrowsed(x.Value.Record, () => noPacketSrvTargets))
            .Concat(records.Where(x => !IsBrowsed(x.Value.Record, () => noPacketSrvTargets)))
            .SelectMany(ToRows).Take(MaxRecordRows).OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();

        IEnumerable<MDNSRecordInfo> ToRows(KeyValuePair<string, Entry> pair)
        {
            var record = pair.Value.Record;

            // Formatted once per record, not for every snapshot (e.g. the hex of a large record of an unknown type)
            if (!_rowValues.TryGetValue(pair.Key, out var row))
                _rowValues[pair.Key] = row = CreateRowValues(pair.Key, record);

            var ttl = (uint)Math.Max(0, Math.Ceiling((pair.Value.Expires - now).TotalSeconds));
            return row.Values.Select((value, index) => new MDNSRecordInfo(row.Ids[index], record.Name, row.Type, value,
                _networkInterface, record.CacheFlush, ttl));
        }
    }

    // Type, values and IDs of the rows of a record (see GetRecords), by key: the same key has the same values
    private sealed record RowValues(string Type, string[] Values, string[] Ids);

    private readonly Dictionary<string, RowValues> _rowValues = new(StringComparer.Ordinal);

    private static RowValues CreateRowValues(string key, MDNSRecord record)
    {
        var type = Enum.IsDefined((DnsClient.QueryType)record.Type)
            ? ((DnsClient.QueryType)record.Type).ToString()
            : $"TYPE{record.Type}";
        var values = record.Type switch
        {
            MDNSMessage.PTR => new[] { record.Target },
            MDNSMessage.SRV => [$"{record.Priority} {record.Weight} {record.Port} {record.Target}"],
            MDNSMessage.TXT => record.Text is { Length: > 0 } ? record.Text : [string.Empty],
            _ => [record.Address?.ToString() ?? $"\\# {record.RawData?.Length ?? 0} {Convert.ToHexString(record.RawData ?? [])}".TrimEnd()]
        };

        return new RowValues(type, values, values.Select((_, index) => $"{key}|{index}").ToArray());
    }
}
