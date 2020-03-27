using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using NETworkManager.Settings;
using System;
using System.Xml;

namespace NETworkManager.Models.Lookup
{
    public static partial class PortLookup
    {
        #region Variables
        private static readonly string PortsFilePath = ""; // ToDo //Path.Combine(ConfigurationManager.Current.ExecutionPath, "Resources", "Ports.xml");

        private static readonly List<PortLookupInfo> PortList;
        private static readonly Lookup<int, PortLookupInfo> Ports;
        #endregion

        #region Constructor
        static PortLookup()
        {
            PortList = new List<PortLookupInfo>();

            var document = new XmlDocument();
            document.Load(PortsFilePath);

            // ReSharper disable once PossibleNullReferenceException
            foreach (XmlNode node in document.SelectNodes("/Ports/Port"))
            {
                if (node == null)
                    continue;

                int.TryParse(node.SelectSingleNode("Number")?.InnerText, out var port);
                Enum.TryParse<Protocol>(node.SelectSingleNode("Protocol")?.InnerText, true, out var protocol);

                PortList.Add(new PortLookupInfo(port, protocol, node.SelectSingleNode("Name")?.InnerText, node.SelectSingleNode("Description")?.InnerText));
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
            return Ports[port].ToList();
        }

        public static Task<List<PortLookupInfo>> LookupByServiceAsync(List<string> portsByService)
        {
            return Task.Run(() => LookupByService(portsByService));
        }

        public static List<PortLookupInfo> LookupByService(List<string> portsByService)
        {
            var list = new List<PortLookupInfo>();

            foreach (var info in PortList)
            {
                foreach (var portByService in portsByService)
                {
                    if (info.Service.IndexOf(portByService, StringComparison.OrdinalIgnoreCase) > -1 || info.Description.IndexOf(portByService, StringComparison.OrdinalIgnoreCase) > -1)
                        list.Add(info);
                }
            }

            return list;

        }

#endregion
    }
}
