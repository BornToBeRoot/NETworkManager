using System.Collections.Generic;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="PingInfo" /> (status, time, TTL, bytes like the ping of the IP Scanner). Time
    ///     (ms), TTL and bytes are empty without a reply.
    /// </summary>
    private static readonly ExportField<PingInfo>[] PingInfoExportFields =
    [
        new(nameof(PingInfo.Timestamp), x => x.Timestamp),
        new(nameof(PingInfo.IPAddress), x => x.IPAddress),
        new(nameof(PingInfo.Hostname), x => x.Hostname, true),
        new(nameof(PingInfo.Status), x => x.Status),
        new(nameof(PingInfo.Time), x => HasPingReply(x.Status) ? x.Time : null),
        new(nameof(PingInfo.TTL), x => HasPingReply(x.Status) ? x.TTL : null),
        new(nameof(PingInfo.Bytes), x => HasPingReply(x.Status) ? x.Bytes : null)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="PingInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{PingInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<PingInfo> collection)
    {
        Export(filePath, fileType, collection, PingInfoExportFields, ApplicationName.PingMonitor);
    }
}
