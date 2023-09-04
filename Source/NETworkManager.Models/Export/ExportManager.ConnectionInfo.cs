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
    /// Method to export objects from type <see cref="ConnectionInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{ConnectionInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<ConnectionInfo> collection)
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

    private static void CreateCsv(IEnumerable<ConnectionInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(ConnectionInfo.Protocol)},{nameof(ConnectionInfo.LocalIPAddress)},{nameof(ConnectionInfo.LocalPort)},{nameof(ConnectionInfo.RemoteIPAddress)},{nameof(ConnectionInfo.RemotePort)},{nameof(ConnectionInfo.TcpState)}");

        foreach (var info in collection)
            stringBuilder.AppendLine(
                $"{info.Protocol},{info.LocalIPAddress},{info.LocalPort},{info.RemoteIPAddress},{info.RemotePort},{info.TcpState}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateXml(IEnumerable<ConnectionInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.Connections.ToString(),
                new XElement(nameof(ConnectionInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(ConnectionInfo),
                            new XElement(nameof(ConnectionInfo.Protocol), info.Protocol),
                            new XElement(nameof(ConnectionInfo.LocalIPAddress), info.LocalIPAddress),
                            new XElement(nameof(ConnectionInfo.LocalPort), info.LocalPort),
                            new XElement(nameof(ConnectionInfo.RemoteIPAddress), info.RemoteIPAddress),
                            new XElement(nameof(ConnectionInfo.RemotePort), info.RemotePort),
                            new XElement(nameof(ConnectionInfo.TcpState), info.TcpState)))));

        document.Save(filePath);
    }

    private static void CreateJson(IReadOnlyList<ConnectionInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                Protocol = collection[i].Protocol.ToString(),
                LocalIPAddress = collection[i].LocalIPAddress.ToString(),
                collection[i].LocalPort,
                RemoteIPAddress = collection[i].RemoteIPAddress.ToString(),
                collection[i].RemotePort,
                TcpState = collection[i].TcpState.ToString()
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}