using System.Collections.Generic;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of an <see cref="IPNetworkInfo" />.
    /// </summary>
    private static readonly ExportField<IPNetworkInfo>[] IPNetworkInfoExportFields =
    [
        new(nameof(IPNetworkInfo.Network), x => x.Network),
        new(nameof(IPNetworkInfo.Broadcast), x => x.Broadcast),
        new(nameof(IPNetworkInfo.Total), x => x.Total),
        new(nameof(IPNetworkInfo.Netmask), x => x.Netmask),
        new(nameof(IPNetworkInfo.Cidr), x => x.Cidr),
        new(nameof(IPNetworkInfo.FirstUsable), x => x.FirstUsable),
        new(nameof(IPNetworkInfo.LastUsable), x => x.LastUsable),
        new(nameof(IPNetworkInfo.Usable), x => x.Usable)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="IPNetworkInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{IPNetworkInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<IPNetworkInfo> collection)
    {
        Export(filePath, fileType, collection, IPNetworkInfoExportFields, ApplicationName.SubnetCalculator);
    }
}
