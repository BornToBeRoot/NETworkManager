namespace NETworkManager.Models.Network;

/// <summary>
/// Enum for the SNMP v3 authentication provider.
/// </summary>
public enum SNMPV3AuthenticationProvider
{
    /// <summary>
    /// MD5.
    /// </summary>
    MD5,

    /// <summary>
    /// SHA 1.
    /// </summary>
    SHA1,
    
    /// <summary>
    /// SHA 256.
    /// </summary>
    SHA256,

    /// <summary>
    /// SHA 384.
    /// </summary>
    SHA384,

    /// <summary>
    /// SHA 512.
    /// </summary>
    SHA512
}
