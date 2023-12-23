using System;
using System.Collections.Generic;
using System.IO;
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
    ///     Method to export objects from type <see cref="PingInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{PingInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<PingInfo> collection)
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
    ///     Creates a CSV file from the given <see cref="PingInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{PingInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateCsv(IEnumerable<PingInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(PingInfo.Timestamp)},{nameof(PingInfo.IPAddress)},{nameof(PingInfo.Hostname)},{nameof(PingInfo.Bytes)},{nameof(PingInfo.Time)},{nameof(PingInfo.TTL)},{nameof(PingInfo.Status)}");

        foreach (var info in collection)
            stringBuilder.AppendLine(
                $"{DateTimeHelper.DateTimeToFullDateTimeString(info.Timestamp)},{info.IPAddress},{info.Hostname},{info.Bytes},{Ping.TimeToString(info.Status, info.Time, true)},{info.TTL},{info.Status}");

        File.WriteAllText(filePath, stringBuilder.ToString());
    }

    /// <summary>
    ///     Creates a XML file from the given <see cref="PingInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{PingInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateXml(IEnumerable<PingInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.PingMonitor.ToString(),
                new XElement(nameof(PingInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(PingInfo),
                            new XElement(nameof(PingInfo.Timestamp),
                                DateTimeHelper.DateTimeToFullDateTimeString(info.Timestamp)),
                            new XElement(nameof(PingInfo.IPAddress), info.IPAddress),
                            new XElement(nameof(PingInfo.Hostname), info.Hostname),
                            new XElement(nameof(PingInfo.Bytes), info.Bytes),
                            new XElement(nameof(PingInfo.Time), Ping.TimeToString(info.Status, info.Time, true)),
                            new XElement(nameof(PingInfo.TTL), info.TTL),
                            new XElement(nameof(PingInfo.Status), info.Status)))));

        document.Save(filePath);
    }

    /// <summary>
    ///     Creates a JSON file from the given <see cref="PingInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{PingInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateJson(IReadOnlyList<PingInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
            jsonData[i] = new
            {
                Timestamp = DateTimeHelper.DateTimeToFullDateTimeString(collection[i].Timestamp),
                IPAddress = collection[i].IPAddress.ToString(),
                collection[i].Hostname,
                collection[i].Bytes,
                Time = Ping.TimeToString(collection[i].Status, collection[i].Time, true),
                collection[i].TTL,
                Status = collection[i].Status.ToString()
            };

        File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}