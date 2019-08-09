using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.WiFi;

namespace NETworkManager.Models.Network
{
    public class WiFi
    {
        public static async Task<List<WiFiAdapterInfo>> GetWiFiAdapterAsync()
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
                        Id = wifiAdapter.NetworkAdapter.NetworkAdapterId
                    });
                }
            }

            if (wifiAdapters.Count == 0)
                return null;

            return wifiAdapters;
        }

        public static async Task<IEnumerable<WiFiNetworkInfo>> GetWiFiNetworksAsync(Guid adapterId)
        {
            //https://docs.microsoft.com/en-us/uwp/api/windows.devices.wifi.wifiadapter.requestaccessasync
            //var access = await WiFiAdapter.RequestAccessAsync();



            return null;
        }

        private static async void Test()
        {


            var result = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());

            if (result.Count >= 1)
            {
                // take first adapter
                WiFiAdapter adapter = await WiFiAdapter.FromIdAsync(result[0].Id);
                // scan for networks
                await adapter.ScanAsync();
                // find network with the correct SSID
                foreach (var x in adapter.NetworkReport.AvailableNetworks)
                {

                    Console.WriteLine("=== Adapter ===");
                    Console.WriteLine(x.Ssid);
                    Console.WriteLine(x.Bssid);
                    Console.WriteLine(x.SecuritySettings);

                }

                // connect 
                //await nwAdapter.ConnectAsync(nw, WiFiReconnectionKind.Automatic);
            }
        }
    }
}