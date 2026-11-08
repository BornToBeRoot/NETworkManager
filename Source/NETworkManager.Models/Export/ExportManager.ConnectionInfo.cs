using System.Collections.Generic;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="ConnectionInfo" />.
    /// </summary>
    private static readonly ExportField<ConnectionInfo>[] ConnectionInfoExportFields =
    [
        new(nameof(ConnectionInfo.Protocol), x => x.Protocol),
        new(nameof(ConnectionInfo.LocalIPAddress), x => x.LocalIPAddress),
        new(nameof(ConnectionInfo.LocalPort), x => x.LocalPort),
        new(nameof(ConnectionInfo.RemoteIPAddress), x => x.RemoteIPAddress),
        new(nameof(ConnectionInfo.RemotePort), x => x.RemotePort),
        new(nameof(ConnectionInfo.RemoteHostname), x => x.RemoteHostname, true),
        new(nameof(ConnectionInfo.TcpState), x => x.TcpState),
        new(nameof(ConnectionInfo.ProcessId), x => x.ProcessId),
        new(nameof(ConnectionInfo.ProcessName), x => x.ProcessName, true),
        new(nameof(ConnectionInfo.ProcessPath), x => x.ProcessPath, true)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="ConnectionInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{ConnectionInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<ConnectionInfo> collection)
    {
        Export(filePath, fileType, collection, ConnectionInfoExportFields, ApplicationName.Connections);
    }
}
