using System.Collections.Generic;
using NETworkManager.Models.HostsFileEditor;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="HostsFileEntry" />.
    /// </summary>
    private static readonly ExportField<HostsFileEntry>[] HostsFileEntryExportFields =
    [
        new(nameof(HostsFileEntry.IsEnabled), x => x.IsEnabled),
        new(nameof(HostsFileEntry.IPAddress), x => x.IPAddress),
        new(nameof(HostsFileEntry.Hostname), x => x.Hostname, true),
        new(nameof(HostsFileEntry.Comment), x => x.Comment, true)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="HostsFileEntry" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{HostsFileEntry}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<HostsFileEntry> collection)
    {
        Export(filePath, fileType, collection, HostsFileEntryExportFields, ApplicationName.HostsFileEditor);
    }
}
