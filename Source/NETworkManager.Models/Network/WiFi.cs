using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;

//https://docs.microsoft.com/en-us/uwp/api/windows.devices.wifi.wifiadapter.requestaccessasync
//var access = await WiFiAdapter.RequestAccessAsync();

namespace NETworkManager.Models.Network
{
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
            switch (ConvertChannelFrequencyToGigahertz(kilohertz))
            {
                // 2.4 GHz
                case 2.412:
                    return 1;
                case 2.417:
                    return 2;
                case 2.422:
                    return 3;
                case 2.427:
                    return 4;
                case 2.432:
                    return 5;
                case 2.437:
                    return 6;
                case 2.442:
                    return 7;
                case 2.447:
                    return 8;
                case 2.452:
                    return 9;
                case 2.457:
                    return 10;
                case 2.462:
                    return 11;
                case 2.467:
                    return 12;
                case 2.472:
                    return 13;
                // 5 GHz
                case 5.180:
                    return 36;
                case 5.200:
                    return 40;
                case 5.220:
                    return 44;
                case 5.240:
                    return 48;
                case 5.260:
                    return 52;
                case 5.280:
                    return 56;
                case 5.300:
                    return 60;
                case 5.320:
                    return 64;
                case 5.500:
                    return 100;
                case 5.520:
                    return 104;
                case 5.540:
                    return 108;
                case 5.560:
                    return 112;
                case 5.580:
                    return 116;
                case 5.600:
                    return 120;
                case 5.620:
                    return 124;
                case 5.640:
                    return 128;
                case 5.660:
                    return 132;
                case 5.680:
                    return 136;
                case 5.700:
                    return 140;
                case 5.720:
                    return 144;
                case 5.745:
                    return 149;
                case 5.765:
                    return 153;
                case 5.785:
                    return 157;
                case 5.805:
                    return 161;
                case 5.825:
                    return 165;
            }

            return -1;
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
            switch (networkAuthenticationType)
            {
                case NetworkAuthenticationType.Open80211:
                    return "Open";
                case NetworkAuthenticationType.Rsna:
                    return "WPA2 Enterprise";
                case NetworkAuthenticationType.RsnaPsk:
                    return "WPA2 PSK";
                case NetworkAuthenticationType.Wpa:
                    return "WPA Enterprise";
                case NetworkAuthenticationType.WpaNone:
                    return "WPA None";
                case NetworkAuthenticationType.WpaPsk:
                    return "WPA PSK";
                case NetworkAuthenticationType.SharedKey80211:
                    return "WEP";
                case NetworkAuthenticationType.Ihv:
                    return "IHV";
                case NetworkAuthenticationType.Unknown:
                    return "Unkown";
                case NetworkAuthenticationType.None:
                    return "-/-";
            }

            return "-/-";
        }

        public static string GetHumandReadablePhyKind(WiFiPhyKind phyKind)
        {
            switch (phyKind)
            {
                case WiFiPhyKind.Dsss:
                case WiFiPhyKind.Fhss:
                    return "802.11";
                case WiFiPhyKind.Ofdm:
                    return "802.11a";
                case WiFiPhyKind.Hrdsss:
                    return "802.11b";
                case WiFiPhyKind.Erp:
                    return "802.11g";
                    break;
                case WiFiPhyKind.HT:
                    return "802.11n";
                case WiFiPhyKind.Dmg:
                    return "802.11ad";
                case WiFiPhyKind.Vht:
                    return "802.11ac";
                case WiFiPhyKind.HE:
                    return "802.11ax";
            }

            return "-/-";
        }

        public enum Radio
        {
            One,
            Two
        }
    }
}