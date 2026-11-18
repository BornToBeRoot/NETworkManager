using System;
using System.Collections.Generic;
using System.Linq;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Per-link DNS-SD cache. Keeps independently arriving PTR/SRV/TXT/address records until their TTL expires,
///     observes goodbye/cache-flush records, and schedules follow-up questions with exponential backoff.
///     The browser serializes access; no UI objects are shared with it.
/// </summary>
public sealed class MDNSServiceCache
{
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
    private readonly string _serviceType;
    private readonly string _networkInterface;

    public MDNSServiceCache(string serviceType, string networkInterface)
    {
        if (!MDNSServiceType.TryNormalize(serviceType, out _serviceType))
            throw new ArgumentException("Invalid service type.", nameof(serviceType));
        _networkInterface = networkInterface;
    }

    /// <summary>
    ///     Incremented when a record is added, replaced by a different value or removed (not when a record is only
    ///     announced again). The browser creates a new snapshot only if it changed (or for the TTL countdown).
    /// </summary>
    public long Version { get; private set; }

    private static string Key(MDNSRecord record) => $"{record.Name.ToLowerInvariant()}|{record.Type}|{record.Identity}";

    private static (string Name, ushort Type) RRset(MDNSRecord record) => (record.Name.ToLowerInvariant(), record.Type);

    private void Set(string key, Entry entry)
    {
        if (_records.TryAdd(key, entry))
        {
            var rrset = RRset(entry.Record);

            if (!_rrsets.TryGetValue(rrset, out var keys))
                _rrsets[rrset] = keys = new HashSet<string>(StringComparer.Ordinal);

            keys.Add(key);
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

        var rrset = RRset(entry.Record);

        if (_rrsets.TryGetValue(rrset, out var keys) && keys.Remove(key) && keys.Count == 0)
            _rrsets.Remove(rrset);

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

        // A cache-flush RRset may span packets. Give older values one second's grace, and preserve values just
        // received in the same second (RFC 6762 section 10.2); a goodbye is also held for one second.
        foreach (var group in records.Where(x => x.CacheFlush && x.TTL > 0).GroupBy(RRset))
        {
            if (!_rrsets.TryGetValue(group.Key, out var cached))
                continue;

            var keys = group.Select(Key).ToHashSet(StringComparer.Ordinal);

            foreach (var key in cached.Where(x => !keys.Contains(x)).ToArray())
            {
                var old = _records[key];

                if (now - old.Received >= TimeSpan.FromSeconds(1))
                    _records[key] = old with { Expires = Min(old.Expires, now.AddSeconds(1)) };
            }
        }

        foreach (var record in records)
        {
            var key = Key(record);
            if (record.TTL == 0)
            {
                if (_records.TryGetValue(key, out var entry))
                    _records[key] = entry with { Expires = Min(entry.Expires, now.AddSeconds(1)) };
            }
            else if (_records.ContainsKey(key) || _records.Count < 4096)
                Set(key, new Entry(record, now, now.AddSeconds(record.TTL)));
        }
    }

    private static DateTimeOffset Min(DateTimeOffset first, DateTimeOffset second) => first < second ? first : second;

    /// <summary>Removes the expired records (also done by the Get methods).</summary>
    public void Expire(DateTimeOffset now)
    {
        foreach (var key in _records.Where(x => x.Value.Expires <= now).Select(x => x.Key).ToArray())
            Remove(key);
    }

    private IEnumerable<Entry> Instances => _records.Values.Where(x => x.Record.Type == MDNSMessage.PTR &&
        MDNSServiceType.TryNormalize(x.Record.Name, out var type) && type != "*" &&
        (_serviceType == "*" || type == _serviceType) &&
        x.Record.Target.EndsWith("." + type, StringComparison.OrdinalIgnoreCase) &&
        x.Record.Target.Length > type.Length + 1);

    /// <summary>Returns at most 256 due questions per tick; repeated unanswered queries back off to 60 seconds.</summary>
    public IReadOnlyList<(string Name, ushort Type)> GetQuestions(DateTimeOffset now)
    {
        Expire(now);
        var wanted = new Dictionary<(string Name, ushort Type), string>();
        void Add(string name, ushort type) => wanted.TryAdd((name.ToLowerInvariant(), type), name);
        if (_serviceType == "*")
        {
            Add(MDNSServiceType.Enumeration, MDNSMessage.PTR);
            foreach (var type in CommonServiceTypes)
                Add(type, MDNSMessage.PTR);
            foreach (var record in _records.Values.Where(x => x.Record.Type == MDNSMessage.PTR &&
                         x.Record.Name.Equals(MDNSServiceType.Enumeration, StringComparison.OrdinalIgnoreCase)))
                if (MDNSServiceType.TryNormalize(record.Record.Target, out var type) && type != "*")
                    Add(type, MDNSMessage.PTR);
        }
        else
            Add(_serviceType, MDNSMessage.PTR);

        var instances = Instances.ToArray();
        var instanceNames = instances.Select(x => x.Record.Target).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var instance in instances)
        {
            Add(instance.Record.Target, MDNSMessage.SRV);
            Add(instance.Record.Target, MDNSMessage.TXT);
        }
        foreach (var record in _records.Values.Where(x => x.Record.Type == MDNSMessage.SRV && instanceNames.Contains(x.Record.Name)))
        {
            if (!record.Record.Target.EndsWith(".local.", StringComparison.OrdinalIgnoreCase))
                continue;
            Add(record.Record.Target, MDNSMessage.A);
            Add(record.Record.Target, MDNSMessage.AAAA);
        }

        foreach (var key in _questions.Keys.Where(x => !wanted.ContainsKey(x)).ToArray())
            _questions.Remove(key);
        var lifetimes = _records.Values.ToLookup(x => (x.Record.Name.ToLowerInvariant(), x.Record.Type));
        var due = new List<(string Name, ushort Type)>();
        foreach (var key in wanted.Keys)
        {
            if (_questions.TryGetValue(key, out var question) && question.Next > now)
                continue;
            if (due.Count == 256)
                break;
            due.Add((wanted[key], key.Type));
            var interval = Math.Min(60, question == null ? 1 : question.Interval * 2);
            // Refresh short-lived records before expiry, even after the question has backed off.
            var lifetime = lifetimes[key]
                .Select(x => (x.Expires - now).TotalSeconds * 0.8).DefaultIfEmpty(interval).Min();
            _questions[key] = new Question(now.AddSeconds(Math.Max(1, Math.Min(interval, lifetime))), interval);
        }
        return due;
    }

