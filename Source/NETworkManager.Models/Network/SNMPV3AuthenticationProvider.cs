namespace NETworkManager.Models.Network;

/// <summary>
/// Enum for the SNMP v3 authentication provider.
/// </summary>
public enum SNMPV3AuthenticationProvider
{
    /// <summary>
    /// MD5.
    /// </summary>
    Md5,

    /// <summary>
    /// SHA 1.
    /// </summary>
    Sha1,
    
    /// <summary>
    /// SHA 256.
    /// </summary>
    Sha256,

    /// <summary>
    /// SHA 384.
    /// </summary>
    Sha384,

    /// <summary>
    /// SHA 512.
    /// </summary>
    Sha512
}
