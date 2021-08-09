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
        /// List of DNS servers as with ip address and port./>.
        /// </summary>
        public List<DNSServerClassicInfo> Servers { get; set; }
        
        /// <summary>
        /// Indicates if DoH is used.
        /// </summary>
        public bool UseDoHServer { get; set; }

        /// <summary>
        /// List of DNS server URLs.
        /// </summary>
        public List<DNSServerDoHInfo> DoHServers { get; set; }

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
            
        }

        /// <summary>
        /// Create an instance of <see cref="DNSServerInfo"/> with DNS servers.
        /// </summary>
        /// <param name="name">Name of the profile.</param>
        /// <param name="servers">List of DNS servers.</param>        
        public DNSServerInfo(string name, List<DNSServerClassicInfo> servers)
        {
            Name = name;
            Servers = servers;
        }

        /// <summary>
        /// Create an instance of <see cref="DNSServerInfo"/> with DNS servers.
        /// </summary>
        /// <param name="name">Name of the profile.</param>
        /// <param name="doHServers">List of DNS servers.</param>
        public DNSServerInfo(string name, List<DNSServerDoHInfo> doHServers)
        {
            Name = name;
            UseDoHServer = true;
            DoHServers = doHServers;
        }
    }
}
