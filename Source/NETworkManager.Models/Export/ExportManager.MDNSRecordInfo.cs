using System.Collections.Generic;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    private static readonly ExportField<MDNSRecordInfo>[] MDNSRecordInfoExportFields =
    [
        new(nameof(MDNSRecordInfo.Name), x => x.Name, true),
        new(nameof(MDNSRecordInfo.Type), x => x.Type),
        new(nameof(MDNSRecordInfo.Class), x => x.Class),
        new(nameof(MDNSRecordInfo.TTL), x => x.TTL),
        new(nameof(MDNSRecordInfo.Value), x => x.Value, true),
        new(nameof(MDNSRecordInfo.CacheFlush), x => x.CacheFlush),
        new(nameof(MDNSRecordInfo.NetworkInterface), x => x.NetworkInterface, true)
    ];

    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<MDNSRecordInfo> collection) =>
        Export(filePath, fileType, collection, MDNSRecordInfoExportFields, ApplicationName.MDNSDiscovery);
}
