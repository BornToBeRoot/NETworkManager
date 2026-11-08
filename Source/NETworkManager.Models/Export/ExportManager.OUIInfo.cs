using System.Collections.Generic;
using NETworkManager.Models.Lookup;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of an <see cref="OUIInfo" />.
    /// </summary>
    private static readonly ExportField<OUIInfo>[] OUIInfoExportFields =
    [
        new(nameof(OUIInfo.MACAddress), x => x.MACAddress),
        new(nameof(OUIInfo.Vendor), x => x.Vendor, true)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="OUIInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{OUIInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<OUIInfo> collection)
    {
        Export(filePath, fileType, collection, OUIInfoExportFields, ApplicationName.Lookup);
    }
}
