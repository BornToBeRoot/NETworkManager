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
    /// Method to export objects from type <see cref="IPNetworkInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{IPNetworkInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<IPNetworkInfo> collection)
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

    private static void CreateCsv(IEnumerable<IPNetworkInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(IPNetworkInfo.Network)},{nameof(IPNetworkInfo.Broadcast)},{nameof(IPNetworkInfo.Total)},{nameof(IPNetworkInfo.Netmask)},{nameof(IPNetworkInfo.Cidr)},{nameof(IPNetworkInfo.FirstUsable)},{nameof(IPNetworkInfo.LastUsable)},{nameof(IPNetworkInfo.Usable)}");

        foreach (var info in collection)
            stringBuilder.AppendLine(
                $"{info.Network},{info.Broadcast},{info.Total},{info.Netmask},{info.Cidr},{info.FirstUsable},{info.LastUsable},{info.Usable}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateXml(IEnumerable<IPNetworkInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.SNMP.ToString(),
                new XElement(nameof(IPNetworkInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(IPNetworkInfo),
                            new XElement(nameof(IPNetworkInfo.Network), info.Network),
                            new XElement(nameof(IPNetworkInfo.Broadcast), info.Broadcast),
                            new XElement(nameof(IPNetworkInfo.Total), info.Total),
                            new XElement(nameof(IPNetworkInfo.Netmask), info.Netmask),
                            new XElement(nameof(IPNetworkInfo.Cidr), info.Cidr),
                            new XElement(nameof(IPNetworkInfo.FirstUsable), info.FirstUsable),
                            new XElement(nameof(IPNetworkInfo.LastUsable), info.LastUsable),
                            new XElement(nameof(IPNetworkInfo.Usable), info.Usable)))));

        document.Save(filePath);
    }

    private static void CreateJson(IReadOnlyList<IPNetworkInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                Network = collection[i].Network.ToString(),
                Broadcast = collection[i].Broadcast.ToString(),
                collection[i].Total,
                Netmask = collection[i].Netmask.ToString(),
                collection[i].Cidr,
                FirstUsable = collection[i].FirstUsable.ToString(),
                LastUsable = collection[i].LastUsable.ToString(),
                collection[i].Usable
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}