using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace NETworkManager.Models.Network;

/// <summary>A DNS record understood by the service browser. Names use escaped DNS presentation format.</summary>
public sealed record MDNSRecord(string Name, ushort Type, uint TTL, bool CacheFlush, string Target = "",
    ushort Port = 0, ushort Priority = 0, ushort Weight = 0, IPAddress Address = null, string[] Text = null,
    byte[] RawData = null)
{
    // Keep row identities compact even when a TXT record contains thousands of strings.
    public string Identity => $"{Target.ToLowerInvariant()}|{Port}|{Priority}|{Weight}|{Address}|" +
        (Text == null ? string.Empty : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\0', Text))))) + "|" +
        (RawData == null ? string.Empty : Convert.ToHexString(SHA256.HashData(RawData)));
}

/// <summary>Bounded DNS wire reader/writer. Decodes DNS-SD records and preserves other record types as opaque RDATA.</summary>
public static class MDNSMessage
{
    public const ushort A = 1, PTR = 12, TXT = 16, AAAA = 28, SRV = 33;

    /// <summary>Builds a multicast question (ID zero, class IN, multicast response).</summary>
    public static byte[] CreateQuery(string name, ushort type)
    {
        var data = new List<byte> { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 };
        var label = new StringBuilder();
        var escaped = false;
        foreach (var character in name.TrimEnd('.') + ".")
        {
            if (escaped)
            {
                label.Append(character);
                escaped = false;
            }
            else if (character == '\\')
                escaped = true;
            else if (character != '.')
                label.Append(character);
            else
            {
                var bytes = Encoding.UTF8.GetBytes(label.ToString());
                if (bytes.Length is < 1 or > 63)
                    throw new ArgumentException("Invalid DNS label.", nameof(name));
                data.Add((byte)bytes.Length);
                data.AddRange(bytes);
                label.Clear();
            }
        }

        if (escaped || data.Count > 266)
            throw new ArgumentException("Invalid DNS name.", nameof(name));
        data.AddRange([0, (byte)(type >> 8), (byte)type, 0, 1]);
        return [.. data];
    }

    /// <summary>Rejects malformed/truncated packets atomically, including invalid compression and RDATA lengths.</summary>
    public static IReadOnlyList<MDNSRecord> ReadResponse(byte[] data, long ipv6ScopeId = 0)
    {
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
                string label;
                try { label = new UTF8Encoding(false, true).GetString(data, position, length); }
                catch (DecoderFallbackException) { throw new InvalidDataException(); }
                if (label.Any(char.IsControl))
                    throw new InvalidDataException();
                label = label.Replace("\\", "\\\\").Replace(".", "\\.");
                labels.Add(label);
                position += length;
            }
            Position = next < 0 ? position : next;
            return string.Join('.', labels) + ".";
        }
    }
}
