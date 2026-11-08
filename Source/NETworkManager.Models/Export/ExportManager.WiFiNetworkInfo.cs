using System;
using System.Collections.Generic;
using System.Globalization;
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
    ///     Exported column (CSV) / element (XML, JSON key) of a WiFi network, see <see cref="WiFiNetworkExportFields" />.
    /// </summary>
    /// <param name="Name">Name of the column / element.</param>
    /// <param name="GetValue">Gets the value of a network (e.g. a string, a number, an enum or a time span).</param>
    /// <param name="IsQuoted">The value is quoted in the CSV file (text that may contain a comma, e.g. the SSID).</param>
    private sealed record WiFiNetworkExportField(
        string Name,
        Func<WiFiNetworkInfo, object> GetValue,
        bool IsQuoted = false);

    /// <summary>
    ///     Exported values of a WiFi network - one list for CSV, XML and JSON. The released columns come first, new ones
    ///     are appended at the end, so the position of a column never changes.
    /// </summary>
    private static readonly WiFiNetworkExportField[] WiFiNetworkExportFields =
    [
        new(nameof(WiFiNetworkInfo.Bssid), x => x.Bssid),
        new(nameof(WiFiNetworkInfo.AvailableNetwork.Ssid), x => x.AvailableNetwork.Ssid, true),
        new(nameof(WiFiNetworkInfo.AvailableNetwork.ChannelCenterFrequencyInKilohertz),
            x => x.AvailableNetwork.ChannelCenterFrequencyInKilohertz),
        new(nameof(WiFiNetworkInfo.AvailableNetwork.SignalBars), x => x.AvailableNetwork.SignalBars),
        new(nameof(WiFiNetworkInfo.AvailableNetwork.IsWiFiDirect), x => x.AvailableNetwork.IsWiFiDirect),
        new(nameof(WiFiNetworkInfo.AvailableNetwork.NetworkRssiInDecibelMilliwatts),
            x => x.AvailableNetwork.NetworkRssiInDecibelMilliwatts),
        new(nameof(WiFiNetworkInfo.AvailableNetwork.PhyKind), x => x.AvailableNetwork.PhyKind),
        new(nameof(WiFiNetworkInfo.AvailableNetwork.NetworkKind), x => x.AvailableNetwork.NetworkKind),
        new(nameof(WiFiNetworkInfo.AvailableNetwork.SecuritySettings.NetworkAuthenticationType),
            x => x.AvailableNetwork.SecuritySettings.NetworkAuthenticationType),
        new(nameof(WiFiNetworkInfo.AvailableNetwork.SecuritySettings.NetworkEncryptionType),
            x => x.AvailableNetwork.SecuritySettings.NetworkEncryptionType),
        new(nameof(WiFiNetworkInfo.AvailableNetwork.BeaconInterval), x => x.AvailableNetwork.BeaconInterval),
        new(nameof(WiFiNetworkInfo.AvailableNetwork.Uptime), x => x.AvailableNetwork.Uptime),
        // Values shown in the grid
        new(nameof(WiFiNetworkInfo.IsConnected), x => x.IsConnected),
        // Human-readable authentication type (e.g. "WPA2 Personal (PSK)")
        new("Security", x => x.NetworkAuthenticationType, true),
        new(nameof(WiFiNetworkInfo.Channel), x => x.Channel),
        // MHz, estimated if the width could not be read from the beacon
        new("ChannelWidth", x => x.ChannelBandwidth),
        new("ChannelWidthEstimated", x => x.IsChannelBandwidthEstimated),
        new(nameof(WiFiNetworkInfo.Vendor), x => x.Vendor, true),
        // Change since the previous scan of the same adapter (empty without a previous scan / change)
        new(nameof(WiFiNetworkInfo.Change), x => x.Change == WiFiNetworkChange.None ? null : x.Change.ToString())
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="WiFiNetworkInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{T}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<WiFiNetworkInfo> collection)
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
    ///     Creates a CSV file from the given <see cref="WiFiNetworkInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{WiFiNetworkInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateCsv(IEnumerable<WiFiNetworkInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(string.Join(",", WiFiNetworkExportFields.Select(field => field.Name)));

        foreach (var info in collection)
            stringBuilder.AppendLine(string.Join(",", WiFiNetworkExportFields.Select(field =>
            {
                var value = Convert.ToString(field.GetValue(info), CultureInfo.CurrentCulture);

                return field.IsQuoted ? CsvHelper.QuoteString(value) : value;
            })));

        File.WriteAllText(filePath, stringBuilder.ToString());
    }

    /// <summary>
    ///     Creates a XML file from the given <see cref="WiFiNetworkInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{WiFiNetworkInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateXml(IEnumerable<WiFiNetworkInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.WiFi.ToString(),
                new XElement(nameof(WiFiNetworkInfo) + "s",
                    from info in collection
                    select new XElement(nameof(WiFiNetworkInfo),
                        from field in WiFiNetworkExportFields
                        select new XElement(field.Name, field.GetValue(info))))));

        document.Save(filePath);
    }

    /// <summary>
    ///     Creates a JSON file from the given <see cref="WiFiNetworkInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{WiFiNetworkInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateJson(IReadOnlyList<WiFiNetworkInfo> collection, string filePath)
    {
        var jsonData = collection
            .Select(info => WiFiNetworkExportFields.ToDictionary(field => field.Name, field =>
            {
                var value = field.GetValue(info);

                // Numbers (frequency), text and empty values as they are, other values (enums, time spans, signal bars,
                // RSSI) as text like in the CSV file
                return value is null or int or string ? value : Convert.ToString(value, CultureInfo.CurrentCulture);
            }))
            .ToArray();

        File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}
