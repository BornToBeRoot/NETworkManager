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

    /// <summary>
    ///     Takes the values of a newer snapshot. Only changed properties are raised: live filtering re-filters a row
    ///     only for its filtered properties (not every second for the TTL countdown).
    /// </summary>
    public void UpdateFrom(MDNSServiceInfo value)
    {
        if (Hostname != value.Hostname) { Hostname = value.Hostname; OnPropertyChanged(nameof(Hostname)); }
        if (Port != value.Port) { Port = value.Port; OnPropertyChanged(nameof(Port)); }
        if (Priority != value.Priority) { Priority = value.Priority; OnPropertyChanged(nameof(Priority)); }
        if (Weight != value.Weight) { Weight = value.Weight; OnPropertyChanged(nameof(Weight)); }
        if (Addresses != value.Addresses) { Addresses = value.Addresses; OnPropertyChanged(nameof(Addresses)); }
        if (TXTRecords != value.TXTRecords) { TXTRecords = value.TXTRecords; OnPropertyChanged(nameof(TXTRecords)); }
        if (TTL != value.TTL) { TTL = value.TTL; OnPropertyChanged(nameof(TTL)); }
    }
}
