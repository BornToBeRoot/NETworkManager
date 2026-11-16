using System;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>A discovered DNS-SD instance. Updated on the UI thread from independent cache snapshots.</summary>
public sealed class MDNSServiceInfo : PropertyChangedBase
{
    public string FullName { get; init; }
    public string Name { get; init; }
    public string ServiceType { get; init; }
    public string NetworkInterface { get; init; }
    public string Hostname { get; private set; }
    public ushort? Port { get; private set; }
    public ushort? Priority { get; private set; }
    public ushort? Weight { get; private set; }
    public string Addresses { get; private set; }
    public string TXTRecords { get; private set; }
    public uint TTL { get; private set; }

    public MDNSServiceInfo(string fullName, string name, string serviceType, string networkInterface,
        string hostname, ushort? port, ushort? priority, ushort? weight, string addresses, string txtRecords, uint ttl)
    {
        FullName = fullName;
        Name = name;
        ServiceType = serviceType;
        NetworkInterface = networkInterface;
        Hostname = hostname;
        Port = port;
        Priority = priority;
        Weight = weight;
        Addresses = addresses;
        TXTRecords = txtRecords;
        TTL = ttl;
    }

    public void UpdateFrom(MDNSServiceInfo value)
    {
        if (Hostname == value.Hostname && Port == value.Port && Priority == value.Priority && Weight == value.Weight &&
            Addresses == value.Addresses && TXTRecords == value.TXTRecords && TTL == value.TTL)
            return;
        Hostname = value.Hostname;
        Port = value.Port;
        Priority = value.Priority;
        Weight = value.Weight;
        Addresses = value.Addresses;
        TXTRecords = value.TXTRecords;
        TTL = value.TTL;
        OnPropertyChanged(string.Empty);
    }
}
