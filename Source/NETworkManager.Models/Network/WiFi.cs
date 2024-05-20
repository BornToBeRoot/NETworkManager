using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;
using log4net;

//https://docs.microsoft.com/en-us/uwp/api/windows.devices.wifi.wifiadapter.requestaccessasync
//var access = await WiFiAdapter.RequestAccessAsync() == WiFiAccessStatus.Allowed;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class with WiFi related methods.
/// </summary>
public static class WiFi
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(WiFi));

    /// <summary>
    ///     Get all WiFi adapters async with additional information from <see cref="NetworkInterface" />.
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
                    WiFiAdapter = wiFiAdapter
                });
            else
                Log.Warn($"Could not find network interface for WiFi adapter with id: {wiFiAdapterId}");
        }

        return wifiAdapterInfos;
    }

    /// <summary>
    ///     Get all available WiFi networks for an adapter with additional information's.
    /// </summary>
    /// <param name="adapter">WiFi adapter as <see cref="WiFiAdapter" />.</param>
    /// <returns>A report as <see cref="WiFiNetworkScanInfo" /> including a list of <see cref="WiFiNetworkInfo" />.</returns>
    public static async Task<WiFiNetworkScanInfo> GetNetworksAsync(WiFiAdapter adapter)
    {
        // Scan network adapter async
        await adapter.ScanAsync();

        // Try to get the current connected wifi network of this network adapter
        var (_, bssid) = TryGetConnectedNetworkFromWiFiAdapter(adapter.NetworkAdapter.NetworkAdapterId.ToString());

        var wifiNetworkInfos = adapter.NetworkReport.AvailableNetworks.Select(availableNetwork => new WiFiNetworkInfo
        {
            AvailableNetwork = availableNetwork,
            IsHidden = string.IsNullOrEmpty(availableNetwork.Ssid),
            IsConnected = availableNetwork.Bssid.Equals(bssid, StringComparison.OrdinalIgnoreCase)
        }).ToList();

        return new WiFiNetworkScanInfo
        {
            NetworkAdapterId = adapter.NetworkAdapter.NetworkAdapterId,
            WiFiNetworkInfos = wifiNetworkInfos,
            Timestamp = adapter.NetworkReport.Timestamp.LocalDateTime
        };
    }

    /// <summary>
    ///     Try to get the current connected wifi network (SSID and BSSID) of a network adapter from
    ///     netsh.exe.
    ///     Calling netsh.exe and parsing the output feels so dirty, but Microsoft's API returns only
    ///     the WLAN profile and the SSID of the connected network. The BSSID is needed to find a
    ///     specific access point among several.
    /// </summary>
    /// <param name="adapterId">GUID of the WiFi network adapter.</param>
    /// <returns>SSID and BSSID of the connected wifi network. Values are null if not detected.</returns>
    // ReSharper disable once UnusedTupleComponentInReturnValue
    private static (string SSID, string BSSID) TryGetConnectedNetworkFromWiFiAdapter(string adapterId)
    {
        string ssid = null;
        string bssid = null;

        using (var powerShell = System.Management.Automation.PowerShell.Create())
        {
            powerShell.AddScript("netsh wlan show interfaces");

            var psOutputs = powerShell.Invoke();

            /*
            if (powerShell.Streams.Error.Count > 0) { // Handle error? }
            if (powerShell.Streams.Warning.Count > 0) { // Handle warning? }
            */

            /* Each object looks like this:
             *
             * Name : Wireless
             * Description : Intel ...
             * GUID : 90d8...
             * SSID : Devices
             * BSSID : 6a:d7:...
             */

            var foundAdapter = false;

            foreach (var outputItem in psOutputs)
            {
                // Find line with the network adapter id...
                if (outputItem.ToString().Contains(adapterId, StringComparison.OrdinalIgnoreCase))
                    foundAdapter = true;

                // ...and skip all lines until we found it.
                if (!foundAdapter)
                    continue;

                // Extract SSID from the line
                if (outputItem.ToString().Contains(" SSID ", StringComparison.OrdinalIgnoreCase))
                    ssid = outputItem.ToString().Split(':')[1].Trim();

                // Extract BSSID from the line
                if (outputItem.ToString().Contains(" BSSID ", StringComparison.OrdinalIgnoreCase))
                    bssid = outputItem.ToString().Split(':', 2)[1].Trim();

                // Break if we got the values, otherwise we might overwrite them
                // with values from another adapter.
                if (!string.IsNullOrEmpty(ssid) && !string.IsNullOrEmpty(bssid))
                    break;
            }
        }

        return (ssid, bssid);
    }

    /// <summary>
    ///     Connect to a WiFi network with Pre-shared key, EAP or no security.
    /// </summary>
    /// <param name="adapter">WiFi adapter which should be used for the connection.</param>
    /// <param name="network">WiFi network to connect to.</param>
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
    ///     Connect to a WiFi network with WPS push button.
    /// </summary>
    /// <param name="adapter">WiFi adapter which should be used for the connection.</param>
    /// <param name="network">WiFi network to connect to.</param>
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
    ///     Disconnect the wifi adapter from the current wifi network.
    /// </summary>
    /// <param name="adapter">WiFi adapter from which the wifi network should be disconnected.</param>
    public static void Disconnect(WiFiAdapter adapter)
    {
        adapter.Disconnect();
    }

    /// <summary>
    ///     Get the connect mode of a wifi network like Open, Eap (WPA2-Enterprise)
    ///     or Psk (WPA2-Personal).
    /// </summary>
    /// <param name="network">WiFi network as <see cref="WiFiAvailableNetwork" />.</param>
    /// <returns>Connect mode as <see cref="WiFiConnectMode" />.</returns>
    public static WiFiConnectMode GetConnectMode(WiFiAvailableNetwork network)
    {
        // Enterprise
        if (network.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.Rsna ||
            network.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.Wpa)
            return WiFiConnectMode.Eap;

        // Open
        if (network.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.Open80211 ||
            network.SecuritySettings.NetworkEncryptionType == NetworkEncryptionType.None)
            return WiFiConnectMode.Open;

        // Pre-Shared-Key
        return WiFiConnectMode.Psk;
    }

    /// <summary>
    ///     Check if WPS is available for a wifi network.
    /// </summary>
    /// <param name="adapter">WiFi adapter as <see cref="WiFiAdapter" />.</param>
    /// <param name="network">WiFi network as <see cref="WiFiAvailableNetwork" />.</param>
    /// <returns></returns>
    public static async Task<bool> IsWpsAvailable(WiFiAdapter adapter, WiFiAvailableNetwork network)
    {
        var result = await adapter.GetWpsConfigurationAsync(network);

        return result.SupportedWpsKinds.Contains(WiFiWpsKind.PushButton);
    }

    /// <summary>
    ///     Get the WiFi channel from channel frequency.
    /// </summary>
    /// <param name="kilohertz">Input like 2422000 or 5240000.</param>
    /// <returns>WiFi channel like 3 or 48.</returns>
    public static int GetChannelFromChannelFrequency(int kilohertz)
    {
        return ConvertChannelFrequencyToGigahertz(kilohertz) switch
        {
            // 2.4 GHz
            2.412 => 1,
            2.417 => 2,
            2.422 => 3,
            2.427 => 4,
            2.432 => 5,
            2.437 => 6,
            2.442 => 7,
            2.447 => 8,
            2.452 => 9,
            2.457 => 10,
            2.462 => 11,
            2.467 => 12,
            2.472 => 13,
            // 5 GHz
            5.180 => 36,
            5.200 => 40,
            5.220 => 44,
            5.240 => 48,
            5.260 => 52,
            5.280 => 56,
            5.300 => 60,
            5.320 => 64,
            5.500 => 100,
            5.520 => 104,
            5.540 => 108,
            5.560 => 112,
            5.580 => 116,
            5.600 => 120,
            5.620 => 124,
            5.640 => 128,
            5.660 => 132,
            5.680 => 136,
            5.700 => 140,
            5.720 => 144,
            5.745 => 149,
            5.765 => 153,
            5.785 => 157,
            5.805 => 161,
            5.825 => 165,
            _ => -1
        };
    }

    /// <summary>
    ///     Convert the channel frequency to gigahertz.
    /// </summary>
    /// <param name="kilohertz">Frequency in kilohertz like 2422000 or 5240000.</param>
    /// <returns>Frequency in gigahertz like 2.422 or 5.240.</returns>
    public static double ConvertChannelFrequencyToGigahertz(int kilohertz)
    {
        return Convert.ToDouble(kilohertz) / 1000 / 1000;
    }

    /// <summary>
    ///     Check if the WiFi network is a 2.4 GHz network.
    /// </summary>
    /// <param name="kilohertz">Frequency in kilohertz like 2422000 or 5240000.</param>
    /// <returns>True if WiFi network is 2.4 GHz.</returns>
    public static bool Is2dot4GHzNetwork(int kilohertz)
    {
        var x = ConvertChannelFrequencyToGigahertz(kilohertz);

        return x is >= 2.412 and <= 2.472;
    }

    /// <summary>
    ///     Check if the WiFi network is a 5 GHz network.
    /// </summary>
    /// <param name="kilohertz">Frequency in kilohertz like 2422000 or 5240000.</param>
    /// <returns>True if WiFi network is 5 GHz.</returns>
    public static bool Is5GHzNetwork(int kilohertz)
    {
        var x = ConvertChannelFrequencyToGigahertz(kilohertz);

        return x is >= 5.180 and <= 5.825;
    }

    /// <summary>
    ///     Get the human readable network authentication type.
    /// </summary>
    /// <param name="networkAuthenticationType">WiFi network authentication type as <see cref="NetworkAuthenticationType" />.</param>
    /// <returns>Human readable authentication type as string like "Open" or "WPA2 Enterprise".</returns>
    public static string GetHumanReadableNetworkAuthenticationType(NetworkAuthenticationType networkAuthenticationType)
    {
        return networkAuthenticationType switch
        {
            NetworkAuthenticationType.Open80211 => "Open",
            NetworkAuthenticationType.Rsna => "WPA2 Enterprise",
            NetworkAuthenticationType.RsnaPsk => "WPA2 PSK",
            NetworkAuthenticationType.Wpa => "WPA Enterprise",
            NetworkAuthenticationType.WpaNone => "WPA None",
            NetworkAuthenticationType.WpaPsk => "WPA PSK",
            NetworkAuthenticationType.SharedKey80211 => "WEP",
            NetworkAuthenticationType.Ihv => "IHV",
            NetworkAuthenticationType.Unknown => "Unknown",
            NetworkAuthenticationType.None => "-/-",
            _ => "-/-"
        };
    }

    /// <summary>
    ///     Get the human readable network phy kind.
    /// </summary>
    /// <param name="phyKind">WiFi network phy kind as <see cref="WiFiPhyKind" />.</param>
    /// <returns>Human readable phy kind as string like "802.11g" or "802.11ax".</returns>
    public static string GetHumanReadablePhyKind(WiFiPhyKind phyKind)
    {
        return phyKind switch
        {
            WiFiPhyKind.Dsss or WiFiPhyKind.Fhss => "802.11",
            WiFiPhyKind.Ofdm => "802.11a",
            WiFiPhyKind.Hrdsss => "802.11b",
            WiFiPhyKind.Erp => "802.11g",
            WiFiPhyKind.HT => "802.11n",
            WiFiPhyKind.Dmg => "802.11ad",
            WiFiPhyKind.Vht => "802.11ac",
            WiFiPhyKind.HE => "802.11ax",
            _ => "-/-"
        };
    }
}