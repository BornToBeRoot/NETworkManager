using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NETworkManager.Models.Firewall;
using NETworkManager.Utilities;
using Newtonsoft.Json;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Method to export objects from type <see cref="FirewallRule" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{FirewallRule}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<FirewallRule> collection)
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
    ///     Creates a CSV file from the given <see cref="FirewallRule" /> collection.
    /// </summary>
    private static void CreateCsv(IEnumerable<FirewallRule> collection, string filePath)
    {
        var sb = new StringBuilder();

        sb.AppendLine(
            $"{nameof(FirewallRule.Name)}," +
            $"{nameof(FirewallRule.IsEnabled)}," +
            $"{nameof(FirewallRule.Direction)}," +
            $"{nameof(FirewallRule.Action)}," +
            $"{nameof(FirewallRule.Protocol)}," +
            $"{nameof(FirewallRule.LocalPorts)}," +
            $"{nameof(FirewallRule.RemotePorts)}," +
            $"{nameof(FirewallRule.LocalAddresses)}," +
            $"{nameof(FirewallRule.RemoteAddresses)}," +
            $"{nameof(FirewallRule.NetworkProfiles)}," +
            $"{nameof(FirewallRule.InterfaceType)}," +
            $"{nameof(FirewallRule.Program)}," +
            $"{nameof(FirewallRule.Description)}");

        foreach (var rule in collection)
            sb.AppendLine(
                $"{CsvHelper.QuoteString(rule.Name)}," +
                $"{rule.IsEnabled}," +
                $"{rule.Direction}," +
                $"{rule.Action}," +
                $"{rule.Protocol}," +
                $"{CsvHelper.QuoteString(rule.LocalPortsDisplay)}," +
                $"{CsvHelper.QuoteString(rule.RemotePortsDisplay)}," +
                $"{CsvHelper.QuoteString(rule.LocalAddressesDisplay)}," +
                $"{CsvHelper.QuoteString(rule.RemoteAddressesDisplay)}," +
                $"{CsvHelper.QuoteString(rule.NetworkProfilesDisplay)}," +
                $"{rule.InterfaceType}," +
                $"{CsvHelper.QuoteString(rule.ProgramDisplay)}," +
                $"{CsvHelper.QuoteString(rule.Description)}");

        File.WriteAllText(filePath, sb.ToString());
    }

    /// <summary>
    ///     Creates an XML file from the given <see cref="FirewallRule" /> collection.
    /// </summary>
    private static void CreateXml(IEnumerable<FirewallRule> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(nameof(ApplicationName.Firewall),
                new XElement(nameof(FirewallRule) + "s",
                    from rule in collection
                    select new XElement(nameof(FirewallRule),
                        new XElement(nameof(FirewallRule.Name), rule.Name),
                        new XElement(nameof(FirewallRule.IsEnabled), rule.IsEnabled),
                        new XElement(nameof(FirewallRule.Direction), rule.Direction),
                        new XElement(nameof(FirewallRule.Action), rule.Action),
                        new XElement(nameof(FirewallRule.Protocol), rule.Protocol),
                        new XElement(nameof(FirewallRule.LocalPorts), rule.LocalPortsDisplay),
                        new XElement(nameof(FirewallRule.RemotePorts), rule.RemotePortsDisplay),
                        new XElement(nameof(FirewallRule.LocalAddresses), rule.LocalAddressesDisplay),
                        new XElement(nameof(FirewallRule.RemoteAddresses), rule.RemoteAddressesDisplay),
                        new XElement(nameof(FirewallRule.NetworkProfiles), rule.NetworkProfilesDisplay),
                        new XElement(nameof(FirewallRule.InterfaceType), rule.InterfaceType),
                        new XElement(nameof(FirewallRule.Program), rule.ProgramDisplay),
                        new XElement(nameof(FirewallRule.Description), rule.Description)))));

        document.Save(filePath);
    }

    /// <summary>
    ///     Creates a JSON file from the given <see cref="FirewallRule" /> collection.
    /// </summary>
    private static void CreateJson(IReadOnlyList<FirewallRule> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            var rule = collection[i];
            jsonData[i] = new
            {
                rule.Name,
                rule.IsEnabled,
                Direction = rule.Direction.ToString(),
                Action = rule.Action.ToString(),
                Protocol = rule.Protocol.ToString(),
                LocalPorts = rule.LocalPortsDisplay,
                RemotePorts = rule.RemotePortsDisplay,
                LocalAddresses = rule.LocalAddressesDisplay,
                RemoteAddresses = rule.RemoteAddressesDisplay,
                NetworkProfiles = rule.NetworkProfilesDisplay,
                InterfaceType = rule.InterfaceType.ToString(),
                Program = rule.ProgramDisplay,
                rule.Description
            };
        }

        File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}