namespace NETworkManager.Models.Network;

/// <summary>
/// Contains the information of an SNMP error in an <see cref="SNMPClient"/>.
/// </summary>
public class SNMPErrorArgs : System.EventArgs
{
    public string Message { get; }

    public bool SNMPV3HasErrorCode { get; }
    
    public SNMPV3ErrorCode SNMPV3ErrorCode { get; }
    
    public SNMPErrorArgs(string message)
    {
        Message = message;
    }
    
    public SNMPErrorArgs(SNMPV3ErrorCode errorCode)
    {
        SNMPV3HasErrorCode = true;
        SNMPV3ErrorCode = errorCode;
    }
}
