using System;

namespace NETworkManager.Models.Network;

/// <summary>
///     Service type profile of the mDNS Discovery tool: a name for several service types (e.g. "Printers" =
///     "_ipp._tcp; _ipps._tcp; ..."), which can be entered in the service type input (see
///     <see cref="MDNSServiceTypeProfile.TryResolve" />).
/// </summary>
public class MDNSServiceTypeProfileInfo : INamedReference
{
    public MDNSServiceTypeProfileInfo()
    {
    }

    /// <summary>
    ///     Creates a new service type profile with a new <see cref="Id" />.
    /// </summary>
    /// <param name="name">Name of the service type profile.</param>
    /// <param name="serviceTypes">Service types, separated by ";".</param>
    public MDNSServiceTypeProfileInfo(string name, string serviceTypes)
    {
        Id = Guid.NewGuid();
        Name = name;
        ServiceTypes = serviceTypes;
    }

    /// <summary>
    ///     Internal ID of the service type profile, kept when it is edited.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Name of the service type profile, entered in the service type input.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Service types, separated by ";" (e.g. "_ipp._tcp; _ipps._tcp").
    /// </summary>
    public string ServiceTypes { get; set; }
}
