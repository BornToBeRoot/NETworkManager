using System.Collections.Generic;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    private static readonly ExportField<MDNSServiceInfo>[] MDNSServiceInfoExportFields =
    [
        new(nameof(MDNSServiceInfo.Name), x => x.Name, true),
        new(nameof(MDNSServiceInfo.FullName), x => x.FullName, true),
        new(nameof(MDNSServiceInfo.ServiceType), x => x.ServiceType),
        new(nameof(MDNSServiceInfo.NetworkInterface), x => x.NetworkInterface, true),
        new(nameof(MDNSServiceInfo.Hostname), x => x.Hostname, true),
        new(nameof(MDNSServiceInfo.Port), x => x.Port),
        new(nameof(MDNSServiceInfo.Priority), x => x.Priority),
        new(nameof(MDNSServiceInfo.Weight), x => x.Weight),
        new(nameof(MDNSServiceInfo.Addresses), x => x.Addresses),
        new(nameof(MDNSServiceInfo.TXTRecords), x => x.TXTRecords, true),
        new(nameof(MDNSServiceInfo.TTL), x => x.TTL)
    ];

    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<MDNSServiceInfo> collection) =>
        Export(filePath, fileType, collection, MDNSServiceInfoExportFields, ApplicationName.MDNSDiscovery);
}
