namespace NETworkManager.Models.Network;

/// <summary>
/// Enum for the SNMP v3 error codes.
/// </summary>
public enum SNMPV3ErrorCode
{
    /// <summary>
    /// No error.
    /// </summary>
    None,
    
    /// <summary>
    /// Unknown user name.
    /// </summary>
    UnknownUserName,
    
    /// <summary>
    ///  Authentication failed.
    /// </summary>
    AuthenticationFailed
}
