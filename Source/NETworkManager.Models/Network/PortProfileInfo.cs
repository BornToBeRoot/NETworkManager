using System;

namespace NETworkManager.Models.Network;

public class PortProfileInfo
{
    public PortProfileInfo()
    {
    }

    /// <summary>
    ///     Creates a new port profile with a new <see cref="Id" />.
    /// </summary>
    /// <param name="name">Name of the port profile.</param>
    /// <param name="ports">Ports of the port profile.</param>
    public PortProfileInfo(string name, string ports)
    {
        Id = Guid.NewGuid();
        Name = name;
        Ports = ports;
    }

    /// <summary>
    ///     Internal ID of the port profile - profiles reference a port profile by it, so a renamed port profile is
    ///     still found. Kept when the port profile is edited.
    /// </summary>
    public Guid Id { get; set; }

    public string Name { get; set; }
    public string Ports { get; set; }
}