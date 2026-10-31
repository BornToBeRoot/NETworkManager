namespace NETworkManager.Models.Network;

/// <summary>
///     System information of a host read via SNMP, see <see cref="SNMPSystemInfoResolver" />.
/// </summary>
/// <param name="Name">Name of the host (sysName).</param>
/// <param name="Description">Description of the host, e.g. model and firmware (sysDescr).</param>
/// <param name="Location">Location of the host (sysLocation).</param>
public sealed record SNMPSystemInfo(string Name, string Description, string Location);
