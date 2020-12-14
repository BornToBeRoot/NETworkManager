using System.Collections.Generic;

namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Class is used to store informations about DNS servers.
    /// It weather a custom DNS server with 
    /// </summary>
    public class DNSServerInfo
    {
        /// <summary>
        /// Name of the DNS server profile.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of DNS server ip addresses as <see cref="string"/>.
        /// </summary>
        public List<string> Servers { get; set; }

        /// <summary>
        /// Port which is used for the DNS request.
        /// </summary>
        public int Port { get; set; } = 53;

        /// <summary>
        /// Indicates if the windows DNS server is used. 
        /// </summary>
        public bool UseWindowsDNSServer { get; set; }

        /// <summary>
        /// Create an empty instance of <see cref="DNSServerInfo"/>. If an empty instance is created, 
        /// the windows DNS server is used.
        /// </summary>
        public DNSServerInfo()
        {
            UseWindowsDNSServer = true;
        }

        /// <summary>
        /// Create an instance of <see cref="DNSServerInfo"/> with DNS server and port.
        /// </summary>
        /// <param name="name">Name of the profile.</param>
        /// <param name="servers">List of DNS server ip addresses as <see cref="string"/>.</param>
        /// <param name="port">Port which is used for the DNS request.</param>
        public DNSServerInfo(string name, List<string> servers, int port = 53)
        {
            UseWindowsDNSServer = false;
            Name = name;
            Servers = servers;
            Port = port;
        }
    }
}