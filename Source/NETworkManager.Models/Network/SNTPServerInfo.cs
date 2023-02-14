using System.Collections.Generic;

namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Class is used to store informations about SNTP servers.    
    /// </summary>
    public class SNTPServerInfo
    {
        /// <summary>
        /// Name of the SNTP server profile.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of SNTP servers as <see cref="ServerInfo"/>.
        /// </summary>
        public List<ServerInfo> Servers { get; set; }

        /// <summary>
        /// Create an instance of <see cref="SNTPServerInfo"/>.
        /// </summary>
        public SNTPServerInfo()
        {

        }

        /// <summary>
        /// Create an instance of <see cref="SNTPServerInfo"/> with parameters.
        /// </summary>
        /// <param name="name">Name of the profile.</param>
        /// <param name="servers">List of SNMP servers as <see cref="ServerInfo"/>.</param>        
        public SNTPServerInfo(string name, List<ServerInfo> servers)
        {
            Name = name;
            Servers = servers;
        }
    }
}
