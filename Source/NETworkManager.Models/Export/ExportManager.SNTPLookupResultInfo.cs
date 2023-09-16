using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using Newtonsoft.Json;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    /// Method to export objects from type <see cref="SNTPLookupInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{SNTPLookupInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType,
        IReadOnlyList<SNTPLookupInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.Csv:
                CreateCsv(collection, filePath);
                break;
            case ExportFileType.Xml:
                CreateXml(collection, filePath);
                break;
            case ExportFileType.Json:
                CreateJson(collection, filePath);
                break;
            case ExportFileType.Txt:
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    /// Creates a CSV file from the given <see cref="SNTPLookupInfo"/> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{SNTPLookupInfo}"/> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateCsv(IEnumerable<SNTPLookupInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(SNTPLookupInfo.Server)},{nameof(SNTPLookupInfo.IPEndPoint)},{nameof(SNTPLookupInfo.DateTime.NetworkTime)},{nameof(SNTPLookupInfo.DateTime.LocalStartTime)},{nameof(SNTPLookupInfo.DateTime.LocalEndTime)},{nameof(SNTPLookupInfo.DateTime.Offset)},{nameof(SNTPLookupInfo.DateTime.RoundTripDelay)}");

        foreach (var info in collection)
            stringBuilder.AppendLine(
                $"{info.Server},{info.IPEndPoint},{DateTimeHelper.DateTimeToFullDateTimeString(info.DateTime.NetworkTime)},{DateTimeHelper.DateTimeToFullDateTimeString(info.DateTime.LocalStartTime)},{DateTimeHelper.DateTimeToFullDateTimeString(info.DateTime.LocalEndTime)},{info.DateTime.Offset} s,{info.DateTime.RoundTripDelay} ms");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    /// <summary>
    /// Creates a XML file from the given <see cref="SNTPLookupInfo"/> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{SNTPLookupInfo}"/> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateXml(IEnumerable<SNTPLookupInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.SNMP.ToString(),
                new XElement(nameof(SNTPLookupInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(SNTPLookupInfo),
                            new XElement(nameof(SNTPLookupInfo.Server), info.Server),
                            new XElement(nameof(SNTPLookupInfo.IPEndPoint), info.IPEndPoint),
                            new XElement(nameof(SNTPLookupInfo.DateTime.NetworkTime),
                                DateTimeHelper.DateTimeToFullDateTimeString(info.DateTime.NetworkTime)),
                            new XElement(nameof(SNTPLookupInfo.DateTime.LocalStartTime),
                                DateTimeHelper.DateTimeToFullDateTimeString(info.DateTime.LocalStartTime)),
                            new XElement(nameof(SNTPLookupInfo.DateTime.LocalEndTime),
                                DateTimeHelper.DateTimeToFullDateTimeString(info.DateTime.LocalEndTime)),
                            new XElement(nameof(SNTPLookupInfo.DateTime.Offset), $"{info.DateTime.Offset} s"),
                            new XElement(nameof(SNTPLookupInfo.DateTime.RoundTripDelay),
                                $"{info.DateTime.RoundTripDelay} ms")))));

        document.Save(filePath);
    }

    /// <summary>
    /// Creates a JSON file from the given <see cref="SNTPLookupInfo"/> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{SNTPLookupInfo}"/> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateJson(IReadOnlyList<SNTPLookupInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                collection[i].Server,
                collection[i].IPEndPoint,
                NetworkTime = DateTimeHelper.DateTimeToFullDateTimeString(collection[i].DateTime.NetworkTime),
                LocalStartTime = DateTimeHelper.DateTimeToFullDateTimeString(collection[i].DateTime.LocalStartTime),
                LocalEndTime = DateTimeHelper.DateTimeToFullDateTimeString(collection[i].DateTime.LocalEndTime),
                Offset = $"{collection[i].DateTime.Offset} s",
                RoundTripDelay = $"{collection[i].DateTime.RoundTripDelay} ms",
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}