namespace NETworkManager.Models.Network;

/// <summary>
///     Enum for the SNMP v3 security.
/// </summary>
public enum SNMPV3Security
{
    /// <summary>
    ///     No authentication and no privacy.
    /// </summary>
    NoAuthNoPriv,

    /// <summary>
    ///     Authentication and no privacy.
    /// </summary>
    AuthNoPriv,

    /// <summary>
    ///     Authentication and privacy.
    /// </summary>
    AuthPriv
}