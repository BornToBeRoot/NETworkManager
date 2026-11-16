using System;
using System.Collections.Generic;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>A cached DNS resource record or individual TXT entry for the Records view and export.</summary>
public sealed class MDNSRecordInfo : PropertyChangedBase
{
    public string Id { get; }
    public string Name { get; }
    public string Type { get; }
    public string Class => "IN";
    public string Value { get; }
    public string NetworkInterface { get; }
    public bool CacheFlush { get; private set; }
    public uint TTL
    {
        get;
        private set { if (value == field) return; field = value; OnPropertyChanged(); }
    }

    public MDNSRecordInfo(string id, string name, string type, string value, string networkInterface, bool cacheFlush, uint ttl)
    {
        Id = id;
        Name = name;
        Type = type;
        Value = value;
        NetworkInterface = networkInterface;
        CacheFlush = cacheFlush;
        TTL = ttl;
    }

    public void UpdateFrom(MDNSRecordInfo value)
    {
        TTL = value.TTL;
        if (CacheFlush == value.CacheFlush) return;
        CacheFlush = value.CacheFlush;
        OnPropertyChanged(nameof(CacheFlush));
    }
}

/// <summary>Detached results delivered together, so the services and records views describe the same cache.</summary>
public sealed record MDNSDiscoverySnapshot(IReadOnlyList<MDNSServiceInfo> Services, IReadOnlyList<MDNSRecordInfo> Records);
