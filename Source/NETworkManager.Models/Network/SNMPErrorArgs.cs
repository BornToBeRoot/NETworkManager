namespace NETworkManager.Models.Network;

/// <summary>
/// Contains the information of an SNMP error in an <see cref="SNMPClient"/>.
/// </summary>
public class SNMPErrorArgs : System.EventArgs
{
    /// <summary>
    /// SNMP error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Indicates if the error is a known SNMPv3 error.
    /// </summary>
    public bool SNMPV3HasErrorCode { get; }
    
    /// <summary>
    /// SNMPv3 error code.
    /// </summary>
    public SNMPV3ErrorCode SNMPV3ErrorCode { get; }
    
    /// <summary>
    /// Creates a new instance of <see cref="SNMPErrorArgs"/> with the given error message.
    /// </summary>
    /// <param name="message">SNMP error message.</param>
    public SNMPErrorArgs(string message)
    {
        Message = message;
    }
    
    /// <summary>
    /// Creates a new instance of <see cref="SNMPErrorArgs"/> with the given SNMPv3 error code.
    /// </summary>
    /// <param name="errorCode">SNMPv3 error code.</param>
    public SNMPErrorArgs(SNMPV3ErrorCode errorCode)
    {
        SNMPV3HasErrorCode = true;
        SNMPV3ErrorCode = errorCode;
    }
}
