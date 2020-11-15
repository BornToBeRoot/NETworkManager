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
            int channel = -1;

            switch (ConvertChannelFrequencyToGigahertz(kilohertz))
            {
                // 2.4 GHz
                case 2.412:
                    channel = 1;
                    break;
                case 2.417:
                    channel = 2;
                    break;
                case 2.422:
                    channel = 3;
                    break;
                case 2.427:
                    channel = 4;
                    break;
                case 2.432:
                    channel = 5;
                    break;
                case 2.437:
                    channel = 6;
                    break;
                case 2.442:
                    channel = 7;
                    break;
                case 2.447:
                    channel = 8;
                    break;
                case 2.452:
                    channel = 9;
                    break;
                case 2.457:
                    channel = 10;
                    break;
                case 2.462:
                    channel = 11;
                    break;
                case 2.467:
                    channel = 12;
                    break;
                case 2.472:
                    channel = 13;
                    break;
                // 5 GHz
                case 5.180:
                    channel = 36;
                    break;
                case 5.200:
                    channel = 40;
                    break;
                case 5.220:
                    channel = 44;
                    break;
                case 5.240:
                    channel = 48;
                    break;
                case 5.260:
                    channel = 52;
                    break;
                case 5.280:
                    channel = 56;
                    break;
                case 5.300:
                    channel = 60;
                    break;
                case 5.320:
                    channel = 64;
                    break;
                case 5.500:
                    channel = 100;
                    break;
                case 5.520:
                    channel = 104;
                    break;
                case 5.540:
                    channel = 108;
                    break;
                case 5.560:
                    channel = 112;
                    break;
                case 5.580:
                    channel = 116;
                    break;
                case 5.600:
                    channel = 120;
                    break;
                case 5.620:
                    channel = 124;
                    break;
                case 5.640:
                    channel = 128;
                    break;
                case 5.660:
                    channel = 132;
                    break;
                case 5.680:
                    channel = 136;
                    break;
                case 5.700:
                    channel = 140;
                    break;
                case 5.720:
                    channel = 144;
                    break;
                case 5.745:
                    channel = 149;
                    break;
                case 5.765:
                    channel = 153;
                    break;
                case 5.785:
                    channel = 157;
                    break;
                case 5.805:
                    channel = 161;
                    break;
                case 5.825:
                    channel = 165;
                    break;
            }

            return channel;
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
            string type = "-/-";

            switch (networkAuthenticationType)
            {
                case NetworkAuthenticationType.Open80211:
                    type = "Open";
                    break;
                case NetworkAuthenticationType.Rsna:
                    type = "WPA2 Enterprise";
                    break;
                case NetworkAuthenticationType.RsnaPsk:
                    type = "WPA2 PSK";
                    break;
                case NetworkAuthenticationType.Wpa:
                    type = "WPA Enterprise";
                    break;
                case NetworkAuthenticationType.WpaNone:
                    type = "WPA None";
                    break;
                case NetworkAuthenticationType.WpaPsk:
                    type = "WPA PSK";
                    break;
                case NetworkAuthenticationType.SharedKey80211:
                    type = "WEP";
                    break;
                case NetworkAuthenticationType.Ihv:
                    type = "IHV";
                    break;
                case NetworkAuthenticationType.Unknown:
                    type = "Unkown";
                    break;
                case NetworkAuthenticationType.None:
                    type = "-/-";
                    break;
            }

            return type;
        }

        public static string GetHumandReadablePhyKind(WiFiPhyKind phyKind)
        {
            string kind = "-/-";

            switch (phyKind)
            {
                case WiFiPhyKind.Dsss:
                case WiFiPhyKind.Fhss:
                    kind = "802.11";
                    break;
                case WiFiPhyKind.Ofdm:
                    kind = "802.11a";
                    break;
                case WiFiPhyKind.Hrdsss:
                    kind = "802.11b";
                    break;
                case WiFiPhyKind.Erp:
                    kind = "802.11g";
                    break;
                case WiFiPhyKind.HT:
                    kind = "802.11n";
                    break;
                case WiFiPhyKind.Dmg:
                    kind = "802.11ad";
                    break;
                case WiFiPhyKind.Vht:
                    kind = "802.11ac";
                    break;
                case WiFiPhyKind.HE:
                    kind = "802.11ax";
                    break;
            }

            return kind;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name", Justification = "<Pending>")]
        public enum Radio
        {
            One,
            Two
        }
    }
}