using System.Collections.Generic;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="ListenerInfo" />.
    /// </summary>
    private static readonly ExportField<ListenerInfo>[] ListenerInfoExportFields =
    [
        new(nameof(ListenerInfo.Protocol), x => x.Protocol),
        new(nameof(ListenerInfo.IPAddress), x => x.IPAddress),
        new(nameof(ListenerInfo.Port), x => x.Port)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="ListenerInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{ListenerInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<ListenerInfo> collection)
    {
        Export(filePath, fileType, collection, ListenerInfoExportFields, ApplicationName.Listeners);
    }
}
