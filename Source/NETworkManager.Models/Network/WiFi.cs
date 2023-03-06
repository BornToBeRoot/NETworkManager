using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;

//https://docs.microsoft.com/en-us/uwp/api/windows.devices.wifi.wifiadapter.requestaccessasync
//var access = await WiFiAdapter.RequestAccessAsync();

namespace NETworkManager.Models.Network;

public class WiFi
{
    public static async Task<List<WiFiAdapterInfo>> GetAdapterAsync()
    {
        List<WiFiAdapterInfo> wifiAdapters = new List<WiFiAdapterInfo>();
        List<NetworkInterfaceInfo> networkInterfaces = await NetworkInterface.GetNetworkInterfacesAsync();

        foreach (WiFiAdapter wifiAdapter in await WiFiAdapter.FindAllAdaptersAsync())
        {
            foreach (NetworkInterfaceInfo networkInterface in networkInterfaces)
            {
                if (!wifiAdapter.NetworkAdapter.NetworkAdapterId.ToString().Equals(networkInterface.Id.TrimStart('{').TrimEnd('}'), StringComparison.OrdinalIgnoreCase))
                    continue;

                wifiAdapters.Add(new WiFiAdapterInfo
                {
                    NetworkInterfaceInfo = networkInterface,
                    WiFiAdapter = wifiAdapter
                });
            }
        }

        return wifiAdapters;
    }

    public static async Task<IEnumerable<WiFiNetworkInfo>> GetNetworksAsync(WiFiAdapter adapter)
    {
        List<WiFiNetworkInfo> wifiNetworks = new List<WiFiNetworkInfo>();

        await adapter.ScanAsync();

        foreach (var network in adapter.NetworkReport.AvailableNetworks)
        {
            wifiNetworks.Add(new WiFiNetworkInfo()
            {
                BSSID = network.Bssid,
                SSID = network.Ssid,
                ChannelCenterFrequencyInKilohertz = network.ChannelCenterFrequencyInKilohertz,
                SignalBars = network.SignalBars,
                IsWiFiDirect = network.IsWiFiDirect,
                NetworkRssiInDecibelMilliwatts = network.NetworkRssiInDecibelMilliwatts,
                PhyKind = network.PhyKind,
                NetworkKind = network.NetworkKind,
                AuthenticationType = network.SecuritySettings.NetworkAuthenticationType,
                EncryptionType = network.SecuritySettings.NetworkEncryptionType,
                BeaconInterval = network.BeaconInterval,
                Uptime = network.Uptime
            });
        }

        return wifiNetworks;
    }

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

    public enum Radio
    {
        One,
        Two
    }
}