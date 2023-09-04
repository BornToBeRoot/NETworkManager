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
    /// Method to export objects from type <see cref="TracerouteHopInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{TracerouteHopInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType,
        IReadOnlyList<TracerouteHopInfo> collection)
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

    private static void CreateCsv(IEnumerable<TracerouteHopInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(TracerouteHopInfo.Hop)},{nameof(TracerouteHopInfo.Status1)},{nameof(TracerouteHopInfo.Time1)},{nameof(TracerouteHopInfo.Status2)},{nameof(TracerouteHopInfo.Time2)},{nameof(TracerouteHopInfo.Status3)}{nameof(TracerouteHopInfo.Time3)},{nameof(TracerouteHopInfo.IPAddress)},{nameof(TracerouteHopInfo.Hostname)},{nameof(TracerouteHopInfo.IPGeolocationResult.Info.Continent)},{nameof(TracerouteHopInfo.IPGeolocationResult.Info.Country)},{nameof(TracerouteHopInfo.IPGeolocationResult.Info.Region)},{nameof(TracerouteHopInfo.IPGeolocationResult.Info.City)},{nameof(TracerouteHopInfo.IPGeolocationResult.Info.District)},{nameof(TracerouteHopInfo.IPGeolocationResult.Info.Isp)},{nameof(TracerouteHopInfo.IPGeolocationResult.Info.Org)}, {nameof(TracerouteHopInfo.IPGeolocationResult.Info.As)},{nameof(TracerouteHopInfo.IPGeolocationResult.Info.Asname)},{nameof(TracerouteHopInfo.IPGeolocationResult.Info.Hosting)},{nameof(TracerouteHopInfo.IPGeolocationResult.Info.Proxy)},{nameof(TracerouteHopInfo.IPGeolocationResult.Info.Mobile)}");

        foreach (var info in collection)
            stringBuilder.AppendLine(
                $"{info.Hop},{info.Status1},{Ping.TimeToString(info.Status1, info.Time1, true)},{info.Status2},{Ping.TimeToString(info.Status2, info.Time2, true)},{info.Status3},{Ping.TimeToString(info.Status3, info.Time3, true)},{info.IPAddress},{info.Hostname},{info.IPGeolocationResult.Info.Continent},{info.IPGeolocationResult.Info.Country},{info.IPGeolocationResult.Info.Region},{info.IPGeolocationResult.Info.City},{info.IPGeolocationResult.Info.District},{info.IPGeolocationResult.Info.Isp?.Replace(",", "")},{info.IPGeolocationResult.Info.Org?.Replace(",", "")},{info.IPGeolocationResult.Info.As?.Replace(",", "")},{info.IPGeolocationResult.Info.Asname?.Replace(",", "")},{info.IPGeolocationResult.Info.Hosting},{info.IPGeolocationResult.Info.Proxy},{info.IPGeolocationResult.Info.Mobile}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateXml(IEnumerable<TracerouteHopInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.Traceroute.ToString(),
                new XElement(nameof(TracerouteHopInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(TracerouteHopInfo),
                            new XElement(nameof(TracerouteHopInfo.Hop), info.Hop),
                            new XElement(nameof(TracerouteHopInfo.Status1), info.Status1),
                            new XElement(nameof(TracerouteHopInfo.Time1),
                                Ping.TimeToString(info.Status1, info.Time1, true)),
                            new XElement(nameof(TracerouteHopInfo.Status2), info.Status2),
                            new XElement(nameof(TracerouteHopInfo.Time2),
                                Ping.TimeToString(info.Status2, info.Time2, true)),
                            new XElement(nameof(TracerouteHopInfo.Status3), info.Status3),
                            new XElement(nameof(TracerouteHopInfo.Time3),
                                Ping.TimeToString(info.Status3, info.Time3, true)),
                            new XElement(nameof(TracerouteHopInfo.IPAddress), info.IPAddress),
                            new XElement(nameof(TracerouteHopInfo.Hostname), info.Hostname),
                            new XElement(nameof(TracerouteHopInfo.IPGeolocationResult.Info.Continent), info.IPGeolocationResult.Info.Continent),
                            new XElement(nameof(TracerouteHopInfo.IPGeolocationResult.Info.Country), info.IPGeolocationResult.Info.Country),
                            new XElement(nameof(TracerouteHopInfo.IPGeolocationResult.Info.Region), info.IPGeolocationResult.Info.Region),
                            new XElement(nameof(TracerouteHopInfo.IPGeolocationResult.Info.City), info.IPGeolocationResult.Info.City),
                            new XElement(nameof(TracerouteHopInfo.IPGeolocationResult.Info.District), info.IPGeolocationResult.Info.District),
                            new XElement(nameof(TracerouteHopInfo.IPGeolocationResult.Info.Isp), info.IPGeolocationResult.Info.Isp),
                            new XElement(nameof(TracerouteHopInfo.IPGeolocationResult.Info.Org), info.IPGeolocationResult.Info.Org),
                            new XElement(nameof(TracerouteHopInfo.IPGeolocationResult.Info.As), info.IPGeolocationResult.Info.As),
                            new XElement(nameof(TracerouteHopInfo.IPGeolocationResult.Info.Asname), info.IPGeolocationResult.Info.Asname),
                            new XElement(nameof(TracerouteHopInfo.IPGeolocationResult.Info.Hosting), info.IPGeolocationResult.Info.Hosting),
                            new XElement(nameof(TracerouteHopInfo.IPGeolocationResult.Info.Proxy), info.IPGeolocationResult.Info.Proxy),
                            new XElement(nameof(TracerouteHopInfo.IPGeolocationResult.Info.Mobile), info.IPGeolocationResult.Info.Mobile)))));

        document.Save(filePath);
    }

    private static void CreateJson(IReadOnlyList<TracerouteHopInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];
        
        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                collection[i].Hop,
                Status1 = collection[i].Status1.ToString(),
                Time1 = Ping.TimeToString(collection[i].Status1, collection[i].Time1, true),
                Status2 = collection[i].Status2.ToString(),
                Time2 = Ping.TimeToString(collection[i].Status2, collection[i].Time2, true),
                Status3 = collection[i].Status3.ToString(),
                Time3 = Ping.TimeToString(collection[i].Status3, collection[i].Time3, true),
                IPAddress = collection[i].IPAddress?.ToString(),
                collection[i].Hostname,
                collection[i].IPGeolocationResult.Info.Continent,
                collection[i].IPGeolocationResult.Info.Country,
                collection[i].IPGeolocationResult.Info.Region,
                collection[i].IPGeolocationResult.Info.City,
                collection[i].IPGeolocationResult.Info.District,
                collection[i].IPGeolocationResult.Info.Isp,
                collection[i].IPGeolocationResult.Info.Org,
                collection[i].IPGeolocationResult.Info.As,
                collection[i].IPGeolocationResult.Info.Asname,
                collection[i].IPGeolocationResult.Info.Hosting,
                collection[i].IPGeolocationResult.Info.Proxy,
                collection[i].IPGeolocationResult.Info.Mobile
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}