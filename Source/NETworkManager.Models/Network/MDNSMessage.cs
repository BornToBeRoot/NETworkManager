using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>A DNS record understood by the service browser. Names use escaped DNS presentation format.</summary>
public sealed record MDNSRecord(string Name, ushort Type, uint TTL, bool CacheFlush, string Target = "",
    ushort Port = 0, ushort Priority = 0, ushort Weight = 0, IPAddress Address = null, string[] Text = null,
    byte[] RawData = null)
{
    // Keep row identities compact even when a TXT record contains thousands of strings. Computed once: the record
    // doesn't change and the identity is part of the cache key of every record of every packet.
    public string Identity { get; } = $"{DNSNameComparer.ToLower(Target)}|{Port}|{Priority}|{Weight}|{Address}|" +
        (Text == null ? string.Empty : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\0', Text))))) + "|" +
        (RawData == null ? string.Empty : Convert.ToHexString(SHA256.HashData(RawData)));
}

/// <summary>Bounded DNS wire reader/writer. Decodes DNS-SD records and preserves other record types as opaque RDATA.</summary>
public static class MDNSMessage
{
    public const ushort A = 1, PTR = 12, TXT = 16, AAAA = 28, SRV = 33;

    /// <summary>
    ///     Maximum size of a query packet: fits the minimum IPv6 MTU (1280 bytes - 40 bytes IPv6 header - 8 bytes UDP
    ///     header), so no packet is fragmented on any link.
    /// </summary>
    public const int MaxQuerySize = 1232;

    /// <summary>Builds a multicast question (ID zero, class IN, multicast response).</summary>
    public static byte[] CreateQuery(string name, ushort type) => CreateQueries([(name, type)])[0];

    /// <summary>
    ///     Builds the packets of multicast questions (ID zero, class IN, multicast response): as many questions as fit
    ///     in one packet (<see cref="MaxQuerySize" />, names compressed), each with its known answers (RFC 6762 7.1), so
    ///     the responders don't send the records again that are cached. Known answers that don't fit with their
    ///     question alone are left out (the responders send them, like without known answers).
    /// </summary>
    /// <param name="questions">Questions (escaped presentation format names).</param>
    /// <param name="getKnownAnswers">
    ///     Gets the known answers of a question (PTR records: the target and the remaining TTL), may be null.
    /// </param>
    /// <returns>Packets in the order of the questions.</returns>
    public static IReadOnlyList<byte[]> CreateQueries(IReadOnlyList<(string Name, ushort Type)> questions,
        Func<string, ushort, IReadOnlyList<(string Target, uint TTL)>> getKnownAnswers = null)
    {
        var packets = new List<byte[]>();
        var current = new List<QueryQuestion>();

        foreach (var (name, type) in questions)
        {
            var question = new QueryQuestion(EncodeName(name), type,
                (getKnownAnswers?.Invoke(name, type) ?? []).Select(x => (Target: TryEncodeName(x.Target), x.TTL))
                .Where(x => x.Target != null).ToArray());

            if (TryBuildQuery([.. current, question], out _))
            {
                current.Add(question);
                continue;
            }

            if (current.Count > 0 && TryBuildQuery(current, out var packet))
                packets.Add(packet);

            current = [question];

            // Alone with too many known answers: as many as fit
            if (TryBuildQuery(current, out packet))
                continue;

            packets.Add(packet);
            current.Clear();
        }

        if (current.Count > 0 && TryBuildQuery(current, out var last))
            packets.Add(last);

        return packets;
    }

    private sealed record QueryQuestion(List<byte[]> Name, ushort Type, (List<byte[]> Target, uint TTL)[] KnownAnswers);

    /// <summary>
    ///     Builds a query packet: the questions, then their known answers while they fit. Names are compressed against
    ///     the names of the questions (their offsets don't change when an answer is added).
    /// </summary>
    /// <returns>True if the packet fits and contains all known answers.</returns>
    private static bool TryBuildQuery(IReadOnlyList<QueryQuestion> questions, out byte[] packet)
    {
        var data = new List<byte>(MaxQuerySize) { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        var offsets = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var question in questions)
        {
            WriteName(data, question.Name, offsets, true);
            data.AddRange([(byte)(question.Type >> 8), (byte)question.Type, 0, 1]);
        }

        var answers = 0;
        var complete = data.Count <= MaxQuerySize;

        foreach (var question in questions)
        {
            foreach (var (target, ttl) in question.KnownAnswers)
            {
                var rdata = new List<byte>();
                WriteName(rdata, target, offsets, false);

                var record = new List<byte>();
                WriteName(record, question.Name, offsets, false);
                record.AddRange([(byte)(question.Type >> 8), (byte)question.Type, 0, 1,
                    (byte)(ttl >> 24), (byte)(ttl >> 16), (byte)(ttl >> 8), (byte)ttl,
                    (byte)(rdata.Count >> 8), (byte)rdata.Count]);
                record.AddRange(rdata);

                if (data.Count + record.Count > MaxQuerySize)
                {
                    complete = false;
                    continue;
                }

                data.AddRange(record);
                answers++;
            }
        }

        data[4] = (byte)(questions.Count >> 8);
        data[5] = (byte)questions.Count;
        data[6] = (byte)(answers >> 8);
        data[7] = (byte)answers;

        packet = [.. data];

        return complete;
    }

    /// <summary>
    ///     Writes a name: its labels until a suffix that is already in the packet (a pointer to it, RFC 1035 4.1.4).
    /// </summary>
    /// <param name="data">Packet (questions) or a record that is added to the packet later (answers).</param>
    /// <param name="labels">Labels of the name.</param>
    /// <param name="offsets">Offsets of the suffixes in the packet (key: lowercase ASCII, DNS compares like that).</param>
    /// <param name="register">Add the suffixes to the offsets (only if <paramref name="data" /> is the packet).</param>
    private static void WriteName(List<byte> data, List<byte[]> labels, Dictionary<string, int> offsets, bool register)
    {
        for (var i = 0; i < labels.Count; i++)
        {
            var key = string.Join('.', labels.Skip(i).Select(x => Convert.ToHexString(x.Select(ToLowerAscii).ToArray())));

            if (offsets.TryGetValue(key, out var offset))
            {
                data.AddRange([(byte)(0xC0 | offset >> 8), (byte)offset]);
                return;
            }

            if (register && data.Count < 0x4000)
                offsets[key] = data.Count;

            data.Add((byte)labels[i].Length);
            data.AddRange(labels[i]);
        }

        data.Add(0);

        static byte ToLowerAscii(byte value) => value is >= (byte)'A' and <= (byte)'Z' ? (byte)(value | 0x20) : value;
    }

    private static List<byte[]> TryEncodeName(string name)
    {
        try
        {
            return EncodeName(name);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <summary>
    ///     Converts a name in the escaped presentation format (see <see cref="ReadResponse" />) to its labels.
    /// </summary>
    /// <exception cref="ArgumentException">The name is no valid DNS name.</exception>
    private static List<byte[]> EncodeName(string name)
    {
        var labels = new List<byte[]>();
        var label = new List<byte>();
        var escaped = false;
        var wireLength = 1;
        // Fully qualified: add the final dot if it is missing. Not TrimEnd: an escaped dot at the end of the last label
        // (e.g. a\.. = label "a.") is part of the label.
        var text = EndsWithUnescapedDot(name) ? name : name + ".";

        for (var i = 0; i < text.Length; i++)
        {
            // A character (also a surrogate pair) as UTF-8; a lone surrogate is no valid text
            if (char.IsSurrogate(text[i]) && !char.IsSurrogatePair(text, i))
                throw new ArgumentException("Invalid DNS name.", nameof(name));

            var length = char.IsSurrogatePair(text, i) ? 2 : 1;
            var bytes = Encoding.UTF8.GetBytes(text, i, length);

            if (escaped)
            {
                escaped = false;

                // \DDD: a byte in decimal (RFC 1035 presentation format, see ReadResponse), e.g. a Latin-1 label;
                // above 255 it is no byte (invalid, not the digits as text)
                if (i + 2 < text.Length && char.IsAsciiDigit(text[i]) && char.IsAsciiDigit(text[i + 1]) &&
                    char.IsAsciiDigit(text[i + 2]))
                {
                    var value = int.Parse(text.AsSpan(i, 3));

                    if (value > 255)
                        throw new ArgumentException("Invalid DNS name.", nameof(name));

                    label.Add((byte)value);
                    i += 2;
                    continue;
                }

                label.AddRange(bytes);
            }
            else if (text[i] == '\\')
                escaped = true;
            else if (text[i] != '.')
                label.AddRange(bytes);
            else
            {
                if (label.Count is < 1 or > 63)
                    throw new ArgumentException("Invalid DNS label.", nameof(name));
                wireLength += label.Count + 1;
                labels.Add([.. label]);
                label.Clear();
            }

            i += length - 1;
        }

        // Open escape or a last label without a dot (e.g. name\ becomes name\. with an escaped dot)
        if (escaped || label.Count > 0 || wireLength > 255)
            throw new ArgumentException("Invalid DNS name.", nameof(name));

        return labels;
    }

    private static bool EndsWithUnescapedDot(string name)
    {
        if (!name.EndsWith('.'))
            return false;

        // An even number of backslashes before the dot escape each other, not the dot
        var backslashes = 0;
        for (var i = name.Length - 2; i >= 0 && name[i] == '\\'; i--)
            backslashes++;

        return backslashes % 2 == 0;
    }

    /// <summary>
    ///     Maximum size of a received packet: a multicast DNS packet must not exceed 9000 bytes (RFC 6762 17). Larger ones
    ///     are ignored, so a single device can't fill the cache with records of up to 64 KB.
    /// </summary>
    public const int MaxPacketSize = 9000;

    /// <summary>
    ///     Rejects malformed/truncated packets atomically, including invalid compression and RDATA lengths, and packets
    ///     larger than <see cref="MaxPacketSize" />.
    /// </summary>
    public static IReadOnlyList<MDNSRecord> ReadResponse(byte[] data, long ipv6ScopeId = 0)
    {
        if (data.Length > MaxPacketSize)
            return [];

        try
        {
            var reader = new Reader(data);
            reader.U16();
            var flags = reader.U16();
            // Response with opcode 0 and rcode 0. The TC bit is ignored in multicast responses (RFC 6762 18.5).
            if ((flags & 0x8000) == 0 || (flags & 0x780F) != 0)
                return [];
            var questions = reader.U16();
            var answers = reader.U16();
            var authority = reader.U16();
            var additional = reader.U16();
            if (questions > 256 || answers + authority + additional > 4096)
                return [];
            for (var i = 0; i < questions; i++)
            {
                reader.Name();
                reader.Skip(4);
            }

            var records = new List<MDNSRecord>();
            for (var i = 0; i < answers + authority + additional; i++)
            {
                var name = reader.Name();
                var type = reader.U16();
                var recordClass = reader.U16();
                var ttl = reader.U32();
                var length = reader.U16();
                var end = reader.Position + length;
                reader.Require(length);
                MDNSRecord record = null;
                if ((recordClass & 0x7FFF) == 1)
                {
                    var flush = (recordClass & 0x8000) != 0;
                    switch (type)
                    {
                        case PTR:
                            record = new MDNSRecord(name, type, ttl, flush, reader.Name(end));
                            break;
                        case SRV:
                            var priority = reader.U16();
                            var weight = reader.U16();
                            var port = reader.U16();
                            record = new MDNSRecord(name, type, ttl, flush, reader.Name(end), port, priority, weight);
                            break;
                        case TXT:
                            var text = new List<string>();
                            while (reader.Position < end)
                            {
                                var size = reader.Byte();
                                if (reader.Position + size > end)
                                    throw new InvalidDataException();
                                text.Add(FormatText(reader.Bytes(size)));
                            }
                            record = new MDNSRecord(name, type, ttl, flush, Text: [.. text]);
                            break;
                        case A when length == 4:
                        case AAAA when length == 16:
                            var address = new IPAddress(reader.Bytes(length));
                            if (address.IsIPv6LinkLocal)
                                address.ScopeId = ipv6ScopeId;
                            record = new MDNSRecord(name, type, ttl, flush, Address: address);
                            break;
                        default:
                            // RFC 3597: preserve unsupported RDATA without assuming its internal format.
                            record = new MDNSRecord(name, type, ttl, flush, RawData: reader.Bytes(length));
                            break;
                    }
                }
                if (reader.Position > end || record != null && reader.Position != end)
                    throw new InvalidDataException();
                reader.Position = end;
                if (record != null)
                    records.Add(record);
            }
            return records;
        }
        catch (InvalidDataException)
        {
            return [];
        }
    }

    private static string FormatText(byte[] bytes)
    {
        string text;
        try { text = new UTF8Encoding(false, true).GetString(bytes); }
        catch (DecoderFallbackException) { return string.Concat(bytes.Select(x => $"\\x{x:X2}")); }
        return string.Concat(text.Select(x => char.IsControl(x) || x == '\\' ? $"\\u{(int)x:X4}" : x.ToString()));
    }

    /// <summary>
    ///     Converts a name in the escaped presentation format (e.g. the instance name "My\.Printer" or "B\252ro") to text
    ///     for display: the escapes are read back to bytes and decoded as UTF-8, or as Latin-1 if they are no valid
    ///     UTF-8 (e.g. "Büro" of an older device). If the text would contain control characters, the escaped name is
    ///     returned (nothing invisible is shown).
    /// </summary>
    public static string ToDisplayText(string name)
    {
        var bytes = new List<byte>();

        for (var i = 0; i < name.Length; i++)
        {
            if (name[i] == '\\' && i + 1 < name.Length)
            {
                // \DDD (byte) or an escaped character
                if (i + 3 < name.Length && char.IsAsciiDigit(name[i + 1]) && char.IsAsciiDigit(name[i + 2]) &&
                    char.IsAsciiDigit(name[i + 3]) && int.Parse(name.AsSpan(i + 1, 3)) <= 255)
                {
                    bytes.Add(byte.Parse(name.AsSpan(i + 1, 3)));
                    i += 3;
                    continue;
                }

                i++;
            }

            var length = char.IsSurrogatePair(name, i) ? 2 : 1;
            bytes.AddRange(Encoding.UTF8.GetBytes(name, i, length));
            i += length - 1;
        }

        string text;

        try
        {
            text = new UTF8Encoding(false, true).GetString([.. bytes]);
        }
        catch (DecoderFallbackException)
        {
            text = Encoding.Latin1.GetString([.. bytes]);
        }

        return text.Any(char.IsControl) ? name : text;
    }

    /// <summary>
    ///     Formats a label in the escaped presentation format: "\" and "." escaped. A label that is not valid UTF-8 (e.g.
    ///     Latin-1 of an older device) or contains a control character is kept with its bytes: printable ASCII as is,
    ///     other bytes as \DDD (decimal, RFC 1035), so the other records of the packet are not lost and a question for
    ///     the name asks for the same bytes (see <see cref="CreateQuery" />).
    /// </summary>
    private static string FormatLabel(byte[] data, int offset, int count)
    {
        try
        {
            var text = new UTF8Encoding(false, true).GetString(data, offset, count);

            if (!text.Any(char.IsControl))
                return text.Replace("\\", "\\\\").Replace(".", "\\.");
        }
        catch (DecoderFallbackException)
        {
            // Bytes, see below
        }

        var label = new StringBuilder();

        foreach (var value in data.AsSpan(offset, count))
        {
            label.Append(value switch
            {
                (byte)'\\' => "\\\\",
                (byte)'.' => "\\.",
                >= 0x20 and < 0x7F => ((char)value).ToString(),
                _ => $"\\{value:D3}"
            });
        }

        return label.ToString();
    }

    private sealed class Reader(byte[] data)
    {
        public int Position { get; set; }
        public void Require(int count)
        {
            if (count < 0 || Position + count > data.Length)
                throw new InvalidDataException();
        }
        public byte Byte() { Require(1); return data[Position++]; }
        public ushort U16() => (ushort)((Byte() << 8) | Byte());
        public uint U32() => ((uint)U16() << 16) | U16();
        public void Skip(int count) { Require(count); Position += count; }
        public byte[] Bytes(int count) { Require(count); var value = data[Position..(Position + count)]; Position += count; return value; }

        public string Name(int? end = null)
        {
            var labels = new List<string>();
            var position = Position;
            var next = -1;
            var jumps = 0;
            var wireLength = 1;
            while (true)
            {
                if (position >= data.Length || next < 0 && end.HasValue && position >= end.Value)
                    throw new InvalidDataException();
                var length = data[position++];
                if (length == 0)
                    break;
                if ((length & 0xC0) == 0xC0)
                {
                    if (position >= data.Length || next < 0 && end.HasValue && position >= end.Value || ++jumps > 32)
                        throw new InvalidDataException();
                    var pointer = ((length & 0x3F) << 8) | data[position++];
                    // DNS compression refers to a prior occurrence; reject forward/self references.
                    if (pointer >= position - 2)
                        throw new InvalidDataException();
                    if (next < 0)
                        next = position;
                    position = pointer;
                    continue;
                }
                if (length > 63 || position + length > data.Length ||
                    next < 0 && end.HasValue && position + length > end.Value || (wireLength += length + 1) > 255)
                    throw new InvalidDataException();
                labels.Add(FormatLabel(data, position, length));
                position += length;
            }
            Position = next < 0 ? position : next;
            return string.Join('.', labels) + ".";
        }
    }
}
