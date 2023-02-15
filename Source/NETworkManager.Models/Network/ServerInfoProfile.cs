using System.Collections.Generic;

namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Class is used to store informations about a server profile.    
    /// </summary>
    public class ServerInfoProfile
    {
        /// <summary>
        /// Name of the server profile.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of servers as <see cref="ServerInfo"/>.
        /// </summary>
        public List<ServerInfo> Servers { get; set; } = new();

        /// <summary>
        /// Create an instance of <see cref="ServerInfoProfile"/>.
        /// </summary>
        public ServerInfoProfile()
        {

        }

        /// <summary>
        /// Create an instance of <see cref="ServerInfoProfile"/> with parameters.
        /// </summary>
        /// <param name="name">Name of the profile.</param>
        /// <param name="servers">List of servers as <see cref="ServerInfo"/>.</param>        
        public ServerInfoProfile(string name, List<ServerInfo> servers)
        {
            Name = name;
            Servers = servers;
        }
    }
}
