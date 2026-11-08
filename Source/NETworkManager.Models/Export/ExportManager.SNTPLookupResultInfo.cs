using System.Collections.Generic;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="SNTPLookupInfo" />.
    /// </summary>
    private static readonly ExportField<SNTPLookupInfo>[] SNTPLookupInfoExportFields =
    [
        new(nameof(SNTPLookupInfo.Server), x => x.Server, true),
        new(nameof(SNTPLookupInfo.IPEndPoint), x => x.IPEndPoint),
        new(nameof(SNTPLookupInfo.DateTime.NetworkTime), x => x.DateTime.NetworkTime),
        new(nameof(SNTPLookupInfo.DateTime.LocalStartTime), x => x.DateTime.LocalStartTime),
        new(nameof(SNTPLookupInfo.DateTime.LocalEndTime), x => x.DateTime.LocalEndTime),
        // Seconds
        new(nameof(SNTPLookupInfo.DateTime.Offset), x => x.DateTime.Offset),
        // ms
        new(nameof(SNTPLookupInfo.DateTime.RoundTripDelay), x => x.DateTime.RoundTripDelay)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="SNTPLookupInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{SNTPLookupInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<SNTPLookupInfo> collection)
    {
        Export(filePath, fileType, collection, SNTPLookupInfoExportFields, ApplicationName.SNTPLookup);
    }
}
