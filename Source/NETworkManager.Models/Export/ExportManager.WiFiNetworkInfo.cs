using System.Collections.Generic;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="WiFiNetworkInfo" />. The released columns come first, new ones are appended at
    ///     the end, so the position of a column never changes.
    /// </summary>
    private static readonly ExportField<WiFiNetworkInfo>[] WiFiNetworkInfoExportFields =
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
        new(nameof(WiFiNetworkInfo.Change), x => x.Change == WiFiNetworkChange.None ? null : x.Change)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="WiFiNetworkInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{T}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<WiFiNetworkInfo> collection)
    {
        Export(filePath, fileType, collection, WiFiNetworkInfoExportFields, ApplicationName.WiFi);
    }
}
