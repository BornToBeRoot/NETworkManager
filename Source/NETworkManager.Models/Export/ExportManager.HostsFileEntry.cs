﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NETworkManager.Models.HostsFileEditor;
using Newtonsoft.Json;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Method to export objects from type <see cref="HostsFileEntry" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{HostsFileEntry}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<HostsFileEntry> collection)
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
    ///     Creates a CSV file from the given <see cref="HostsFileEntry" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{HostsFileEntry}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateCsv(IEnumerable<HostsFileEntry> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(HostsFileEntry.IsEnabled)},{nameof(HostsFileEntry.IPAddress)},{nameof(HostsFileEntry.Hostname)},{nameof(HostsFileEntry.Comment)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.IsEnabled},{info.IPAddress},{info.Hostname},{info.Comment}");

        File.WriteAllText(filePath, stringBuilder.ToString());
    }

    /// <summary>
    ///     Creates a XML file from the given <see cref="HostsFileEntry" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{HostsFileEntry}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateXml(IEnumerable<HostsFileEntry> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.HostsFileEditor.ToString(),
                new XElement(nameof(HostsFileEntry) + "s",
                    from info in collection
                    select
                        new XElement(nameof(HostsFileEntry),
                            new XElement(nameof(HostsFileEntry.IsEnabled), info.IsEnabled),
                            new XElement(nameof(HostsFileEntry.IPAddress), info.IPAddress),
                            new XElement(nameof(HostsFileEntry.Hostname), info.Hostname),
                            new XElement(nameof(HostsFileEntry.Comment), info.Comment)))));

        document.Save(filePath);
    }

    /// <summary>
    ///     Creates a JSON file from the given <see cref="HostsFileEntry" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{HostsFileEntry}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateJson(IReadOnlyList<HostsFileEntry> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
            jsonData[i] = new
            {
                collection[i].IsEnabled,
                collection[i].IPAddress,
                collection[i].Hostname,
                collection[i].Comment
            };

        File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}
