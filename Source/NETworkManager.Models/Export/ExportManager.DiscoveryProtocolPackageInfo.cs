using System.Collections.Generic;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="DiscoveryProtocolPackageInfo" />. The released columns come first, new ones
    ///     are appended at the end, so the position of a column never changes.
    /// </summary>
    private static readonly ExportField<DiscoveryProtocolPackageInfo>[] DiscoveryProtocolPackageInfoExportFields =
    [
        new(nameof(DiscoveryProtocolPackageInfo.Device), x => x.Device, true),
        new(nameof(DiscoveryProtocolPackageInfo.DeviceDescription), x => x.DeviceDescription, true),
        new(nameof(DiscoveryProtocolPackageInfo.Port), x => x.Port, true),
        new(nameof(DiscoveryProtocolPackageInfo.PortDescription), x => x.PortDescription, true),
        new(nameof(DiscoveryProtocolPackageInfo.Model), x => x.Model, true),
        new(nameof(DiscoveryProtocolPackageInfo.VLAN), x => x.VLAN),
        new(nameof(DiscoveryProtocolPackageInfo.IPAddress), x => x.IPAddress, true),
        new(nameof(DiscoveryProtocolPackageInfo.Protocol), x => x.Protocol),
        new(nameof(DiscoveryProtocolPackageInfo.TimeToLive), x => x.TimeToLive),
        // Added later - after the released columns, so their position doesn't change
        new(nameof(DiscoveryProtocolPackageInfo.Management), x => x.Management, true),
        new(nameof(DiscoveryProtocolPackageInfo.ChassisId), x => x.ChassisId, true),
        new(nameof(DiscoveryProtocolPackageInfo.LocalConnection), x => x.LocalConnection, true),
        new(nameof(DiscoveryProtocolPackageInfo.LocalInterface), x => x.LocalInterface, true)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="DiscoveryProtocolPackageInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{DiscoveryProtocolPackageInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType,
        IReadOnlyList<DiscoveryProtocolPackageInfo> collection)
    {
        Export(filePath, fileType, collection, DiscoveryProtocolPackageInfoExportFields,
            ApplicationName.DiscoveryProtocol);
    }
}
