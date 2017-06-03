using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using NETworkManager.Models.Settings;
using System;
using System.Windows;

namespace NETworkManager.Models.Network
{
    public static class PortLookup
    {
        #region Variables
        private static string PortsFilePath = Path.Combine(ConfigurationManager.Current.StartupPath, "Resources", "ports.txt");

        public static Lookup<int, PortInfo> Ports;
        #endregion

        #region Methods
        static PortLookup()
        {
            List<PortInfo> portList = new List<PortInfo>();

            // Load list from resource folder (.txt-file)
            foreach (string line in File.ReadAllLines(PortsFilePath))
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                string[] portData = line.Split('|');

                int port = int.Parse(portData[0]);
                Protocol protocol = (Protocol)Enum.Parse(typeof(Protocol), portData[1]);

                // string key = GetKey(port, protocol);

                portList.Add(new PortInfo(port, protocol, portData[2], portData[3]));
            }

            Ports = (Lookup<int, PortInfo>)portList.ToLookup(x => x.Number);
        }

        public static Task<List<PortInfo>> LookupAsync(int port)
        {
            return Task.Run(() => Lookup(port));
        }

        public static List<PortInfo> Lookup(int port)
        {
            List<PortInfo> list = new List<PortInfo>();

            foreach (PortInfo info in Ports[port])
            {
                list.Add(info);
            }

            return list;
        }

        public static Task<List<PortInfo>> LookupAsync(int[] ports)
        {
            return Task.Run(() => Lookup(ports));
        }

        public static List<PortInfo> Lookup(int[] ports)
        {
            List<PortInfo> list = new List<PortInfo>();

            foreach (int port in ports)
            {
                foreach (PortInfo info in Ports[port])
                {
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
