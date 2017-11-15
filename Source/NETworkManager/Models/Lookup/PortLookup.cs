using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using NETworkManager.Models.Settings;
using System;
using System.Xml;

namespace NETworkManager.Models.Lookup
{
    public static class PortLookup
    {
        #region Variables
        private static string PortsFilePath = Path.Combine(ConfigurationManager.Current.ExecutionPath, "Resources", "Ports.xml");

        private static List<PortLookupInfo> PortList;
        private static Lookup<int, PortLookupInfo> Ports;
        #endregion

        #region Constructor
        static PortLookup()
        {
            PortList = new List<PortLookupInfo>();

            XmlDocument document = new XmlDocument();
            document.Load(PortsFilePath);

            foreach (XmlNode node in document.SelectNodes("/Ports/Port"))
            {
                int port = int.Parse(node.SelectSingleNode("Number").InnerText);
                Protocol protocol = (Protocol)Enum.Parse(typeof(Protocol), node.SelectSingleNode("Protocol").InnerText);

                PortList.Add(new PortLookupInfo(port, protocol, node.SelectSingleNode("Name").InnerText, node.SelectSingleNode("Description").InnerText));
            }

            Ports = (Lookup<int, PortLookupInfo>)PortList.ToLookup(x => x.Number);
        }
        #endregion

        #region Methods
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
                    if (info.Service.IndexOf(portByService, StringComparison.OrdinalIgnoreCase) > -1 || info.Description.IndexOf(portByService, StringComparison.OrdinalIgnoreCase) > -1)
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
