namespace NETworkManager.Models.Network;

/// <summary>
///     Enum for the SNMP v3 privacy provider.
/// </summary>
public enum SNMPV3PrivacyProvider
{
    /// <summary>
    ///     DES.
    /// </summary>
    DES,

    /// <summary>
    ///     AES with 128 bit.
    /// </summary>
    AES,

    /// <summary>
    ///     AES with 192 bit.
    /// </summary>
    AES192,

    /// <summary>
    ///     AES with 256 bit.
    /// </summary>
    AES256
}