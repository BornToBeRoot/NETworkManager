using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NETworkManager.Models.IPApi;
using Newtonsoft.Json;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Method to export objects from type <see cref="IPGeolocationInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{IPGeolocationInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<IPGeolocationInfo> collection)
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
    ///     Creates a CSV file from the given <see cref="IPGeolocationInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{IPGeolocationInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateCsv(IEnumerable<IPGeolocationInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(IPGeolocationInfo.Status)},{nameof(IPGeolocationInfo.Continent)},{nameof(IPGeolocationInfo.ContinentCode)},{nameof(IPGeolocationInfo.Country)},{nameof(IPGeolocationInfo.CountryCode)},{nameof(IPGeolocationInfo.Region)},{nameof(IPGeolocationInfo.RegionName)},{nameof(IPGeolocationInfo.City)},{nameof(IPGeolocationInfo.District)},{nameof(IPGeolocationInfo.Zip)},{nameof(IPGeolocationInfo.Lat)},{nameof(IPGeolocationInfo.Lon)},{nameof(IPGeolocationInfo.Timezone)},{nameof(IPGeolocationInfo.Offset)},{nameof(IPGeolocationInfo.Currency)},{nameof(IPGeolocationInfo.Isp)},{nameof(IPGeolocationInfo.Org)},{nameof(IPGeolocationInfo.As)},{nameof(IPGeolocationInfo.Asname)},{nameof(IPGeolocationInfo.Reverse)},{nameof(IPGeolocationInfo.Mobile)},{nameof(IPGeolocationInfo.Proxy)},{nameof(IPGeolocationInfo.Hosting)},{nameof(IPGeolocationInfo.Query)}");

        foreach (var info in collection)
            stringBuilder.AppendLine(
                $"{info.Status},{info.Continent},{info.ContinentCode},{info.Country},{info.CountryCode},{info.Region},{info.RegionName},{info.City},{info.District},{info.Zip},{info.Lat},{info.Lon},{info.Timezone},{info.Offset},{info.Currency},{info.Isp},{info.Org},{info.As},{info.Asname},{info.Reverse},{info.Mobile},{info.Proxy},{info.Hosting},{info.Query}");

        File.WriteAllText(filePath, stringBuilder.ToString());
    }

    /// <summary>
    ///     Creates a XML file from the given <see cref="IPGeolocationInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{IPGeolocationInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateXml(IEnumerable<IPGeolocationInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.IPGeolocation.ToString(),
                new XElement(nameof(IPGeolocationInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(IPGeolocationInfo),
                            new XElement(nameof(IPGeolocationInfo.Status), info.Status),
                            new XElement(nameof(IPGeolocationInfo.Continent), info.Continent),
                            new XElement(nameof(IPGeolocationInfo.ContinentCode), info.ContinentCode),
                            new XElement(nameof(IPGeolocationInfo.Country), info.Country),
                            new XElement(nameof(IPGeolocationInfo.CountryCode), info.CountryCode),
                            new XElement(nameof(IPGeolocationInfo.Region), info.Region),
                            new XElement(nameof(IPGeolocationInfo.RegionName), info.RegionName),
                            new XElement(nameof(IPGeolocationInfo.City), info.City),
                            new XElement(nameof(IPGeolocationInfo.District), info.District),
                            new XElement(nameof(IPGeolocationInfo.Zip), info.Zip),
                            new XElement(nameof(IPGeolocationInfo.Lat), info.Lat),
                            new XElement(nameof(IPGeolocationInfo.Lon), info.Lon),
                            new XElement(nameof(IPGeolocationInfo.Timezone), info.Timezone),
                            new XElement(nameof(IPGeolocationInfo.Offset), info.Offset),
                            new XElement(nameof(IPGeolocationInfo.Currency), info.Currency),
                            new XElement(nameof(IPGeolocationInfo.Isp), info.Isp),
                            new XElement(nameof(IPGeolocationInfo.Org), info.Org),
                            new XElement(nameof(IPGeolocationInfo.As), info.As),
                            new XElement(nameof(IPGeolocationInfo.Asname), info.Asname),
                            new XElement(nameof(IPGeolocationInfo.Reverse), info.Reverse),
                            new XElement(nameof(IPGeolocationInfo.Mobile), info.Mobile),
                            new XElement(nameof(IPGeolocationInfo.Proxy), info.Proxy),
                            new XElement(nameof(IPGeolocationInfo.Hosting), info.Hosting),
                            new XElement(nameof(IPGeolocationInfo.Query), info.Query)))));

        document.Save(filePath);
    }

    /// <summary>
    ///     Creates a JSON file from the given <see cref="IPGeolocationInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{IPGeolocationInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateJson(IReadOnlyList<IPGeolocationInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
            jsonData[i] = new
            {
                collection[i].Status,
                collection[i].Continent,
                collection[i].ContinentCode,
                collection[i].Country,
                collection[i].CountryCode,
                collection[i].Region,
                collection[i].RegionName,
                collection[i].City,
                collection[i].District,
                collection[i].Zip,
                collection[i].Lat,
                collection[i].Lon,
                collection[i].Timezone,
                collection[i].Offset,
                collection[i].Currency,
                collection[i].Isp,
                collection[i].Org,
                collection[i].As,
                collection[i].Asname,
                collection[i].Reverse,
                collection[i].Mobile,
                collection[i].Proxy,
                collection[i].Hosting,
                collection[i].Query
            };

        File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}