using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Xml;
using System.IO;
using System.Reflection;

namespace NETworkManager.Models.Lookup;

public static partial class PortLookup
{
    #region Variables
    /// <summary>
    /// Path to the xml file with all ports, protocols and services.
    /// </summary>
    private static readonly string PortsFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Resources", "Ports.xml");

    /// <summary>
    /// List of <see cref="PortLookupInfo"/> with all ports, protocols and services.
    /// </summary>
    private static readonly List<PortLookupInfo> PortList;

    /// <summary>
    /// Lookup of <see cref="PortLookupInfo"/> to quickly query the information by port.
    /// </summary>
    private static readonly Lookup<int, PortLookupInfo> Ports;
    #endregion

    #region Constructor
    /// <summary>
    /// Load all ports, protocols and services from the xml file.
    /// </summary>
    static PortLookup()
    {
        PortList = new List<PortLookupInfo>();

        var document = new XmlDocument();
        document.Load(PortsFilePath);

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
    /// <summary>
    /// Method to search for ports, protocols and services as <see cref="PortLookupInfo"/>
    /// by port number. For e.g. port 22 will return 22/tcp, 22/udp and 22/sctp.
    /// </summary>
    /// <param name="port">Port number to search.</param>
    /// <returns>List of ports, protocols and services as <see cref="PortLookupInfo"/>. Empty if nothing was found.</returns>
    public static Task<List<PortLookupInfo>> SearchByPortAsync(int port)
    {
        return Task.Run(() => SearchByPort(port));
    }

    /// <summary>
    /// Method to search for ports, protocols and services as <see cref="PortLookupInfo"/>
    /// by port number async. For e.g. port 22 will return 22/tcp, 22/udp and 22/sctp.
    /// </summary>
    /// <param name="port">Port number to search.</param>
    /// <returns>List of ports, protocols and services as <see cref="PortLookupInfo"/>. Empty if nothing was found.</returns>
    public static List<PortLookupInfo> SearchByPort(int port)
    {
        return Ports[port].ToList();
    }

    /// <summary>
    /// Method to search for ports, protocols and services as <see cref="PortLookupInfo"/>
    /// by service or description async. For e.g. "ssh" will return 22/tcp, 22/udp and 22/sctp.
    /// </summary>
    /// <param name="search">Service or description to search.</param>
    /// <returns>List of ports, protocols and services as <see cref="PortLookupInfo"/>. Empty if nothing was found.</returns>
    public static Task<List<PortLookupInfo>> SearchByServiceOrDescriptionAsync(string search)
    {
        return Task.Run(() => SearchByServiceOrDescription(search));
    }

    /// <summary>
    /// Method to search for ports, protocols and services as <see cref="PortLookupInfo"/>
    /// by service or description. For e.g. "ssh" will return 22/tcp, 22/udp and 22/sctp.
    /// </summary>
    /// <param name="search">Service or description to search.</param>
    /// <returns>List of ports, protocols and services as <see cref="PortLookupInfo"/>. Empty if nothing was found.</returns>
    public static List<PortLookupInfo> SearchByServiceOrDescription(string search)
    {
        var list = new List<PortLookupInfo>();

        foreach (var info in PortList)
        {
            if (info.Service.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.Description.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1)
                list.Add(info);
        }

        return list;
    }

    /// <summary>
    /// Method to get a <see cref="PortLookupInfo"/> by port and protocol async.
    /// </summary>
    /// <param name="port">Port number.</param>
    /// <param name="protocol">Port protocol. Default is <see cref="Protocol.Tcp"/>.</param>
    /// <returns>Port, protocol and service as <see cref="PortLookupInfo"/>. Service and description is empty if not found.</returns>
    public static Task<PortLookupInfo> GetByPortAndProtocolAsync(int port, Protocol protocol = Protocol.Tcp)
    {
        return Task.Run(() => GetByPortAndProtocol(port, protocol));
    }

    /// <summary>
    /// Method to get a <see cref="PortLookupInfo"/> by port and protocol.
    /// </summary>
    /// <param name="port">Port number.</param>
    /// <param name="protocol">Port protocol.</param>
    /// <returns>Port, protocol and service as <see cref="PortLookupInfo"/>. Service and description is empty if not found.</returns>
    public static PortLookupInfo GetByPortAndProtocol(int port, Protocol protocol = Protocol.Tcp)
    {
        return Ports[port].ToList().FirstOrDefault(x => x.Protocol.Equals(protocol)) ?? new PortLookupInfo(port, protocol, "-/-", "-/-");
    }
    #endregion
}
