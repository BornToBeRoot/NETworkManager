using System;
using System.Collections.Generic;
using System.Linq;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="NetworkInterfaceInfo" /> in the order of the details: interface, IPv4 (incl.
    ///     DHCP), IPv6, DNS.
    /// </summary>
    private static readonly ExportField<NetworkInterfaceInfo>[] NetworkInterfaceInfoExportFields =
    [
        new(nameof(NetworkInterfaceInfo.Id), x => x.Id),
        new(nameof(NetworkInterfaceInfo.Name), x => x.Name, true),
        new(nameof(NetworkInterfaceInfo.Description), x => x.Description, true),
        new(nameof(NetworkInterfaceInfo.Type), x => x.Type),
        new(nameof(NetworkInterfaceInfo.PhysicalAddress), x => x.PhysicalAddress),
        new(nameof(NetworkInterfaceInfo.Status), x => x.Status),
        new(nameof(NetworkInterfaceInfo.IsOperational), x => x.IsOperational),
        // Bit/s
        new(nameof(NetworkInterfaceInfo.Speed), x => x.Speed),
        new(nameof(NetworkInterfaceInfo.Profile), x => x.Profile),
        new(nameof(NetworkInterfaceInfo.IPv4ProtocolAvailable), x => x.IPv4ProtocolAvailable),
        // IP address with prefix length (e.g. 192.168.1.10/24)
        new(nameof(NetworkInterfaceInfo.IPv4Address),
            x => x.IPv4Address?.Select(ip => $"{ip.Item1}/{Subnetmask.ConvertSubnetmaskToCidr(ip.Item2)}")),
        new(nameof(NetworkInterfaceInfo.IPv4Gateway), x => x.IPv4Gateway),
        new(nameof(NetworkInterfaceInfo.DhcpEnabled), x => x.DhcpEnabled),
        new(nameof(NetworkInterfaceInfo.DhcpServer), x => x.DhcpServer),
        // Empty without DHCP
        new(nameof(NetworkInterfaceInfo.DhcpLeaseObtained), x => GetDhcpLease(x.DhcpLeaseObtained)),
        new(nameof(NetworkInterfaceInfo.DhcpLeaseExpires), x => GetDhcpLease(x.DhcpLeaseExpires)),
        new(nameof(NetworkInterfaceInfo.IPv6ProtocolAvailable), x => x.IPv6ProtocolAvailable),
        new(nameof(NetworkInterfaceInfo.IPv6AddressLinkLocal), x => x.IPv6AddressLinkLocal),
        new(nameof(NetworkInterfaceInfo.IPv6Address), x => x.IPv6Address),
        new(nameof(NetworkInterfaceInfo.IPv6Gateway), x => x.IPv6Gateway),
        new(nameof(NetworkInterfaceInfo.DNSAutoconfigurationEnabled), x => x.DNSAutoconfigurationEnabled),
        new(nameof(NetworkInterfaceInfo.DNSSuffix), x => x.DNSSuffix, true),
        new(nameof(NetworkInterfaceInfo.DNSServer), x => x.DNSServer)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="NetworkInterfaceInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{NetworkInterfaceInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<NetworkInterfaceInfo> collection)
    {
        Export(filePath, fileType, collection, NetworkInterfaceInfoExportFields, ApplicationName.NetworkInterface);
    }

    /// <summary>
    ///     Gets the time of a DHCP lease, null if not set (no DHCP).
    /// </summary>
    private static DateTime? GetDhcpLease(DateTime dateTime)
    {
        return dateTime == DateTime.MinValue ? null : dateTime;
    }
}
