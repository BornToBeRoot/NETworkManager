using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using NETworkManager.Models.Settings;
using System;
using System.Globalization;

namespace NETworkManager.Models.Network
{
    public static class PortLookup
    {
        #region Variables
        private static string PortsFilePath = Path.Combine(ConfigurationManager.Current.ExecutionPath, "Resources", "ports.txt");

        private static List<PortLookupInfo> PortList;
        private static Lookup<int, PortLookupInfo> Ports;
        #endregion

        #region Methods
        static PortLookup()
        {
            PortList = new List<PortLookupInfo>();

            // Load list from resource folder (.txt-file)
            foreach (string line in File.ReadAllLines(PortsFilePath))
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                string[] portData = line.Split('|');

                int port = int.Parse(portData[0]);
                Protocol protocol = (Protocol)Enum.Parse(typeof(Protocol), portData[1]);

                // string key = GetKey(port, protocol);

                PortList.Add(new PortLookupInfo(port, protocol, portData[2], portData[3]));
            }

            Ports = (Lookup<int, PortLookupInfo>)PortList.ToLookup(x => x.Number);
        }

        public static Task<List<PortLookupInfo>> LookupAsync(int port)
        {
            return Task.Run(() => Lookup(port));
        }

        public static List<PortLookupInfo> Lookup(int port)
        {
            List<PortLookupInfo> list = new List<PortLookupInfo>();

            foreach (PortLookupInfo info in Ports[port])
            {
                list.Add(info);
            }

            return list;
        }

        public static Task<List<PortLookupInfo>> LookupAsync(List<int> ports)
        {
            return Task.Run(() => Lookup(ports));
        }

        public static List<PortLookupInfo> Lookup(List<int> ports)
        {
            List<PortLookupInfo> list = new List<PortLookupInfo>();

            foreach (int port in ports)
            {
                foreach (PortLookupInfo info in Ports[port])
                {
                    list.Add(info);
                }
            }

            return list;
        }

        public static Task<List<PortLookupInfo>> LookupByServiceAsync(List<string> portsByService)
        {
            return Task.Run(() => LookupByService(portsByService));
        }

        public static List<PortLookupInfo> LookupByService(List<string> portsByService)
        {
            List<PortLookupInfo> list = new List<PortLookupInfo>();

            foreach (PortLookupInfo info in PortList)
            {
                foreach (string portByService in portsByService)
                {
                    if (info.Service.IndexOf(portByService, StringComparison.OrdinalIgnoreCase) > 0 || info.Description.IndexOf(portByService, StringComparison.OrdinalIgnoreCase) > 0)
                        list.Add(info);
                }
            }

            return list;

        }
        #endregion

        public enum Protocol
        {
            tcp,
            udp,
            sctp,
            dccp
        }
    }
}
