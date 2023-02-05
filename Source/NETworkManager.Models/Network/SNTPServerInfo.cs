using System;
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
        /// List of SNTP servers as <see cref="Tuple{string, int}"/>.
        /// </summary>
        public List<(string Server, int Port)> Servers { get; set; }

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
        /// <param name="servers">List of SNMP servers as <see cref="Tuple{string, int}"/>.</param>        
        public SNTPServerInfo(string name, List<(string Server, int Port)> servers)
        {
            Name = name;
            Servers = servers;
        }
    }
}
