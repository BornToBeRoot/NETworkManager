using System.Collections.Generic;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="DNSLookupRecordInfo" />.
    /// </summary>
    private static readonly ExportField<DNSLookupRecordInfo>[] DNSLookupRecordInfoExportFields =
    [
        new(nameof(DNSLookupRecordInfo.DomainName), x => x.DomainName, true),
        new(nameof(DNSLookupRecordInfo.TTL), x => x.TTL),
        new(nameof(DNSLookupRecordInfo.RecordClass), x => x.RecordClass),
        new(nameof(DNSLookupRecordInfo.RecordType), x => x.RecordType),
        new(nameof(DNSLookupRecordInfo.Result), x => x.Result, true),
        new(nameof(DNSLookupRecordInfo.NameServerIPAddress), x => x.NameServerIPAddress),
        new(nameof(DNSLookupRecordInfo.NameServerHostName), x => x.NameServerHostName, true),
        new(nameof(DNSLookupRecordInfo.NameServerPort), x => x.NameServerPort)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="DNSLookupRecordInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{DNSLookupRecordInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType,
        IReadOnlyList<DNSLookupRecordInfo> collection)
    {
        Export(filePath, fileType, collection, DNSLookupRecordInfoExportFields, ApplicationName.DNSLookup);
    }
}
