using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;

//https://docs.microsoft.com/en-us/uwp/api/windows.devices.wifi.wifiadapter.requestaccessasync
//var access = await WiFiAdapter.RequestAccessAsync() == WiFiAccessStatus.Allowed;

namespace NETworkManager.Models.Network;

public static class WiFi
{
    /// <summary>
    /// Get all WiFi adapters async with additional information from <see cref="NetworkInterface"/>.
    /// </summary>
    /// <returns><see cref="WiFiAdapterInfo"/> with <see cref="NetworkInterface"/> and <see cref="WiFiAdapter"/> as <see cref="List{T}"/>.</returns>
    public static async Task<List<WiFiAdapterInfo>> GetAdapterAsync()
    {
        List<WiFiAdapterInfo> wifiAdapterInfos = new();

        IReadOnlyList<WiFiAdapter> wifiAdapters = await WiFiAdapter.FindAllAdaptersAsync();

        if (wifiAdapters.Count > 0)
        {
            List<NetworkInterfaceInfo> networkInterfaces = await NetworkInterface.GetNetworkInterfacesAsync();

            foreach (var wifiAdapter in wifiAdapters)
            {
                var networkInteraceInfo = networkInterfaces.FirstOrDefault(x => x.Id.TrimStart('{').TrimEnd('}').Equals(wifiAdapter.NetworkAdapter.NetworkAdapterId.ToString(), StringComparison.OrdinalIgnoreCase));

                wifiAdapterInfos.Add(new WiFiAdapterInfo
                {
                    NetworkInterfaceInfo = networkInteraceInfo,
                    WiFiAdapter = wifiAdapter
                });
            }
        }

        return wifiAdapterInfos;
    }

    /// <summary>
    /// Get all available WiFi networks for an adapter with additional informations.
    /// </summary>
    /// <param name="wifiAdapter">WiFi adapter as <see cref="WiFiAdapter"/>.</param>
    /// <returns>A report as <see cref="WiFiNetworkScanInfo"/> including a list of <see cref="WiFiNetworkInfo"/>.</returns>
    public static async Task<WiFiNetworkScanInfo> GetNetworksAsync(WiFiAdapter wifiAdapter)
    {
        // Scan network adapter async
        await wifiAdapter.ScanAsync();

        // Try to get the current connected wifi network of this network adapter
        var (ssid, bssid) = TryGetConnectedNetworkFromWiFiAdapter(wifiAdapter.NetworkAdapter.NetworkAdapterId.ToString());

        List<WiFiNetworkInfo> wifiNetworkInfos = new();

        foreach (var availableNetwork in wifiAdapter.NetworkReport.AvailableNetworks)
        {
            wifiNetworkInfos.Add(new WiFiNetworkInfo()
            {
                AvailableNetwork = availableNetwork,
                IsHidden = string.IsNullOrEmpty(availableNetwork.Ssid),
                IsConnected = availableNetwork.Bssid.Equals(bssid, StringComparison.OrdinalIgnoreCase)
            });
        }

        return new WiFiNetworkScanInfo()
        {
            NetworkAdapterId = wifiAdapter.NetworkAdapter.NetworkAdapterId,
            WiFiNetworkInfos = wifiNetworkInfos,
            Timestamp = wifiAdapter.NetworkReport.Timestamp.LocalDateTime
        };
    }

    /// <summary>
    /// Try to get the current connected wifi network (SSID and BSSID) of a network adapter from 
    /// netsh.exe.
    /// 
    /// Calling netsh.exe and parsing the output feels so dirty, but Microsoft's API returns only 
    /// the WLAN profile and the SSID of the connected network. The BSSID is needed to find a 
    /// specific access point among several.
    /// </summary>
    /// <param name="wifiAdapterId">GUID of the network adapter.</param>
    /// <returns>SSID and BSSID of the connected wifi network. Values are null if not detected.</returns>
    private static (string SSID, string BSSID) TryGetConnectedNetworkFromWiFiAdapter(string wifiAdapterId)
    {
        string ssid = null;
        string bssid = null;

        using (System.Management.Automation.PowerShell powerShell = System.Management.Automation.PowerShell.Create())
        {
            powerShell.AddScript("netsh wlan show interfaces");

            Collection<PSObject> psOutputs = powerShell.Invoke();

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

            bool foundAdapter = false;

            foreach (PSObject outputItem in psOutputs)
            {
                // Find line with the network adapter id...
                if (outputItem.ToString().Contains(wifiAdapterId, StringComparison.OrdinalIgnoreCase))
                    foundAdapter = true;

                if (foundAdapter)
                {
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
        }

        return (ssid, bssid);
    }

    /// <summary>
    /// Disconnect the wifi adapter from the current wifi network.
    /// </summary>
    /// <param name="wifiAdapter">WiFi adapter from which the wifi network should be disconnected.</param>
    public static void Disconnect(WiFiAdapter wifiAdapter)
    {
        wifiAdapter.Disconnect();
    }

    /// <summary>
    /// Get the connect mode of a wifi network like Open, Eap (WPA2-Enterprise) 
    /// or Psk (WPA2-Personal).
    /// </summary>
    /// <param name="network">WiFi network as <see cref="WiFiAvailableNetwork"/>.</param>
    /// <returns>Connect mode as <see cref="WiFiConnectMode"/>.</returns>
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
    /// Check if WPS is available for a wifi network.
    /// </summary>
    /// <param name="adapter">WiFi adapter as <see cref="WiFiAdapter"/>.</param>
    /// <param name="network">WiFi network as <see cref="WiFiAvailableNetwork"/>.</param>
    /// <returns></returns>
    public static async Task<bool> IsWpsAvailable(WiFiAdapter adapter, WiFiAvailableNetwork network)
    {
        var result = await adapter.GetWpsConfigurationAsync(network);

        return result.SupportedWpsKinds.Contains(WiFiWpsKind.PushButton);
    }

    /// <summary>
    /// Get the WiFi channel from channel frequency.
    /// </summary>
    /// <param name="kilohertz">Input like 2422000 or 5240000.</param>
    /// <returns>WiFi channel like 3 or 48.</returns>
    public static int GetChannelFromChannelFrequency(int kilohertz)
    {
        return (double)ConvertChannelFrequencyToGigahertz(kilohertz) switch
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
            _ => -1,
        };
    }

    /// <summary>
    /// Convert the channel frequency to gigahertz.
    /// </summary>
    /// <param name="kilohertz">Frequency in kilohertz like 2422000 or 5240000.</param>
    /// <returns>Frequency in gigahertz like 2.422 or 5.240.</returns>
    public static double ConvertChannelFrequencyToGigahertz(int kilohertz)
    {
        return Convert.ToDouble(kilohertz) / 1000 / 1000;
    }
        
    public static bool Is2dot4GHzNetwork(int kilohertz)
    {
        var x = ConvertChannelFrequencyToGigahertz(kilohertz);

        return x >= 2.412 && x <= 2.472;
    }

    public static bool Is5GHzNetwork(int kilohertz)
    {
        var x = ConvertChannelFrequencyToGigahertz(kilohertz);

        return x >= 5.180 && x <= 5.825;
    }


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
            NetworkAuthenticationType.Unknown => "Unkown",
            NetworkAuthenticationType.None => "-/-",
            _ => "-/-",
        };
    }


    public static string GetHumandReadablePhyKind(WiFiPhyKind phyKind)
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
            _ => "-/-",
        };
    }
}