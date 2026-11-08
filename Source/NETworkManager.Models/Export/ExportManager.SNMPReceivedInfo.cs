using System.Collections.Generic;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="SNMPInfo" />.
    /// </summary>
    private static readonly ExportField<SNMPInfo>[] SNMPInfoExportFields =
    [
        new(nameof(SNMPInfo.OID), x => x.OID),
        new(nameof(SNMPInfo.Data), x => x.Data, true)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="SNMPInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{SNMPInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<SNMPInfo> collection)
    {
        Export(filePath, fileType, collection, SNMPInfoExportFields, ApplicationName.SNMP);
    }
}
