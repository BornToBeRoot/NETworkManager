using System.Collections.Generic;
using NETworkManager.Models.Lookup;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="PortScannerPortInfo" />.
    /// </summary>
    private static readonly ExportField<PortScannerPortInfo>[] PortScannerPortInfoExportFields =
    [
        new(nameof(PortScannerPortInfo.IPAddress), x => x.IPAddress),
        new(nameof(PortScannerPortInfo.Hostname), x => x.Hostname, true),
        new(nameof(PortScannerPortInfo.Port), x => x.Port),
        new(nameof(PortLookupInfo.Protocol), x => x.LookupInfo.Protocol),
        new(nameof(PortLookupInfo.Service), x => x.LookupInfo.Service, true),
        new(nameof(PortLookupInfo.Description), x => x.LookupInfo.Description, true),
        new(nameof(PortScannerPortInfo.State), x => x.State),
        // Change since the previous scan (empty without a previous scan / change)
        new(nameof(PortScannerPortInfo.Change), x => x.Change == PortScannerPortChange.None ? null : x.Change)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="PortScannerPortInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{PortScannerPortInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType,
        IReadOnlyList<PortScannerPortInfo> collection)
    {
        Export(filePath, fileType, collection, PortScannerPortInfoExportFields, ApplicationName.PortScanner);
    }
}
