using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using Xunit;

namespace NETworkManager.Tests.Network;

public class MDNSDiscoveryTests
{
    private const string Type = "_ipp._tcp.local.";
    private const string Instance = "Office Printer._ipp._tcp.local.";
    private const string Host = "printer.local.";
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 12, 0, 0, TimeSpan.Zero);

    private sealed record Record(string Name, ushort Type, byte[] Data, uint TTL = 120, bool Flush = false);

    private static byte[] Name(string name) => Labels(name.TrimEnd('.').Split('.'));
    private static byte[] Labels(params string[] labels) =>
        [.. labels.SelectMany(x => new[] { (byte)Encoding.UTF8.GetByteCount(x) }.Concat(Encoding.UTF8.GetBytes(x))), 0];

    private static byte[] Packet(params Record[] records)
    {
        var bytes = new List<byte> { 0, 0, 0x84, 0, 0, 0, (byte)(records.Length >> 8), (byte)records.Length, 0, 0, 0, 0 };
        foreach (var record in records)
        {
            bytes.AddRange(Name(record.Name));
            bytes.AddRange([(byte)(record.Type >> 8), (byte)record.Type, record.Flush ? (byte)0x80 : (byte)0, 1,
                (byte)(record.TTL >> 24), (byte)(record.TTL >> 16), (byte)(record.TTL >> 8), (byte)record.TTL,
                (byte)(record.Data.Length >> 8), (byte)record.Data.Length]);
            bytes.AddRange(record.Data);
        }
        return [.. bytes];
    }

    private static Record Ptr(uint ttl = 120) => new(Type, 12, Name(Instance), ttl);
    private static Record Srv() => new(Instance, 33, [0, 10, 0, 20, 2, 119, .. Name(Host)], Flush: true);
    private static Record Address(string address = "192.168.1.20", bool flush = false) =>
        new(Host, (ushort)(address.Contains(':') ? 28 : 1), IPAddress.Parse(address).GetAddressBytes(), Flush: flush);

    [Theory]
    [InlineData("*", "*")]
    [InlineData(" _IPP._TCP ", Type)]
    [InlineData("_ipp._tcp.local.", Type)]
    [InlineData("_pdl-datastream._tcp", "_pdl-datastream._tcp.local.")]
    [InlineData("_foo1._udp.local", "_foo1._udp.local.")]
    [InlineData("_1a-2._tcp", "_1a-2._tcp.local.")]
    [InlineData("_a._udp", "_a._udp.local.")]
    public void NormalizesLocalServiceTypes(string input, string expected)
    {
        Assert.True(MDNSServiceType.TryNormalize(input, out var type));
        Assert.Equal(expected, type);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("_ipp._tcp.example.com")]
    [InlineData("ipp._tcp")]
    [InlineData("_ipp._sctp")]
    [InlineData("_123._tcp")]
    [InlineData("_-ipp._tcp")]
    [InlineData("_ip--p._tcp")]
    [InlineData("_ipp-._tcp")]
    [InlineData("_1-2._tcp")]
    [InlineData("_ipp._tcp.local.local")]
    [InlineData("_1234567890123456._tcp")]
    [InlineData("_http._tcp;_ipp._tcp")]
    [InlineData("_Kerberos._tcp")] // Kelvin sign, not "k"
    public void RejectsInvalidAndNonLocalServiceTypes(string input) =>
        Assert.False(MDNSServiceType.TryNormalize(input, out _));

    [Fact]
    public void BrowsingQueriesHaveZeroIdAndMulticastINClass()
    {
        var query = MDNSMessage.CreateQuery(Type, 12);
        Assert.Equal(new byte[] { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 }, query[..12]);
        Assert.Equal(Name(Type), query[12..^4]);
        Assert.Equal(new byte[] { 0, 12, 0, 1 }, query[^4..]);
        Assert.Empty(MDNSMessage.ReadResponse(query));
    }

    [Fact]
    public void ReadsCompressedPtrAndAdditionalRecords()
    {
        // The instance suffix points back to the PTR owner's service type at offset 12.
        var packet = Packet(new Record(Type, 12, [14, .. Encoding.UTF8.GetBytes("Office Printer"), 0xC0, 12]), Srv(), Address());
        // Move the SRV and address from the answer section into the additional section.
        packet[7] = 1;
        packet[11] = 2;
        var records = MDNSMessage.ReadResponse(packet);
        Assert.Equal(3, records.Count);
        Assert.Equal(Instance, records[0].Target);
        Assert.Equal((ushort)631, records[1].Port);
        Assert.Equal((ushort)10, records[1].Priority);
        Assert.Equal((ushort)20, records[1].Weight);
        Assert.True(records[1].CacheFlush);
        Assert.Equal(IPAddress.Parse("192.168.1.20"), records[2].Address);
    }

    [Fact]
    public void InstanceLabelsWithDotsBackslashesAndUnicodeRoundTripIntoFollowupQuestion()
    {
        var instanceWire = Labels("Büro.Printer\\One", "_ipp", "_tcp", "local");
        var record = Assert.Single(MDNSMessage.ReadResponse(Packet(new Record(Type, 12, instanceWire))));
        var query = MDNSMessage.CreateQuery(record.Target, 33);
        Assert.Equal(instanceWire, query[12..^4]);
    }

    [Theory]
    [InlineData(new byte[] { 0x42, 0xFC, 0x72, 0x6F }, "B\\252ro")] // Latin-1 "Büro", not valid UTF-8
    [InlineData(new byte[] { 0x61, 0x01, 0x62 }, "a\\001b")] // control character
    [InlineData(new byte[] { 0x61, 0x2E, 0xFF }, "a\\.\\255")] // dot and invalid byte
    public void KeepsLabelsThatAreNoUTF8TextAsBytesAndTheOtherRecords(byte[] label, string expected)
    {
        var instance = new byte[] { (byte)label.Length }.Concat(label).Concat(Name(Type)).ToArray();
        var records = MDNSMessage.ReadResponse(Packet(new Record(Type, 12, instance), Address()));

        // The other records of the packet are not lost
        Assert.Equal(2, records.Count);
        Assert.Equal(expected + "." + Type, records[0].Target);

        // A question for the name asks for the same bytes
        Assert.Equal(instance, MDNSMessage.CreateQuery(records[0].Target, 33)[12..^4]);
    }

    [Theory]
    [InlineData(@"My\.Printer", "My.Printer")]
    [InlineData(@"x\\", @"x\")]
    [InlineData(@"B\252ro", "Büro")] // Latin-1 (no valid UTF-8)
    [InlineData(@"B\195\188ro", "Büro")] // UTF-8 bytes
    [InlineData(@"a\001b", @"a\001b")] // control character: kept escaped
    [InlineData("Büro", "Büro")]
    public void InstanceNamesAreShownAsText(string name, string expected) =>
        Assert.Equal(expected, MDNSMessage.ToDisplayText(name));

    [Fact]
    public void ServiceNameOfALatin1InstanceIsShownAsText()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(new Record(Type, 12, [4, 0x42, 0xFC, 0x72, 0x6F, .. Name(Type)])), Now);
        var service = Assert.Single(cache.GetServices(Now));
        Assert.Equal("Büro", service.Name);
        Assert.Equal(@"B\252ro." + Type, service.FullName); // technical name: escaped
    }

    [Fact]
    public void PreservesBinaryTXTAndEscapesControlCharacters()
    {
        var txt = new Record(Instance, 16, [3, 97, 0, 98, 2, 0xFF, 0xFE, 0]);
        var record = Assert.Single(MDNSMessage.ReadResponse(Packet(txt)));
        Assert.Equal(new[] { "a\\u0000b", "\\xFF\\xFE", "" }, record.Text);
    }

    [Fact]
    public void RejectsAllTruncatedPrefixesAndPointerLoops()
    {
        var packet = Packet(Ptr(), Srv(), Address());
        for (var length = 0; length < packet.Length; length++)
            Assert.Empty(MDNSMessage.ReadResponse(packet[..length]));
        packet = Packet(new Record(Type, 12, [0xC0, 12]));
        // Owner name points to itself.
        packet[12] = 0xC0;
        packet[13] = 12;
        Assert.Empty(MDNSMessage.ReadResponse(packet));
        Assert.Empty(MDNSMessage.ReadResponse(Packet(new Record(Type, 12, [0xFF, 0xFF]))));
        Assert.Empty(MDNSMessage.ReadResponse(Packet(new Record(Instance, 16, [20, 1]))));
        Assert.Empty(MDNSMessage.ReadResponse(Packet(new Record(Instance, 33, [0, 1]))));
    }

    [Fact]
    public void VersionChangesOnlyWhenTheCacheChanges()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr(2)), Now);
        var version = cache.Version;
        cache.ProcessResponse(Packet(Ptr(2)), Now.AddSeconds(1)); // announced again
        Assert.Equal(version, cache.Version);
        cache.ProcessResponse(Packet(Srv()), Now.AddSeconds(1));
        Assert.NotEqual(version, version = cache.Version);
        cache.Expire(Now.AddSeconds(3)); // PTR expired
        Assert.NotEqual(version, cache.Version);
        Assert.Empty(cache.GetServices(Now.AddSeconds(3)));
    }

    [Fact]
    public void IgnoresTheTruncatedBitOfResponses()
    {
        var packet = Packet(Ptr());
        packet[2] |= 0x02; // TC
        Assert.Single(MDNSMessage.ReadResponse(packet));
    }

    [Fact]
    public void MergesOutOfOrderResponsesAndReturnsUnresolvedInstances()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr()), Now);
        var pending = Assert.Single(cache.GetServices(Now));
        Assert.Null(pending.Port);
        Assert.Empty(pending.Hostname);
        Assert.Contains((Instance, (ushort)33), cache.GetQuestions(Now));
        cache.ProcessResponse(Packet(Address("fe80::1234"), Address()), Now, 7);
        cache.ProcessResponse(Packet(Srv()), Now);
        var result = Assert.Single(cache.GetServices(Now));
        Assert.Equal("Office Printer", result.Name);
        Assert.Equal(Host, result.Hostname);
        Assert.Equal((ushort)631, result.Port);
        Assert.Contains("192.168.1.20", result.Addresses);
        Assert.Contains("fe80::1234%7", result.Addresses);
        Assert.Equal("Ethernet", result.NetworkInterface);
        Assert.Single(cache.GetServices(Now));
    }

    [Fact]
    public void EnumeratesUnknownTypesAndSeedsPrinters()
    {
        var cache = new MDNSServiceCache("*", "Ethernet");
        var questions = cache.GetQuestions(Now);
        Assert.Contains((MDNSServiceType.Enumeration, (ushort)12), questions);
        Assert.Contains((Type, (ushort)12), questions);
        cache.ProcessResponse(Packet(new Record(MDNSServiceType.Enumeration, 12, Name("_custom._udp.local."))), Now);
        Assert.Contains(("_custom._udp.local.", (ushort)12), cache.GetQuestions(Now));
        Assert.Empty(cache.GetQuestions(Now));
        Assert.NotEmpty(cache.GetQuestions(Now.AddSeconds(1)));
    }

    [Theory]
    [InlineData(" _ipp._tcp.local.")] // spaces are allowed in a label, the type is trimmed
    [InlineData("_ipp._tcp.")] // without .local.
    [InlineData("_IPP._TCP.LOCAL.")]
    public void InstanceNameIsCutByTheNormalizedType(string owner)
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(new Record(owner, 12, Name(Instance))), Now);
        var service = Assert.Single(cache.GetServices(Now));
        Assert.Equal("Office Printer", service.Name);
        Assert.Equal(Type, service.ServiceType);
    }

    [Fact]
    public void EscapedDotBeforeTheTypeIsNoInstance()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        // Labels "x._ipp", "_tcp", "local" (the dot is part of the first label)
        cache.ProcessResponse(Packet(new Record(Type, 12, Labels("x._ipp", "_tcp", "local"))), Now);
        Assert.Empty(cache.GetServices(Now));
        // Labels "x\", "_ipp", "_tcp", "local": an instance named "x\" (escaped backslash, then a real dot)
        cache.ProcessResponse(Packet(new Record(Type, 12, Labels("x\\", "_ipp", "_tcp", "local"))), Now);
        Assert.Equal("x\\", Assert.Single(cache.GetServices(Now)).Name);
    }

    [Fact]
    public void BrowsesSeveralServiceTypes()
    {
        var cache = new MDNSServiceCache(["_ipp._tcp", "_http._tcp.local."], "Ethernet");
        Assert.Equal(new[] { ("_http._tcp.local.", (ushort)12), (Type, (ushort)12) },
            cache.GetQuestions(Now).Order());
        cache.ProcessResponse(Packet(Ptr(), new Record("_http._tcp.local.", 12, Name("Web._http._tcp.local.")),
            new Record("_ssh._tcp.local.", 12, Name("Server._ssh._tcp.local."))), Now);
        Assert.Equal(new[] { "_http._tcp.local.", Type }, cache.GetServices(Now).Select(x => x.ServiceType).Order());
    }

    [Fact]
    public void AllServiceTypesIncludeTheOthers()
    {
        var cache = new MDNSServiceCache(["_ipp._tcp", "*"], "Ethernet");
        Assert.Contains((MDNSServiceType.Enumeration, (ushort)12), cache.GetQuestions(Now));
    }

    [Fact]
    public void RejectsNoOrInvalidServiceTypes()
    {
        Assert.Throws<ArgumentException>(() => new MDNSServiceCache(Array.Empty<string>(), "Ethernet"));
        Assert.Throws<ArgumentException>(() => new MDNSServiceCache(["_ipp._tcp", "printers"], "Ethernet"));
    }

    [Fact]
    public void FiltersServicesByRequestedTypeAndHonorsTTL()
    {
        var cache = new MDNSServiceCache("_http._tcp", "Ethernet");
        cache.ProcessResponse(Packet(Ptr()), Now);
        Assert.Empty(cache.GetServices(Now));
        cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr(2)), Now);
        Assert.Single(cache.GetServices(Now));
        Assert.Empty(cache.GetServices(Now.AddSeconds(2)));
    }

    [Fact]
    public void GoodbyeRemovesServiceAfterOneSecondGrace()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr(), Srv(), Address()), Now);
        cache.ProcessResponse(Packet(Ptr(0)), Now.AddSeconds(1));
        Assert.Single(cache.GetServices(Now.AddSeconds(1.5)));
        Assert.Empty(cache.GetServices(Now.AddSeconds(2)));
    }

    [Fact]
    public void CacheFlushReplacesOldAddressAndPreservesRRSetAcrossPackets()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr(), Srv(), Address()), Now);
        cache.ProcessResponse(Packet(Address("192.168.1.30", true)), Now.AddSeconds(2));
        cache.ProcessResponse(Packet(Address("192.168.1.31", true)), Now.AddSeconds(2.1));
        var result = Assert.Single(cache.GetServices(Now.AddSeconds(3.2)));
        Assert.DoesNotContain("192.168.1.20", result.Addresses);
        Assert.Contains("192.168.1.30", result.Addresses);
        Assert.Contains("192.168.1.31", result.Addresses);
    }

    [Fact]
    public void RepeatedAnnouncementsDeduplicateAndRefreshTTL()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr(2)), Now);
        cache.ProcessResponse(Packet(Ptr(2)), Now.AddSeconds(1));
        Assert.Single(cache.GetServices(Now.AddSeconds(2.5)));
        Assert.Empty(cache.GetServices(Now.AddSeconds(3)));
    }

    [Fact]
    public void RecordsViewHasOneLinePerResourceRecordWithTTLAndType()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr(), Srv(), Address(), Address("fe80::1234"),
            new Record(Instance, 16, [4, 107, 61, 97, 10])), Now, 7);
        var records = cache.GetRecords(Now.AddSeconds(1));
        Assert.Equal(5, records.Count);
        Assert.Equal(new[] { "A", "AAAA", "PTR", "SRV", "TXT" }, records.Select(x => x.Type).Order());
        Assert.All(records, x =>
        {
            Assert.Equal("IN", x.Class);
            Assert.Equal((uint)119, x.TTL);
            Assert.DoesNotContain('\n', x.Value);
            Assert.DoesNotContain('\r', x.Value);
        });
        Assert.Equal("10 20 631 printer.local.", Assert.Single(records, x => x.Type == "SRV").Value);
        Assert.Equal("k=a\\u000A", Assert.Single(records, x => x.Type == "TXT").Value);
    }

    [Fact]
    public void TXTValuesAreCaseSensitiveAndReplacedAfterCacheFlushGrace()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(new Record(Instance, 16, [3, 107, 61, 97], Flush: true)), Now);
        cache.ProcessResponse(Packet(new Record(Instance, 16, [3, 107, 61, 65], Flush: true)), Now.AddSeconds(2));
        Assert.Equal(2, cache.GetRecords(Now.AddSeconds(2)).Count);
        Assert.Equal("k=A", Assert.Single(cache.GetRecords(Now.AddSeconds(3))).Value);
    }

    [Fact]
    public void FollowupQuestionsPreserveInstanceNameCaseAndIgnoreUnrelatedSRVRecords()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr(), new Record("Unrelated._http._tcp.local.", 33, [0, 0, 0, 0, 0, 80, .. Name("other.local.")])), Now);
        var questions = cache.GetQuestions(Now);
        Assert.Contains((Instance, (ushort)33), questions);
        Assert.DoesNotContain(questions, x => x.Name == "other.local.");
    }

    [Fact]
    public void EachTXTEntryHasItsOwnStableRowAndSharedRecordTTL()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        var txt = new Record(Instance, 16, [9, .. Encoding.UTF8.GetBytes("txtvers=1"),
            10, .. Encoding.UTF8.GetBytes("ty=Printer"), 12, .. Encoding.UTF8.GetBytes("rp=ipp/print")]);
        cache.ProcessResponse(Packet(txt), Now);
        var rows = cache.GetRecords(Now);
        Assert.Equal(new[] { "txtvers=1", "ty=Printer", "rp=ipp/print" }, rows.Select(x => x.Value));
        Assert.Equal(3, rows.Select(x => x.Id).Distinct().Count());
        Assert.All(rows, x => { Assert.Equal("TXT", x.Type); Assert.Equal((uint)120, x.TTL); });
        cache.ProcessResponse(Packet(txt), Now.AddSeconds(1));
        Assert.Equal(rows.Select(x => x.Id), cache.GetRecords(Now.AddSeconds(2)).Select(x => x.Id));
        Assert.All(cache.GetRecords(Now.AddSeconds(2)), x => Assert.Equal((uint)119, x.TTL));
    }

    [Fact]
    public void LargeTXTResponseHasBoundedRowsAndCompactUniqueIds()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        // Two TXT records with more empty strings than rows can be shown (packets up to 9000 bytes)
        cache.ProcessResponse(Packet(new Record(Instance, 16, new byte[8000])), Now);
        cache.ProcessResponse(Packet(new Record("Other._ipp._tcp.local.", 16, new byte[8000])), Now);
        var rows = cache.GetRecords(Now);
        Assert.Equal(MDNSServiceCache.MaxRecordRows, rows.Count);
        Assert.Equal(rows.Count, rows.Select(x => x.Id).Distinct().Count());
        Assert.All(rows, x => Assert.True(x.Id.Length < 500));
    }

    [Fact]
    public void UnknownRecordTypesArePreservedAndExpireLikeOtherRecords()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        var packet = Packet(new Record(Host, 65400, [0x00, 0xFF, 0x42, 0x80], TTL: 2));
        var parsed = Assert.Single(MDNSMessage.ReadResponse(packet));
        Assert.Equal(new byte[] { 0x00, 0xFF, 0x42, 0x80 }, parsed.RawData);
        cache.ProcessResponse(packet, Now);
        var row = Assert.Single(cache.GetRecords(Now));
        Assert.Equal("TYPE65400", row.Type);
        Assert.Equal("\\# 4 00FF4280", row.Value);
        Assert.Empty(cache.GetServices(Now));
        Assert.Empty(cache.GetRecords(Now.AddSeconds(2)));
    }

    [Fact]
    public void UnsupportedStandardRecordsAreVisibleAlongsideServiceRecords()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr(), Srv(), Address(), new Record(Host, 47, [0, 0, 1, 0x40])), Now);
        var records = cache.GetRecords(Now);
        Assert.Equal(4, records.Count);
        Assert.Contains(records, x => x.Type == "NSEC" && x.Value == "\\# 4 00000140");
        Assert.Single(cache.GetServices(Now));
    }

    [Fact]
    public void UnknownZeroLengthRecordsRemainVisible()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(new Record(Host, 65401, [])), Now);
        Assert.Equal("\\# 0", Assert.Single(cache.GetRecords(Now)).Value);
    }

    [Fact]
    public void RecordsViewAlsoPreservesReverseOwnersWithoutStartingUnrelatedQueries()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(new Record("20.1.168.192.in-addr.arpa.", 65400, [0x42])), Now);
        Assert.Equal("20.1.168.192.in-addr.arpa.", Assert.Single(cache.GetRecords(Now)).Name);
        Assert.Equal(new[] { (Type, (ushort)12) }, cache.GetQuestions(Now));
    }

    // Query building

    [Theory]
    [InlineData("a\\256b.local.")] // \DDD above 255
    [InlineData("a\\999.local.")]
    [InlineData("")]
    [InlineData("a..local.")]
    [InlineData("trailing\\")]
    public void CreateQueryRejectsEmptyLabelsAndOpenEscapes(string name) =>
        Assert.Throws<ArgumentException>(() => MDNSMessage.CreateQuery(name, 12));

    [Fact]
    public void CreateQueryKeepsAnEscapedDotAtTheEndOfTheLastLabel()
    {
        Assert.Equal(Labels("a."), MDNSMessage.CreateQuery("a\\..", 12)[12..^4]);
        Assert.Equal(Labels("a."), MDNSMessage.CreateQuery("a\\.", 12)[12..^4]);
        Assert.Equal(Labels("a\\"), MDNSMessage.CreateQuery("a\\\\.", 12)[12..^4]);
        Assert.Equal(Labels("local"), MDNSMessage.CreateQuery("local", 12)[12..^4]);
    }

    [Fact]
    public void CreateQueryRejectsLoneSurrogates()
    {
        // In code: InlineData strings are stored as UTF-8 (a lone surrogate would arrive as U+FFFD)
        Assert.Throws<ArgumentException>(() => MDNSMessage.CreateQuery("a\uD800.local.", 12)); // would take the dot
        Assert.Throws<ArgumentException>(() => MDNSMessage.CreateQuery("a\uDC00.local.", 12));
        Assert.Equal(Labels("a\U0001F5A8", "local"), MDNSMessage.CreateQuery("a\U0001F5A8.local.", 12)[12..^4]);
    }

    [Fact]
    public void CreateQueryRejectsLabelsLongerThan63Bytes() =>
        Assert.Throws<ArgumentException>(() => MDNSMessage.CreateQuery(new string('a', 64) + ".local.", 12));

    [Fact]
    public void CreateQueryRejectsNamesLongerThan255Bytes() =>
        Assert.Throws<ArgumentException>(() =>
            MDNSMessage.CreateQuery(string.Join('.', Enumerable.Repeat(new string('a', 63), 5)) + ".", 12));

    private static int Count(byte[] packet, int offset) => (packet[offset] << 8) | packet[offset + 1];

    // The known answers of a query, read like a response (QR bit set)
    private static IReadOnlyList<MDNSRecord> KnownAnswers(byte[] query)
    {
        var response = query.ToArray();
        response[2] = 0x84;
        return MDNSMessage.ReadResponse(response);
    }

    [Fact]
    public void QueriesContainSeveralQuestionsWithCompressedNames()
    {
        var packet = Assert.Single(MDNSMessage.CreateQueries([(Type, 12), ("_IPPS._tcp.local.", 12), (Instance, 33)]));

        Assert.Equal(3, Count(packet, 4));
        Assert.Equal(0, Count(packet, 6));
        // First name in full, "_tcp.local." of the second and the type of the instance as pointers
        Assert.Equal(Name(Type), packet[12..(12 + Name(Type).Length)]);
        Assert.Equal(12 + Name(Type).Length + 4 + 1 + 5 + 2 + 4 + 1 + 14 + 2 + 4, packet.Length);
    }

    [Fact]
    public void QueriesContainTheKnownAnswersAfterTheQuestions()
    {
        var packet = Assert.Single(MDNSMessage.CreateQueries([(Type, 12), (Instance, 33)],
            (name, type) => type == 12 ? [(Instance, 100), ("Other._ipp._tcp.local.", 4000)] : []));

        Assert.Equal(2, Count(packet, 4));
        Assert.Equal(2, Count(packet, 6));
        var answers = KnownAnswers(packet);
        Assert.Equal(new[] { Instance, "Other._ipp._tcp.local." }, answers.Select(x => x.Target));
        Assert.All(answers, x => Assert.Equal((Type, (ushort)12, false), (x.Name, x.Type, x.CacheFlush)));
        Assert.Equal(new uint[] { 100, 4000 }, answers.Select(x => x.TTL));
    }

    [Fact]
    public void QueriesAreSplitIntoPacketsThatFitTheMinimumIPv6MTU()
    {
        var questions = Enumerable.Range(0, 100)
            .Select(i => ($"Printer {i} on the second floor._ipp._tcp.local.", (ushort)33)).ToArray();
        var packets = MDNSMessage.CreateQueries(questions);

        Assert.True(packets.Count > 1);
        Assert.All(packets, x => Assert.True(x.Length <= MDNSMessage.MaxQuerySize));
        Assert.Equal(100, packets.Sum(x => Count(x, 4)));
    }

    private static bool IsTruncated(byte[] packet) => (packet[2] & 0x02) != 0;

    [Fact]
    public void KnownAnswersThatDoNotFitFollowInPacketsWithTheTCBit()
    {
        var known = Enumerable.Range(0, 200).Select(i => ($"Printer {i}._ipp._tcp.local.", 120u)).ToArray();
        var packets = MDNSMessage.CreateQueries([(Type, 12), (Instance, 33)], (_, type) => type == 12 ? known : []);

        // The PTR question with known answers, packets with only known answers, then the next question
        Assert.True(packets.Count > 3);
        Assert.All(packets, x => Assert.True(x.Length <= MDNSMessage.MaxQuerySize));
        var question = packets.Take(packets.Count - 1).ToArray();
        Assert.Equal(1, Count(question[0], 4));
        Assert.All(question.Skip(1), x => Assert.Equal(0, Count(x, 4)));

        // TC bit on all but the last packet of the question, every known answer once
        Assert.All(question[..^1], x => Assert.True(IsTruncated(x)));
        Assert.False(IsTruncated(question[^1]));
        Assert.Equal(known.Select(x => x.Item1).Order(), question.SelectMany(KnownAnswers).Select(x => x.Target).Order());
        Assert.All(question.SelectMany(KnownAnswers), x => Assert.Equal((Type, (ushort)12, 120u), (x.Name, x.Type, x.TTL)));

        Assert.Equal(1, Count(packets[^1], 4));
        Assert.Equal(0, Count(packets[^1], 6));
        Assert.False(IsTruncated(packets[^1]));
    }

    [Fact]
    public void KnownAnswersOfAQuestionUseAtMostMaxKnownAnswerPackets()
    {
        var known = Enumerable.Range(0, 5000).Select(i => ($"Printer {i}._ipp._tcp.local.", 120u)).ToArray();
        var packets = MDNSMessage.CreateQueries([(Type, 12)], (_, _) => known);

        Assert.Equal(MDNSMessage.MaxKnownAnswerPackets, packets.Count);
        Assert.False(IsTruncated(packets[^1])); // no more known answers follow
        Assert.InRange(packets.Sum(x => Count(x, 6)), 100, 4999);
    }

    [Fact]
    public void KnownAnswersArePTRRecordsWithMoreThanHalfOfTheirTTL()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr(120), Srv()), Now);

        Assert.Equal(new[] { (Instance, 61u) }, cache.GetKnownAnswers("_IPP._tcp.local.", 12, Now.AddSeconds(59)));
        Assert.Empty(cache.GetKnownAnswers(Type, 12, Now.AddSeconds(61)));
        Assert.Empty(cache.GetKnownAnswers(Instance, 33, Now));
    }

    // Response parsing

    [Fact]
    public void IgnoresQueriesAndOtherOpcodesAndErrors()
    {
        var packet = Packet(Ptr());
        packet[2] = 0x00; // query (QR = 0)
        Assert.Empty(MDNSMessage.ReadResponse(packet));
        packet = Packet(Ptr());
        packet[2] = 0x84 | 0x08; // opcode 1
        Assert.Empty(MDNSMessage.ReadResponse(packet));
        packet = Packet(Ptr());
        packet[3] = 0x03; // rcode 3 (NXDOMAIN)
        Assert.Empty(MDNSMessage.ReadResponse(packet));
    }

    [Fact]
    public void IgnoresRecordsOfOtherClassesButKeepsTheRestOfThePacket()
    {
        var packet = Packet(Address(), Ptr());
        // Class of the first record (A of printer.local.): CH (3) instead of IN
        var classOffset = 12 + Name(Host).Length + 2;
        packet[classOffset + 1] = 3;
        var record = Assert.Single(MDNSMessage.ReadResponse(packet));
        Assert.Equal(MDNSMessage.PTR, record.Type);
    }

    [Fact]
    public void AddressRecordsWithAWrongLengthArePreservedAsRawData()
    {
        var record = Assert.Single(MDNSMessage.ReadResponse(Packet(new Record(Host, 1, [192, 168, 1]))));
        Assert.Null(record.Address);
        Assert.Equal(new byte[] { 192, 168, 1 }, record.RawData);
    }

    [Fact]
    public void LinkLocalIPv6AddressesGetTheScopeOfTheInterface()
    {
        var records = MDNSMessage.ReadResponse(Packet(Address("fe80::1"), Address("2001:db8::1")), 12);
        Assert.Equal(12, records[0].Address.ScopeId);
        Assert.Equal(0, records[1].Address.ScopeId);
    }

    [Fact]
    public void TTLsWithTheMostSignificantBitSetAreZero()
    {
        Assert.Equal((uint)0, Assert.Single(MDNSMessage.ReadResponse(Packet(Ptr(0xFFFFFFFF)))).TTL);
        Assert.Equal((uint)int.MaxValue, Assert.Single(MDNSMessage.ReadResponse(Packet(Ptr(int.MaxValue)))).TTL);

        // Not cached (like a goodbye)
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr(0x80000000)), Now);
        Assert.Empty(cache.GetServices(Now));
    }

    [Fact]
    public void PacketsLargerThan9000BytesAreIgnored()
    {
        var packet = Packet(Ptr(), new Record(Instance, 16, new byte[9000]));
        Assert.Empty(MDNSMessage.ReadResponse(packet));
        Assert.Single(MDNSMessage.ReadResponse(Packet(Ptr(), new Record(Instance, 16, new byte[8800]))), x => x.Type == 12);
    }

    // Cache

    [Fact]
    public void CountsPacketsAndPacketsWithoutRecords()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr()), Now);
        cache.ProcessResponse(MDNSMessage.CreateQuery(Type, 12), Now); // query of another host
        cache.ProcessResponse([1, 2, 3], Now); // invalid
        Assert.Equal(3, cache.Packets);
        Assert.Equal(2, cache.PacketsWithoutRecords);
    }

    [Fact]
    public void FullCacheIgnoresNewRecordsButRefreshesCachedOnes()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr(2)), Now);

        // Fill the cache with other records (A records of different hosts)
        var hosts = Enumerable.Range(0, MDNSServiceCache.MaxRecords - 1)
            .Select(i => new Record($"host{i}.local.", 1, [10, 0, (byte)(i >> 8), (byte)i])).ToArray();
        foreach (var chunk in hosts.Chunk(200))
            cache.ProcessResponse(Packet(chunk), Now);

        Assert.Equal(0, cache.RecordsNotCached);
        // An address of a host without SRV record (other traffic of the link) is not cached
        cache.ProcessResponse(Packet(Address("192.168.1.99")), Now);
        Assert.Equal(1, cache.RecordsNotCached);
        Assert.DoesNotContain(cache.GetRecords(Now), x => x.Value == "192.168.1.99");

        // The records of a browsed service are still cached (SRV of the instance, then the address of its host)
        cache.ProcessResponse(Packet(Srv()), Now);
        cache.ProcessResponse(Packet(Address("192.168.1.99")), Now);
        var service = Assert.Single(cache.GetServices(Now));
        Assert.Equal(Host, service.Hostname);
        Assert.Contains("192.168.1.99", service.Addresses);
        Assert.Equal(1, cache.RecordsNotCached);

        // The host of a service that is not browsed (SRV in the same packet) is not cached in the extra room
        cache.ProcessResponse(Packet(new Record("Speaker._airplay._tcp.local.", 33, [0, 0, 0, 0, 0x1B, 0x58, .. Name("speaker.local.")]),
            new Record("speaker.local.", 1, [192, 168, 1, 31])), Now);
        Assert.Equal(3, cache.RecordsNotCached);
        Assert.DoesNotContain(cache.GetRecords(Now), x => x.Value == "192.168.1.31");

        // The cached PTR (TTL 2) is still refreshed by a new announcement
        cache.ProcessResponse(Packet(Ptr(2)), Now.AddSeconds(1.5));
        Assert.Single(cache.GetServices(Now.AddSeconds(3)));
    }

    [Fact]
    public void InstanceNamesOnlyDifferingInTheCaseOfOtherThanAsciiLettersAreDifferentServices()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(new Record(Type, 12, Labels("Büro", "_ipp", "_tcp", "local")),
            new Record(Type, 12, Labels("BÜRO", "_ipp", "_tcp", "local")),
            new Record(Type, 12, Labels("büro", "_IPP", "_TCP", "LOCAL"))), Now); // same as the first (ASCII case)

        var names = cache.GetServices(Now).Select(x => x.Name).ToArray();
        Assert.Equal(2, names.Length);
        Assert.Contains("BÜRO", names);
    }

    [Fact]
    public void AddressRecordsWithAnInvalidLengthAreNoAddressesOfTheService()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        // A with 16 bytes and AAAA with 4 bytes: kept as raw data (no address)
        cache.ProcessResponse(Packet(Ptr(), Srv(), Address(), new Record(Host, 1, new byte[16]),
            new Record(Host, 28, [192, 168, 1, 21])), Now);

        Assert.Equal("192.168.1.20", Assert.Single(cache.GetServices(Now)).Addresses);
        Assert.Contains(cache.GetRecords(Now), x => x.Type == "A" && x.Value.StartsWith("\\# 16"));
    }

    [Fact]
    public void AnnouncedAgainTheServiceAndItsQuestionsUseTheNewTTL()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr(120), Srv()), Now);
        Assert.Equal((uint)120, Assert.Single(cache.GetServices(Now)).TTL);
        cache.GetQuestions(Now);

        // Same records again (the cache doesn't change its version), 60 s later
        var version = cache.Version;
        cache.ProcessResponse(Packet(Ptr(120), Srv()), Now.AddSeconds(60));
        Assert.Equal(version, cache.Version);
        Assert.Equal((uint)120, Assert.Single(cache.GetServices(Now.AddSeconds(60))).TTL);

        // The SRV question at 80 % of the new TTL (60 + 96 s), not of the first one
        Assert.DoesNotContain((Instance, (ushort)33), cache.GetQuestions(Now.AddSeconds(100)));
        Assert.Contains((Instance, (ushort)33), cache.GetQuestions(Now.AddSeconds(156)));
    }

    [Fact]
    public void FullCacheKeepsTheOldValuesOfACacheFlushThatIsNotCached()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        var hosts = Enumerable.Range(0, MDNSServiceCache.MaxRecords)
            .Select(i => new Record($"host{i}.local.", 1, [10, 0, (byte)(i >> 8), (byte)i])).ToArray();
        foreach (var chunk in hosts.Chunk(200))
            cache.ProcessResponse(Packet(chunk), Now);

        // New address of host0 with the cache-flush bit: not cached (full), the old one must not expire
        cache.ProcessResponse(Packet(new Record("host0.local.", 1, [10, 9, 9, 9], Flush: true)), Now.AddSeconds(2));
        Assert.Equal(1, cache.RecordsNotCached);
        Assert.Contains(cache.GetRecords(Now.AddSeconds(4)), x => x.Name == "host0.local." && x.Value == "10.0.0.0");
    }

    [Fact]
    public void RecordRowsKeepTheRecordsOfTheBrowsedServicesWhenThereAreTooMany()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr(), Srv(), Address()), Now);

        // TXT records of another service with more strings than rows can be shown (sorted before the printer)
        var strings = Enumerable.Range(0, 3000).SelectMany(_ => new byte[] { 1, (byte)'a' }).ToArray();
        foreach (var name in new[] { "A", "B", "C" })
            cache.ProcessResponse(Packet(new Record($"{name}._airplay._tcp.local.", 16, strings)), Now);

        var rows = cache.GetRecords(Now);
        Assert.Equal(MDNSServiceCache.MaxRecordRows, rows.Count);
        Assert.Contains(rows, x => x.Name == Type && x.Type == "PTR");
        Assert.Contains(rows, x => x.Name == Instance && x.Type == "SRV");
        Assert.Contains(rows, x => x.Name == Host && x.Value == "192.168.1.20");
        // In the order of the records, the strings of a TXT record in their order (not "|10" before "|2")
        var owners = rows.Select(x => x.Name).Distinct().ToArray();
        Assert.Equal(owners.OrderBy(DNSNameComparer.ToLower, StringComparer.Ordinal), owners); // key: lowercase name
        Assert.Equal(Enumerable.Range(0, 3000).Select(i => i.ToString()),
            rows.Where(x => x.Name == "A._airplay._tcp.local.").Select(x => x.Id.Split('|')[^1]));
    }

    [Fact]
    public void UnansweredQuestionsBackOffExponentially()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        var times = new List<double>();
        for (var second = 0.0; second <= 300; second += 0.25)
            if (cache.GetQuestions(Now.AddSeconds(second)).Count > 0)
                times.Add(second);

        // The interval doubles (1, 2, 4, ... s), not capped at 60 seconds (RFC 6762 5.2: up to 60 minutes)
        Assert.Equal(new[] { 0.0, 1, 3, 7, 15, 31, 63, 127, 255 }, times);
    }

    [Fact]
    public void AnsweredUniqueRecordsAreOnlyAskedAgainBeforeTheyExpire()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr(4500)), Now);
        Assert.Contains((Instance, (ushort)33), cache.GetQuestions(Now));
        cache.ProcessResponse(Packet(Srv()), Now); // TTL 120

        var srvQuestions = new List<double>();
        for (var second = 0.25; second <= 110; second += 0.25)
            if (cache.GetQuestions(Now.AddSeconds(second)).Contains((Instance, (ushort)33)))
                srvQuestions.Add(second);

        // Not on the backoff interval (1, 3, 7, ... s), only at 80 % of the TTL (96 s)
        Assert.Equal(new[] { 96.0 }, srvQuestions);
    }

    [Fact]
    public void QuestionsRefreshShortLivedRecordsBeforeTheyExpire()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.GetQuestions(Now);
        cache.ProcessResponse(Packet(Ptr(10)), Now);
        cache.GetQuestions(Now.AddSeconds(1)); // asked again (backoff 2 s)
        // Next question at 80 % of the remaining lifetime (9 s * 0.8), not after the backoff of the question
        Assert.DoesNotContain((Type, (ushort)12), cache.GetQuestions(Now.AddSeconds(2)));
        Assert.Contains((Type, (ushort)12), cache.GetQuestions(Now.AddSeconds(3)));
    }

    [Fact]
    public void NoAddressQuestionsForHostsOutsideTheLocalDomain()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Ptr(), new Record(Instance, 33, [0, 0, 0, 0, 0, 80, .. Name("printer.example.com.")])), Now);
        var questions = cache.GetQuestions(Now);
        Assert.DoesNotContain(questions, x => x.Name == "printer.example.com.");
    }

    [Fact]
    public void ResumedCacheKeepsValidRecordsAndAsksDueQuestionsRightAway()
    {
        // Paused browse (view hidden) continued 5 minutes later with the same cache
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.GetQuestions(Now);
        cache.ProcessResponse(Packet(Ptr(4500), Srv(), Address()), Now); // SRV / A: TTL 120
        cache.GetQuestions(Now.AddSeconds(1));

        var resumed = Now.AddMinutes(5);
        var service = Assert.Single(cache.GetServices(resumed));
        Assert.Empty(service.Hostname); // SRV expired during the pause
        Assert.Contains((Instance, (ushort)33), cache.GetQuestions(resumed));
    }

    [Fact]
    public void NoAddressQuestionsForAHostWithAnEscapedDotBeforeLocal()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        // Target: one label "host.local" under the root, not in .local
        cache.ProcessResponse(Packet(Ptr(), new Record(Instance, 33, [0, 0, 0, 0, 0, 80, .. Labels("host.local")])), Now);
        Assert.DoesNotContain(cache.GetQuestions(Now), x => x.Type is 1 or 28);
    }

    [Fact]
    public void RecordsAreReturnedInAStableOrder()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Address("192.168.1.3"), Ptr(), Address("192.168.1.1"), Srv()), Now);
        cache.ProcessResponse(Packet(Address("192.168.1.2")), Now);
        var first = cache.GetRecords(Now).Select(x => x.Id).ToArray();
        cache.ProcessResponse(Packet(Address("192.168.1.2")), Now.AddSeconds(1)); // announced again
        Assert.Equal(first, cache.GetRecords(Now.AddSeconds(1)).Select(x => x.Id));
        Assert.Equal(first.Order(StringComparer.Ordinal), first);
    }

    [Fact]
    public void VersionChangesWhenTheCacheFlushFlagChanges()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(Address()), Now);
        var version = cache.Version;
        cache.ProcessResponse(Packet(Address(flush: true)), Now.AddSeconds(1));
        Assert.NotEqual(version, cache.Version);
        Assert.True(Assert.Single(cache.GetRecords(Now.AddSeconds(1))).CacheFlush);
    }

    [Fact]
    public void ServiceNamesWithEscapedDotsAndBackslashesAreUnescaped()
    {
        var cache = new MDNSServiceCache(Type, "Ethernet");
        cache.ProcessResponse(Packet(new Record(Type, 12, Labels("Büro.Printer\\One", "_ipp", "_tcp", "local"))), Now);
        Assert.Equal("Büro.Printer\\One", Assert.Single(cache.GetServices(Now)).Name);
    }

    // Rows

    [Fact]
    public void ServiceUpdateRaisesOnlyTheChangedProperties()
    {
        var row = new MDNSServiceInfo(Instance, "Office Printer", Type, "Ethernet", Host, 631, 0, 0, "192.168.1.20", "a=1", 120);
        var changed = new List<string>();
        row.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        row.UpdateFrom(new MDNSServiceInfo(Instance, "Office Printer", Type, "Ethernet", Host, 631, 0, 0, "192.168.1.20", "a=1", 119));
        Assert.Equal(new[] { nameof(MDNSServiceInfo.TTL) }, changed);

        changed.Clear();
        row.UpdateFrom(new MDNSServiceInfo(Instance, "Office Printer", Type, "Ethernet", Host, 631, 0, 0, "192.168.1.20; fe80::1%7", "a=1", 119));
        Assert.Equal(new[] { nameof(MDNSServiceInfo.Addresses) }, changed);
        Assert.Equal("192.168.1.20; fe80::1%7", row.Addresses);

        changed.Clear();
        row.UpdateFrom(new MDNSServiceInfo(Instance, "Office Printer", Type, "Ethernet", Host, 631, 0, 0, "192.168.1.20; fe80::1%7", "a=1", 119));
        Assert.Empty(changed);
    }

    [Fact]
    public void RecordUpdateRaisesTTLAndCacheFlush()
    {
        var row = new MDNSRecordInfo("id", Host, "A", "192.168.1.20", "Ethernet", false, 120);
        var changed = new List<string>();
        row.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
        row.UpdateFrom(new MDNSRecordInfo("id", Host, "A", "192.168.1.20", "Ethernet", true, 119));
        Assert.Equal(new[] { nameof(MDNSRecordInfo.TTL), nameof(MDNSRecordInfo.CacheFlush) }, changed);
        Assert.True(row.CacheFlush);
        Assert.Equal((uint)119, row.TTL);
    }
}
