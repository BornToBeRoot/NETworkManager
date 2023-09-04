using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NETworkManager.Models.Network;
using Newtonsoft.Json;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    /// Method to export objects from type <see cref="DNSLookupRecordInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{DNSLookupRecordInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType,
        IReadOnlyList<DNSLookupRecordInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.CSV:
                CreateCsv(collection, filePath);
                break;
            case ExportFileType.XML:
                CreateXml(collection, filePath);
                break;
            case ExportFileType.JSON:
                CreateJson(collection, filePath);
                break;
            case ExportFileType.TXT:
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    private static void CreateCsv(IEnumerable<DNSLookupRecordInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(DNSLookupRecordInfo.DomainName)},{nameof(DNSLookupRecordInfo.TTL)},{nameof(DNSLookupRecordInfo.Class)},{nameof(DNSLookupRecordInfo.Type)},{nameof(DNSLookupRecordInfo.Result)},{nameof(DNSLookupRecordInfo.Server)},{nameof(DNSLookupRecordInfo.IPEndPoint)}");

        foreach (var info in collection)
            stringBuilder.AppendLine(
                $"{info.DomainName},{info.TTL},{info.Class},{info.Type},{info.Result},{info.Server},{info.IPEndPoint}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateXml(IEnumerable<DNSLookupRecordInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.DNSLookup.ToString(),
                new XElement(nameof(DNSLookupRecordInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(DNSLookupRecordInfo),
                            new XElement(nameof(DNSLookupRecordInfo.DomainName), info.DomainName),
                            new XElement(nameof(DNSLookupRecordInfo.TTL), info.TTL),
                            new XElement(nameof(DNSLookupRecordInfo.Class), info.Class),
                            new XElement(nameof(DNSLookupRecordInfo.Type), info.Type),
                            new XElement(nameof(DNSLookupRecordInfo.Result), info.Result),
                            new XElement(nameof(DNSLookupRecordInfo.Server), info.Server),
                            new XElement(nameof(DNSLookupRecordInfo.IPEndPoint), info.IPEndPoint)))));

        document.Save(filePath);
    }

    private static void CreateJson(IReadOnlyList<DNSLookupRecordInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                collection[i].DomainName,
                collection[i].TTL,
                collection[i].Class,
                collection[i].Type,
                collection[i].Result,
                collection[i].Server,
                collection[i].IPEndPoint
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}