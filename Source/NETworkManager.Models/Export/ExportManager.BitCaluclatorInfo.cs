using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NETworkManager.Models.Network;
using Newtonsoft.Json;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Method to export objects from type <see cref="BitCaluclatorInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{BitCaluclatorInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<BitCaluclatorInfo> collection)
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
    ///     Creates a CSV file from the given <see cref="BitCaluclatorInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{BitCaluclatorInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateCsv(IEnumerable<BitCaluclatorInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(BitCaluclatorInfo.Bits)},{nameof(BitCaluclatorInfo.Bytes)},{nameof(BitCaluclatorInfo.Kilobits)},{nameof(BitCaluclatorInfo.Kilobytes)},{nameof(BitCaluclatorInfo.Megabits)},{nameof(BitCaluclatorInfo.Megabytes)},{nameof(BitCaluclatorInfo.Gigabits)},{nameof(BitCaluclatorInfo.Gigabytes)},{nameof(BitCaluclatorInfo.Terabits)},{nameof(BitCaluclatorInfo.Terabytes)},{nameof(BitCaluclatorInfo.Petabits)},{nameof(BitCaluclatorInfo.Petabytes)}");

        foreach (var info in collection)
            stringBuilder.AppendLine(
                $"{info.Bits},{info.Bytes},{info.Kilobits},{info.Kilobytes},{info.Megabits},{info.Megabytes},{info.Gigabits},{info.Gigabytes},{info.Terabits},{info.Terabytes},{info.Petabits},{info.Petabytes}");

        File.WriteAllText(filePath, stringBuilder.ToString());
    }

    /// <summary>
    ///     Creates a XML file from the given <see cref="BitCaluclatorInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{BitCaluclatorInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateXml(IEnumerable<BitCaluclatorInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.BitCalculator.ToString(),
                new XElement(nameof(BitCaluclatorInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(BitCaluclatorInfo),
                            new XElement(nameof(BitCaluclatorInfo.Bits), info.Bits),
                            new XElement(nameof(BitCaluclatorInfo.Bytes), info.Bytes),
                            new XElement(nameof(BitCaluclatorInfo.Kilobits), info.Kilobits),
                            new XElement(nameof(BitCaluclatorInfo.Kilobytes), info.Kilobytes),
                            new XElement(nameof(BitCaluclatorInfo.Megabits), info.Megabits),
                            new XElement(nameof(BitCaluclatorInfo.Megabytes), info.Megabytes),
                            new XElement(nameof(BitCaluclatorInfo.Gigabits), info.Gigabits),
                            new XElement(nameof(BitCaluclatorInfo.Gigabytes), info.Gigabytes),
                            new XElement(nameof(BitCaluclatorInfo.Terabits), info.Terabits),
                            new XElement(nameof(BitCaluclatorInfo.Terabytes), info.Terabytes),
                            new XElement(nameof(BitCaluclatorInfo.Petabits), info.Petabits),
                            new XElement(nameof(BitCaluclatorInfo.Petabytes), info.Petabytes)))));

        document.Save(filePath);
    }

    /// <summary>
    ///     Creates a JSON file from the given <see cref="BitCaluclatorInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{BitCaluclatorInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateJson(IReadOnlyList<BitCaluclatorInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
            jsonData[i] = new
            {
                collection[i].Bits,
                collection[i].Bytes,
                collection[i].Kilobits,
                collection[i].Kilobytes,
                collection[i].Megabits,
                collection[i].Megabytes,
                collection[i].Gigabits,
                collection[i].Gigabytes,
                collection[i].Terabits,
                collection[i].Terabytes,
                collection[i].Petabits,
                collection[i].Petabytes
            };

        File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}