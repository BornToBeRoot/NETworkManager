namespace NETworkManager.Models.Network;

/// <summary>
///     System information of a host read via SNMP, see <see cref="SNMPSystemInfoResolver" />.
/// </summary>
/// <param name="Name">Name of the host (sysName).</param>
/// <param name="Description">Description of the host, e.g. model and firmware (sysDescr).</param>
/// <param name="Location">Location of the host (sysLocation).</param>
/// <param name="Error">
///     SNMPv3 error of the report of the host (e.g. unknown user, wrong password - the host answered, the values are
///     empty), <see cref="SNMPV3ErrorCode.None" /> otherwise.
/// </param>
public sealed record SNMPSystemInfo(
    string Name,
    string Description,
    string Location,
    SNMPV3ErrorCode Error = SNMPV3ErrorCode.None);
