using System;
using System.Collections.Generic;
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

            if (wifiAdapters.Count == 0)
                return null;

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
                    SSID = network.Ssid
                });
            }

            if (wifiNetworks.Count == 0)
                return null;

            return wifiNetworks;
        }
    }
}