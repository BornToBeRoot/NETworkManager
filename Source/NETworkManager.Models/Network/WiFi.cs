using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    /// 
    /// </summary>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="adapter"></param>
    /// <returns></returns>
    public static async Task<WiFiNetworkScanInfo> GetNetworksAsync(WiFiAdapter adapter)
    {
        // Scan network adapter async
        await adapter.ScanAsync();

        // Try to get the current connected wifi network of this network adapter
        var (ssid, bssid) = TryGetConnectedNetworkFromAdapter(adapter.NetworkAdapter.NetworkAdapterId.ToString());

        List<WiFiNetworkInfo> wifiNetworkInfos = new();

        foreach (var availableNetwork in adapter.NetworkReport.AvailableNetworks)
        {
            wifiNetworkInfos.Add(new WiFiNetworkInfo()
            {
                WiFiAvailableNetwork = availableNetwork,
                
                // Add scan timestamp from adapter...
                IsConnected = availableNetwork.Bssid.Equals(bssid, StringComparison.OrdinalIgnoreCase)
            });
        }

        return new WiFiNetworkScanInfo()
        {
            NetworkAdapterId = adapter.NetworkAdapter.NetworkAdapterId,
            WiFiNetworkInfos = wifiNetworkInfos,
            Timestamp = adapter.NetworkReport.Timestamp.LocalDateTime
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
    /// <param name="networkAdapterId">GUID of the network adapter.</param>
    /// <returns>SSID and BSSID of the connected wifi network. Values are null if not detected.</returns>
    private static (string SSID, string BSSID) TryGetConnectedNetworkFromAdapter(string networkAdapterId)
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
                if (outputItem.ToString().Contains(networkAdapterId, StringComparison.OrdinalIgnoreCase))
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
    /// 
    /// </summary>
    /// <param name="kilohertz"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="kilohertz"></param>
    /// <returns></returns>
    public static double ConvertChannelFrequencyToGigahertz(int kilohertz)
    {
        return Convert.ToDouble(kilohertz) / 1000 / 1000;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="kilohertz"></param>
    /// <returns></returns>
    public static bool Is2dot4GHzNetwork(int kilohertz)
    {
        var x = ConvertChannelFrequencyToGigahertz(kilohertz);

        return x >= 2.412 && x <= 2.472;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="kilohertz"></param>
    /// <returns></returns>
    public static bool Is5GHzNetwork(int kilohertz)
    {
        var x = ConvertChannelFrequencyToGigahertz(kilohertz);

        return x >= 5.180 && x <= 5.825;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="networkAuthenticationType"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="phyKind"></param>
    /// <returns></returns>
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