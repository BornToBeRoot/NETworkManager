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
    /// Method to export objects from type <see cref="SNTPLookupResultInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{SNTPLookupResultInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType,
        IReadOnlyList<SNTPLookupResultInfo> collection)
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

    private static void CreateCsv(IEnumerable<SNTPLookupResultInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(SNTPLookupResultInfo.Server)},{nameof(SNTPLookupResultInfo.IPEndPoint)},{nameof(SNTPLookupResultInfo.DateTime.NetworkTime)},{nameof(SNTPLookupResultInfo.DateTime.LocalStartTime)},{nameof(SNTPLookupResultInfo.DateTime.LocalEndTime)},{nameof(SNTPLookupResultInfo.DateTime.Offset)},{nameof(SNTPLookupResultInfo.DateTime.RoundTripDelay)}");

        foreach (var info in collection)
            stringBuilder.AppendLine(
                $"{info.Server},{info.IPEndPoint},{info.DateTime.NetworkTime.ToString("yyyy.MM.dd HH:mm:ss.fff")},{info.DateTime.LocalStartTime.ToString("yyyy.MM.dd HH:mm:ss.fff")},{info.DateTime.LocalEndTime.ToString("yyyy.MM.dd HH:mm:ss.fff")},{info.DateTime.Offset} s,{info.DateTime.RoundTripDelay} ms");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateXml(IEnumerable<SNTPLookupResultInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.SNMP.ToString(),
                new XElement(nameof(SNTPLookupResultInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(SNTPLookupResultInfo),
                            new XElement(nameof(SNTPLookupResultInfo.Server), info.Server),
                            new XElement(nameof(SNTPLookupResultInfo.IPEndPoint), info.IPEndPoint),
                            new XElement(nameof(SNTPLookupResultInfo.DateTime.NetworkTime),
                                info.DateTime.NetworkTime.ToString("yyyy.MM.dd HH:mm:ss.fff")),
                            new XElement(nameof(SNTPLookupResultInfo.DateTime.LocalStartTime),
                                info.DateTime.LocalStartTime.ToString("yyyy.MM.dd HH:mm:ss.fff")),
                            new XElement(nameof(SNTPLookupResultInfo.DateTime.LocalEndTime),
                                info.DateTime.LocalEndTime.ToString("yyyy.MM.dd HH:mm:ss.fff")),
                            new XElement(nameof(SNTPLookupResultInfo.DateTime.Offset), $"{info.DateTime.Offset} s"),
                            new XElement(nameof(SNTPLookupResultInfo.DateTime.RoundTripDelay),
                                $"{info.DateTime.RoundTripDelay} ms")))));

        document.Save(filePath);
    }

    private static void CreateJson(IReadOnlyList<SNTPLookupResultInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                collection[i].Server,
                collection[i].IPEndPoint,
                NetworkTime = collection[i].DateTime.NetworkTime.ToString("yyyy.MM.dd HH:mm:ss.fff"),
                LocalStartTime = collection[i].DateTime.LocalStartTime.ToString("yyyy.MM.dd HH:mm:ss.fff"),
                LocalEndTime = collection[i].DateTime.LocalEndTime.ToString("yyyy.MM.dd HH:mm:ss.fff"),
                Offset = $"{collection[i].DateTime.Offset} s",
                RoundTripDelay = $"{collection[i].DateTime.RoundTripDelay} ms",
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}