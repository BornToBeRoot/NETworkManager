using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;
using NETworkManager.Models.Lookup;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class with Wi-Fi related methods.
/// </summary>
public static class WiFi
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(WiFi));

    /// <summary>
    ///     Get all Wi-Fi adapters async with additional information from <see cref="NetworkInterface" />.
    /// </summary>
    /// <returns>
    ///     <see cref="WiFiAdapterInfo" /> with <see cref="NetworkInterface" /> and <see cref="WiFiAdapter" /> as
    ///     <see cref="List{T}" />.
    /// </returns>
    public static async Task<List<WiFiAdapterInfo>> GetAdapterAsync()
    {
        List<WiFiAdapterInfo> wifiAdapterInfos = [];

        var wifiAdapters = await WiFiAdapter.FindAllAdaptersAsync();

        if (wifiAdapters.Count <= 0)
            return wifiAdapterInfos;

        var networkInterfaces = await NetworkInterface.GetNetworkInterfacesAsync();

        foreach (var wiFiAdapter in wifiAdapters)
        {
            var wiFiAdapterId = wiFiAdapter.NetworkAdapter.NetworkAdapterId.ToString();

            var networkInterface = networkInterfaces.FirstOrDefault(x => x.Id.TrimStart('{').TrimEnd('}')
                .Equals(wiFiAdapterId, StringComparison.OrdinalIgnoreCase));

            if (networkInterface != null)
                wifiAdapterInfos.Add(new WiFiAdapterInfo
                {
                    NetworkInterfaceInfo = networkInterface,
                    WiFiAdapter = wiFiAdapter,
                    IsOperational = networkInterface.IsOperational
                });
            else
                Log.Warn($"Could not find network interface for WiFi adapter with id: {wiFiAdapterId}");
        }

        return wifiAdapterInfos;
    }

    /// <summary>
    ///     Updates <see cref="WiFiAdapterInfo.IsOperational" /> of the adapters (e.g. after connecting or
    ///     disconnecting). Reads only the operational status of the network interfaces (fast), not all
    ///     their information like <see cref="NetworkInterface.GetNetworkInterfacesAsync" />.
    /// </summary>
    /// <param name="adapters">Wi-Fi adapters to update.</param>
    public static void UpdateAdapterStates(IEnumerable<WiFiAdapterInfo> adapters)
    {
        // Optional (only the icon of the adapter selection): an error keeps the previous states instead of failing the
        // scan it is called after
        try
        {
            var states = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            foreach (var networkInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                states.TryAdd(networkInterface.Id,
                    networkInterface.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up);

            foreach (var adapter in adapters)
                if (states.TryGetValue(adapter.NetworkInterfaceInfo.Id, out var isOperational))
                    adapter.IsOperational = isOperational;
        }
        catch (Exception ex)
        {
            Log.Error("Error while reading the state of the WiFi adapters.", ex);
        }
    }

    /// <summary>
    ///     Get all available Wi-Fi networks for an adapter with additional information's.
    /// </summary>
    /// <param name="adapter">WiFi adapter as <see cref="WiFiAdapter" />.</param>
    /// <returns>A report as <see cref="WiFiNetworkScanInfo" /> including a list of <see cref="WiFiNetworkInfo" />.</returns>
    public static async Task<WiFiNetworkScanInfo> GetNetworksAsync(WiFiAdapter adapter)
    {
        // Scan network adapter async
        await adapter.ScanAsync();

        // The rest (native calls, vendor lookups) runs in the background - the caller continues on the UI thread
        // after ScanAsync
        return await Task.Run(() => CreateScanInfo(adapter));
    }

    /// <summary>
    ///     Creates the result of a scan from the network report of the adapter.
    /// </summary>
    /// <param name="adapter">WiFi adapter as <see cref="WiFiAdapter" /> after the scan.</param>
    /// <returns>A report as <see cref="WiFiNetworkScanInfo" /> including a list of <see cref="WiFiNetworkInfo" />.</returns>
    private static WiFiNetworkScanInfo CreateScanInfo(WiFiAdapter adapter)
    {
        var adapterId = adapter.NetworkAdapter.NetworkAdapterId;

        // BSSID of the network this adapter is connected to (null if not connected or not read). The WinRT API does
        // not expose the channel bandwidth (and its center), so it is read from the native BSS list (wlanapi.dll)
        // and matched by BSSID below. Failures yield an empty map and we fall back to a heuristic per network.
        var (bssid, channels) = WlanApi.GetBssInfo(adapterId);

        var wifiNetworkInfos = new List<WiFiNetworkInfo>();

        foreach (var availableNetwork in adapter.NetworkReport.AvailableNetworks)
        {
            var channelFrequencyInGigahertz = ConvertChannelFrequencyToGigahertz(availableNetwork.ChannelCenterFrequencyInKilohertz);
            var radio = GetWiFiRadioFromChannelFrequency(channelFrequencyInGigahertz);
            var (channelBandwidth, channelBandwidthCenterFrequencyInGigahertz, isChannelBandwidthEstimated) =
                GetChannelBandwidth(channels, availableNetwork.Bssid, radio, availableNetwork.PhyKind,
                    channelFrequencyInGigahertz);

            var wifiNetworkInfo = new WiFiNetworkInfo
            {
                AvailableNetwork = availableNetwork,
                Bssid = MACAddressHelper.GetDefaultFormat(availableNetwork.Bssid),
                Radio = radio,
                ChannelCenterFrequencyInGigahertz = channelFrequencyInGigahertz,
                Channel = GetChannelFromChannelFrequency(channelFrequencyInGigahertz),
                ChannelBandwidth = channelBandwidth,
                ChannelBandwidthCenterFrequencyInGigahertz = channelBandwidthCenterFrequencyInGigahertz,
                IsChannelBandwidthEstimated = isChannelBandwidthEstimated,
                IsHidden = string.IsNullOrEmpty(availableNetwork.Ssid),
                IsConnected = availableNetwork.Bssid.Equals(bssid, StringComparison.OrdinalIgnoreCase),
                NetworkAuthenticationType = GetHumanReadableNetworkAuthenticationType(availableNetwork.SecuritySettings.NetworkAuthenticationType),
                Vendor = OUILookup.LookupByMacAddress(availableNetwork.Bssid).FirstOrDefault()?.Vendor ?? "-/-",
                PhyKind = GetHumanReadablePhyKind(availableNetwork.PhyKind)
            };

            wifiNetworkInfos.Add(wifiNetworkInfo);
        }

        return new WiFiNetworkScanInfo
        {
            WiFiNetworkInfos = wifiNetworkInfos,
            Timestamp = adapter.NetworkReport.Timestamp.LocalDateTime
        };
    }

    /// <summary>
    ///     Connect to a Wi-Fi network with Pre-shared key, EAP or no security.
    /// </summary>
    /// <param name="adapter">Wi-Fi adapter which should be used for the connection.</param>
    /// <param name="network">Wi-Fi network to connect to.</param>
    /// <param name="reconnectionKind">Reconnection type to automatically or manuel reconnect.</param>
    /// <param name="credential">Credentials for EAP or PSK. Empty for open networks.</param>
    /// <param name="ssid">SSID for hidden networks.</param>
    /// <returns></returns>
    public static async Task<WiFiConnectionStatus> ConnectAsync(WiFiAdapter adapter, WiFiAvailableNetwork network,
        WiFiReconnectionKind reconnectionKind, PasswordCredential credential, string ssid = null)
    {
        WiFiConnectionResult connectionResult;

        if (string.IsNullOrEmpty(ssid))
            connectionResult = await adapter.ConnectAsync(network, reconnectionKind, credential);
        else
            connectionResult = await adapter.ConnectAsync(network, reconnectionKind, credential, ssid);

        // Wrong password may cause connection to timeout.
        // Disconnect any network from the adapter to return it to a non-busy state.
        if (connectionResult.ConnectionStatus == WiFiConnectionStatus.Timeout)
            Disconnect(adapter);

        return connectionResult.ConnectionStatus;
    }

    /// <summary>
    ///     Connect to a Wi-Fi network with WPS push button.
    /// </summary>
    /// <param name="adapter">Wi-Fi adapter which should be used for the connection.</param>
    /// <param name="network">Wi-Fi network to connect to.</param>
    /// <param name="reconnectionKind">Reconnection type to automatically or manuel reconnect.</param>
    /// <returns></returns>
    public static async Task<WiFiConnectionStatus> ConnectWpsAsync(WiFiAdapter adapter, WiFiAvailableNetwork network,
        WiFiReconnectionKind reconnectionKind)
    {
        var connectionResult = await adapter.ConnectAsync(network, reconnectionKind, null,
            string.Empty, WiFiConnectionMethod.WpsPushButton);

        // Wrong password may cause connection to timeout.
        // Disconnect any network from the adapter to return it to a non-busy state.
        if (connectionResult.ConnectionStatus == WiFiConnectionStatus.Timeout)
            Disconnect(adapter);

        return connectionResult.ConnectionStatus;
    }

    /// <summary>
    ///     Disconnect the Wi-Fi adapter from the current Wi-Fi network.
    /// </summary>
    /// <param name="adapter">Wi-Fi adapter from which the Wi-Fi network should be disconnected.</param>
    public static void Disconnect(WiFiAdapter adapter)
    {
        adapter.Disconnect();
    }

    /// <summary>
    ///     Get the connect mode of a Wi-Fi network like Open, Eap (WPA2-Enterprise)
    ///     or Psk (WPA2-Personal).
    /// </summary>
    /// <param name="network">Wi-Fi network as <see cref="WiFiAvailableNetwork" />.</param>
    /// <returns>Connect mode as <see cref="WiFiConnectMode" />.</returns>
    public static WiFiConnectMode GetConnectMode(WiFiAvailableNetwork network)
    {
        return GetConnectMode(network.SecuritySettings.NetworkAuthenticationType,
            network.SecuritySettings.NetworkEncryptionType);
    }

    /// <summary>
    ///     Get the connect mode from the authentication and encryption type of a Wi-Fi network.
    /// </summary>
    /// <param name="authenticationType">Authentication type of the network.</param>
    /// <param name="encryptionType">Encryption type of the network.</param>
    /// <returns>Connect mode as <see cref="WiFiConnectMode" />.</returns>
    public static WiFiConnectMode GetConnectMode(NetworkAuthenticationType authenticationType,
        NetworkEncryptionType encryptionType)
    {
        // Enterprise (WPA, WPA2, WPA3)
        if (authenticationType is NetworkAuthenticationType.Rsna or NetworkAuthenticationType.Wpa
            or NetworkAuthenticationType.Wpa3Enterprise or NetworkAuthenticationType.Wpa3Enterprise192Bits)
            return WiFiConnectMode.Eap;

        // Open (no encryption), also OWE (Enhanced Open: encrypted, but without a password). Not Open80211 with WEP:
        // the authentication is open, but the WEP key is needed (Pre-Shared-Key)
        if (authenticationType == NetworkAuthenticationType.Owe || encryptionType == NetworkEncryptionType.None)
            return WiFiConnectMode.Open;

        // Pre-Shared-Key (WEP, WPA, WPA2, WPA3 SAE)
        return WiFiConnectMode.Psk;
    }

    /// <summary>
    ///     Check if WPS is available for a Wi-Fi network.
    /// </summary>
    /// <param name="adapter">WiFi adapter as <see cref="WiFiAdapter" />.</param>
    /// <param name="network">WiFi network as <see cref="WiFiAvailableNetwork" />.</param>
    /// <returns>Ture if WPS is available.</returns>
    public static async Task<bool> IsWpsAvailable(WiFiAdapter adapter, WiFiAvailableNetwork network)
    {
        var result = await adapter.GetWpsConfigurationAsync(network);

        return result.SupportedWpsKinds.Contains(WiFiWpsKind.PushButton);
    }

    /// <summary>
    ///     Get the Wi-Fi channel from channel frequency.
    /// </summary>
    /// <param name="gigahertz">Input like 2.422 or 5.240.</param>
    /// <returns>Wi-Fi channel like 3 or 48, -1 if the frequency is not a channel.</returns>
    public static int GetChannelFromChannelFrequency(double gigahertz)
    {
        // Convert to integer MHz to avoid floating-point precision issues from the
        // kilohertz / 1_000_000.0 conversion in ConvertChannelFrequencyToGigahertz.
        var mhz = (int)Math.Round(gigahertz * 1000);

        // 2.4 GHz: channels 1-13 follow f = 2407 + ch×5 MHz; channel 14 is at 2484 MHz
        if (mhz is >= 2412 and <= 2472)
            return (mhz - 2407) / 5;
        if (mhz == 2484)
            return 14;

        // 5 GHz: channels 32-177 follow f = 5000 + ch×5 MHz (169-177 = UNII-4)
        if (mhz is >= 5160 and <= 5885)
            return (mhz - 5000) / 5;

        // 6 GHz: channel 2 is at 5935 MHz, channels 1-233 follow f = 5950 + ch×5 MHz
        if (mhz == 5935)
            return 2;

        if (mhz is >= 5955 and <= 7115)
            return (mhz - 5950) / 5;

        return -1;
    }

    /// <summary>
    ///     Convert the channel frequency to gigahertz.
    /// </summary>
    /// <param name="kilohertz">Frequency in kilohertz like 2422000 or 5240000.</param>
    /// <returns>Frequency in gigahertz like 2.422 or 5.240.</returns>
    private static double ConvertChannelFrequencyToGigahertz(int kilohertz)
    {
        return Convert.ToDouble(kilohertz) / 1000 / 1000;
    }

    /// <summary>
    ///    Get the Wi-Fi radio like 2.4 GHz, 5 GHz, etc. from the channel frequency.
    /// </summary>
    /// <param name="gigahertz">Frequency in gigahertz like 2.412 or 5.180.</param>
    /// <returns>Radio like 2.4 GHz, 5 GHz, etc. as <see cref="WiFiRadio" />.</returns>
    public static WiFiRadio GetWiFiRadioFromChannelFrequency(double gigahertz)
    {
        return gigahertz switch
        {
            >= 2.412 and <= 2.484 => WiFiRadio.GHz2dot4,
            >= 5.160 and <= 5.885 => WiFiRadio.GHz5,
            >= 5.925 and <= 7.125 => WiFiRadio.GHz6,
            _ => WiFiRadio.Unknown
        };
    }

    /// <summary>
    ///     Determines the channel bandwidth (MHz) and its center frequency (GHz) for a network. Prefers
    ///     the values parsed from the native BSS list (<see cref="WlanApi" />); if unavailable, falls back
    ///     to a heuristic based on the radio band and PHY kind (centered on the primary channel, marked as estimated).
    /// </summary>
    private static (int Bandwidth, double CenterFrequencyInGigahertz, bool IsEstimated) GetChannelBandwidth(
        IReadOnlyDictionary<string, WlanApi.WlanBssChannel> channels, string networkBssid, WiFiRadio radio,
        WiFiPhyKind phyKind, double primaryFrequencyInGigahertz)
    {
        if (!string.IsNullOrEmpty(networkBssid) && channels.TryGetValue(networkBssid, out var channel) &&
            channel.BandwidthMHz > 0 && channel.CenterFrequencyMHz > 0)
            return (channel.BandwidthMHz, channel.CenterFrequencyMHz / 1000.0, false);

        return (GetHeuristicChannelBandwidth(radio, phyKind), primaryFrequencyInGigahertz, true);
    }

    /// <summary>
    ///     Estimates the channel bandwidth (MHz) from the radio band and PHY kind when the exact
    ///     value could not be read from the BSS list. 2.4 GHz is always 20 MHz: 40 MHz is supported since
    ///     802.11n, but rarely used (only 3 channels without overlap).
    /// </summary>
    private static int GetHeuristicChannelBandwidth(WiFiRadio radio, WiFiPhyKind phyKind)
    {
        return radio switch
        {
            WiFiRadio.GHz2dot4 => 20,
            WiFiRadio.GHz5 => phyKind switch
            {
                WiFiPhyKind.Vht or WiFiPhyKind.HE or WiFiPhyKind.Eht => 80,
                WiFiPhyKind.HT => 40,
                _ => 20
            },
            WiFiRadio.GHz6 => phyKind is WiFiPhyKind.HE or WiFiPhyKind.Eht ? 160 : 20,
            _ => 20
        };
    }

    /// <summary>
    ///     Get the human-readable network authentication type.
    /// </summary>
    /// <param name="networkAuthenticationType">Wi-Fi network authentication type as <see cref="NetworkAuthenticationType" />.</param>
    /// <returns>Human-readable authentication type as string like "Open" or "WPA2 Enterprise".</returns>
    public static string GetHumanReadableNetworkAuthenticationType(NetworkAuthenticationType networkAuthenticationType)
    {
        return networkAuthenticationType switch
        {
            // No value like an unknown vendor ("Unknown" is not translated in the model)
            NetworkAuthenticationType.None or NetworkAuthenticationType.Unknown => "-/-",
            NetworkAuthenticationType.Open80211 => "Open",
            NetworkAuthenticationType.SharedKey80211 => "WEP",
            NetworkAuthenticationType.Wpa => "WPA Enterprise",
            NetworkAuthenticationType.WpaPsk => "WPA PSK",
            NetworkAuthenticationType.WpaNone => "WPA None",
            NetworkAuthenticationType.Rsna => "WPA2 Enterprise",
            NetworkAuthenticationType.RsnaPsk => "WPA2 Personal (PSK)",
            NetworkAuthenticationType.Ihv => "IHV",
            NetworkAuthenticationType.Wpa3Enterprise192Bits => "WPA3 Enterprise (192-bit)", // Same as Wpa3
            NetworkAuthenticationType.Wpa3Sae => "WPA3 Personal (SAE)",
            NetworkAuthenticationType.Owe => "OWE",
            NetworkAuthenticationType.Wpa3Enterprise => "WPA3 Enterprise",
            _ => "-/-"
        };
    }

    /// <summary>
    ///     Get the human-readable network phy kind.
    /// </summary>
    /// <param name="phyKind">Wi-Fi network phy kind as <see cref="WiFiPhyKind" />.</param>
    /// <returns>Human-readable phy kind as string like "802.11g" or "802.11ax".</returns>
    public static string GetHumanReadablePhyKind(WiFiPhyKind phyKind)
    {
        return phyKind switch
        {
            WiFiPhyKind.Unknown => "-/-",
            WiFiPhyKind.Dsss or WiFiPhyKind.Fhss => "802.11",
            WiFiPhyKind.Ofdm => "802.11a",
            WiFiPhyKind.Hrdsss => "802.11b",
            WiFiPhyKind.Erp => "802.11g",
            WiFiPhyKind.HT => "802.11n",
            WiFiPhyKind.Vht => "802.11ac",
            WiFiPhyKind.Dmg => "802.11ad",
            WiFiPhyKind.HE => "802.11ax",
            WiFiPhyKind.Eht => "802.11be",
            _ => "-/-"
        };
    }
}