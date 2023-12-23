using System;
using Lextm.SharpSnmpLib;

namespace NETworkManager.Models.Network;

/// <summary>
///     Contains the information of an SNMP error in an <see cref="SNMPClient" />.
/// </summary>
public class SNMPErrorArgs : EventArgs
{
    /// <summary>
    ///     Creates a new instance of <see cref="SNMPErrorArgs" /> with the given SNMP error message.
    /// </summary>
    /// <param name="errorMessage">SNMP error message</param>
    public SNMPErrorArgs(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="SNMPErrorArgs" /> with the given SNMP error code.
    /// </summary>
    /// <param name="errorCode">SNMP error code.</param>
    public SNMPErrorArgs(ErrorCode errorCode)
    {
        IsErrorCode = true;
        ErrorCode = errorCode;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="SNMPErrorArgs" /> with the given SNMPv3 error code.
    /// </summary>
    /// <param name="errorCode">SNMPv3 error code.</param>
    public SNMPErrorArgs(SNMPV3ErrorCode errorCode)
    {
        IsErrorCodeV3 = true;
        ErrorCodeV3 = errorCode;
    }

    /// <summary>
    ///     SNMP error message.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    ///     Indicate if the error code is an SNMP error code.
    /// </summary>
    public bool IsErrorCode { get; }

    /// <summary>
    ///     SNMP error code.
    /// </summary>
    public ErrorCode ErrorCode { get; }

    /// <summary>
    ///     Indicate if the error code is an SNMPv3 error code.
    /// </summary>
    public bool IsErrorCodeV3 { get; }

    /// <summary>
    ///     SNMPv3 error code.
    /// </summary>
    public SNMPV3ErrorCode ErrorCodeV3 { get; }
}