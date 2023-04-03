using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public static async Task<IEnumerable<WiFiNetworkInfo>> GetNetworksAsync(WiFiAdapter adapter)
    {
        List<WiFiNetworkInfo> wifiNetworkInfos = new();

        await adapter.ScanAsync();

        foreach (var availableNetwork in adapter.NetworkReport.AvailableNetworks)
        {
            wifiNetworkInfos.Add(new WiFiNetworkInfo()
            {
                AvailableNetwork = availableNetwork,
                IsConnected = false
            });
        }

        //_ = GetConnectionProfile();

        return wifiNetworkInfos;
    }

    /*
    public static ConnectionProfile GetConnectionProfile()
    {
        var x = NetworkInformation.GetConnectionProfiles();

        foreach(var y in x.Where(u => u.IsWwanConnectionProfile))
        {
            Debug.WriteLine(y);
        }

     
    return x.FirstOrDefault(u => u.IsWlanConnectionProfile);
    }
    */

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