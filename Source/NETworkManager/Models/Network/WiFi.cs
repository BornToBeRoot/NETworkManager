using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.WiFi;

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
                    Security = network.SecuritySettings,
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
            }

            return channel;
        }

        public static double ConvertChannelFrequencyToGigahertz(int kilohertz)
        {
            return Convert.ToDouble(kilohertz) / 1000 / 1000;
        }
    }
}