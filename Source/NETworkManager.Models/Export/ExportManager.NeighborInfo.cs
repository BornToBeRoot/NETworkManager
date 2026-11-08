using System.Collections.Generic;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="NeighborInfo" />.
    /// </summary>
    private static readonly ExportField<NeighborInfo>[] NeighborInfoExportFields =
    [
        new(nameof(NeighborInfo.IPAddress), x => x.IPAddress),
        new(nameof(NeighborInfo.MACAddress), x => x.MACAddress),
        new(nameof(NeighborInfo.InterfaceAlias), x => x.InterfaceAlias, true),
        new(nameof(NeighborInfo.InterfaceIndex), x => x.InterfaceIndex),
        new(nameof(NeighborInfo.State), x => x.State),
        new(nameof(NeighborInfo.AddressFamily), x => x.AddressFamily),
        new(nameof(NeighborInfo.IsMulticast), x => x.IsMulticast)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="NeighborInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{NeighborInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<NeighborInfo> collection)
    {
        Export(filePath, fileType, collection, NeighborInfoExportFields, ApplicationName.NeighborTable);
    }
}