    /// <summary>Creates detached snapshots, including instances whose SRV/address resolution is still pending.</summary>
    public IReadOnlyList<MDNSServiceInfo> GetServices(DateTimeOffset now)
    {
        Expire(now);
        var result = new List<MDNSServiceInfo>();
        var byName = _records.Values.ToLookup(x => x.Record.Name, StringComparer.OrdinalIgnoreCase);
        foreach (var instance in Instances)
        {
            var fullName = instance.Record.Target;
            var srv = byName[fullName].Where(x => x.Record.Type == MDNSMessage.SRV)
                .OrderByDescending(x => x.Received).FirstOrDefault();
            var txt = byName[fullName].Where(x => x.Record.Type == MDNSMessage.TXT)
                .OrderByDescending(x => x.Received).FirstOrDefault();
            var addresses = srv == null ? [] : byName[srv.Record.Target].Where(x => x.Record.Address != null)
                .Select(x => x.Record.Address.ToString()).Distinct().Order().ToArray();
            var name = fullName[..^(instance.Record.Name.Length + 1)].Replace("\\.", ".").Replace("\\\\", "\\");
            result.Add(new MDNSServiceInfo(fullName, name, instance.Record.Name, _networkInterface,
                srv?.Record.Target ?? string.Empty, srv?.Record.Port, srv?.Record.Priority, srv?.Record.Weight,
                string.Join("; ", addresses), string.Join(Environment.NewLine, txt?.Record.Text ?? []),
                (uint)Math.Max(0, Math.Ceiling((instance.Expires - now).TotalSeconds))));
        }
        return result;
    }

    /// <summary>
    ///     One row per resource record, with each character string of a TXT record on its own row. At most 4096 rows,
    ///     in the order of the keys, so the same rows are shown in every snapshot.
    /// </summary>
    public IReadOnlyList<MDNSRecordInfo> GetRecords(DateTimeOffset now)
    {
        Expire(now);
        return _records.OrderBy(x => x.Key, StringComparer.Ordinal).SelectMany(pair =>
        {
            var record = pair.Value.Record;
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
            var ttl = (uint)Math.Max(0, Math.Ceiling((pair.Value.Expires - now).TotalSeconds));
            return values.Select((value, index) => new MDNSRecordInfo($"{pair.Key}|{index}", record.Name, type, value,
                _networkInterface, record.CacheFlush, ttl));
        }).Take(4096).ToArray();
    }
}
