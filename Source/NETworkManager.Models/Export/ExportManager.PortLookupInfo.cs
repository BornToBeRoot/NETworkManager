using System.Collections.Generic;
using NETworkManager.Models.Lookup;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="PortLookupInfo" />.
    /// </summary>
    private static readonly ExportField<PortLookupInfo>[] PortLookupInfoExportFields =
    [
        new(nameof(PortLookupInfo.Number), x => x.Number),
        new(nameof(PortLookupInfo.Protocol), x => x.Protocol),
        new(nameof(PortLookupInfo.Service), x => x.Service, true),
        new(nameof(PortLookupInfo.Description), x => x.Description, true)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="PortLookupInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{PortLookupInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<PortLookupInfo> collection)
    {
        Export(filePath, fileType, collection, PortLookupInfoExportFields, ApplicationName.Lookup);
    }
}
