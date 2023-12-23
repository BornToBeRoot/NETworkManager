using System;
using DnsClient;

namespace NETworkManager.Models.Network;

public class DNSLookupSettings
{
    public bool AddDNSSuffix { get; set; } = true;
    public bool UseCustomDNSSuffix { get; set; }
    public string CustomDNSSuffix { get; set; }
    public QueryClass QueryClass { get; set; } = QueryClass.IN;
    public QueryType QueryType { get; set; } = QueryType.ANY;

    public bool UseCache { get; set; }
    public bool Recursion { get; set; } = true;
    public bool UseTCPOnly { get; set; }
    public int Retries { get; set; } = 3;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(2);
}