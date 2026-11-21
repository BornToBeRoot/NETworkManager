using System.Collections.Generic;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="LLDPCDPPackageInfo" /> in the order shown.
    /// </summary>
    private static readonly ExportField<LLDPCDPPackageInfo>[] LLDPCDPPackageInfoExportFields =
    [
        new(nameof(LLDPCDPPackageInfo.Device), x => x.Device, true),
        new(nameof(LLDPCDPPackageInfo.DeviceDescription), x => x.DeviceDescription, true),
        new(nameof(LLDPCDPPackageInfo.Port), x => x.Port, true),
        new(nameof(LLDPCDPPackageInfo.PortDescription), x => x.PortDescription, true),
        new(nameof(LLDPCDPPackageInfo.Model), x => x.Model, true),
        new(nameof(LLDPCDPPackageInfo.VLAN), x => x.VLAN),
        new(nameof(LLDPCDPPackageInfo.IPAddress), x => x.IPAddress, true),
        new(nameof(LLDPCDPPackageInfo.Management), x => x.Management, true),
        new(nameof(LLDPCDPPackageInfo.ChassisId), x => x.ChassisId, true),
        new(nameof(LLDPCDPPackageInfo.Protocol), x => x.Protocol),
        new(nameof(LLDPCDPPackageInfo.TimeToLive), x => x.TimeToLive),
        new(nameof(LLDPCDPPackageInfo.LocalConnection), x => x.LocalConnection, true),
        new(nameof(LLDPCDPPackageInfo.LocalInterface), x => x.LocalInterface, true)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="LLDPCDPPackageInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{LLDPCDPPackageInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType,
        IReadOnlyList<LLDPCDPPackageInfo> collection)
    {
        Export(filePath, fileType, collection, LLDPCDPPackageInfoExportFields,
            ApplicationName.LLDPCDPCapture);
    }
}
