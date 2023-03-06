using System.Collections.Generic;

namespace NETworkManager.Models.Network;

/// <summary>
/// Class is used to store informations about a server profile.    
/// </summary>
public class ServerConnectionInfoProfile
{
    /// <summary>
    /// Name of the server profile.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// List of servers as <see cref="ServerConnectionInfo"/>.
    /// </summary>
    public List<ServerConnectionInfo> Servers { get; set; } = new();

    /// <summary>
    /// Create an instance of <see cref="ServerConnectionInfoProfile"/>.
    /// </summary>
    public ServerConnectionInfoProfile()
    {

    }

    /// <summary>
    /// Create an instance of <see cref="ServerConnectionInfoProfile"/> with parameters.
    /// </summary>
    /// <param name="name">Name of the profile.</param>
    /// <param name="servers">List of servers as <see cref="ServerConnectionInfo"/>.</param>        
    public ServerConnectionInfoProfile(string name, List<ServerConnectionInfo> servers)
    {
        Name = name;
        Servers = servers;
    }        
}
