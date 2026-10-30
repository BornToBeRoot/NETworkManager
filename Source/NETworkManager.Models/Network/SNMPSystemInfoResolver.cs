using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;

namespace NETworkManager.Models.Network;

/// <summary>
///     Reads the system information (sysName, sysDescr, sysLocation of the MIB-II system group) of a host with an SNMP
///     v1 / v2c GET request to UDP port 161. A host with another community usually doesn't answer at all (timeout).
/// </summary>
public static class SNMPSystemInfoResolver
{
    private const int SNMPPort = 161;

    /// <summary>
    ///     OID of sysName.
    /// </summary>
    public const string SysNameOID = "1.3.6.1.2.1.1.5.0";

    /// <summary>
    ///     OID of sysDescr.
    /// </summary>
    public const string SysDescrOID = "1.3.6.1.2.1.1.1.0";

    /// <summary>
    ///     OID of sysLocation.
    /// </summary>
    public const string SysLocationOID = "1.3.6.1.2.1.1.6.0";

    /// <summary>
    ///     Reads the system information of a host.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="version">SNMP version (<see cref="SNMPVersion.V1" /> or <see cref="SNMPVersion.V2C" />).</param>
    /// <param name="community">Community (e.g. "public").</param>
    /// <param name="timeout">Timeout in milliseconds to wait for the answer.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>System information, null if the host didn't answer or the answer is invalid.</returns>
    public static async Task<SNMPSystemInfo> ResolveAsync(IPAddress ipAddress, SNMPVersion version, string community,
        int timeout, CancellationToken cancellationToken)
    {
        try
        {
            var requestId = Messenger.NextRequestId;

            var request = CreateRequest(requestId, version, community);

            var response = await UdpRequest.SendAsync(ipAddress, SNMPPort, request.ToBytes(), timeout,
                cancellationToken).ConfigureAwait(false);

            return response == null ? null : ParseResponse(response, requestId);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     Creates the GET request for sysName, sysDescr and sysLocation.
    /// </summary>
    /// <param name="requestId">ID of the request (repeated in the answer).</param>
    /// <param name="version">SNMP version (<see cref="SNMPVersion.V1" /> or <see cref="SNMPVersion.V2C" />).</param>
    /// <param name="community">Community (e.g. "public").</param>
    /// <returns>SNMP message.</returns>
    public static GetRequestMessage CreateRequest(int requestId, SNMPVersion version, string community)
    {
        List<Variable> variables =
        [
            new(new ObjectIdentifier(SysNameOID)),
            new(new ObjectIdentifier(SysDescrOID)),
            new(new ObjectIdentifier(SysLocationOID))
        ];

        return new GetRequestMessage(requestId, version == SNMPVersion.V1 ? VersionCode.V1 : VersionCode.V2,
            new OctetString(community ?? string.Empty), variables);
    }

    /// <summary>
    ///     Gets the system information from the answer to <see cref="CreateRequest" />.
    /// </summary>
    /// <param name="response">SNMP message of the answer.</param>
    /// <param name="requestId">ID of the request.</param>
    /// <returns>System information, null if the answer is invalid, for another request or an error.</returns>
    public static SNMPSystemInfo ParseResponse(byte[] response, int requestId)
    {
        ISnmpMessage message;

        try
        {
            message = MessageFactory.ParseMessages(response, new UserRegistry()).FirstOrDefault();
        }
        catch (Exception)
        {
            return null;
        }

        if (message is not ResponseMessage || message.RequestId() != requestId ||
            message.Pdu().ErrorStatus.ToInt32() != 0)
            return null;

        var values = message.Variables().ToDictionary(x => x.Id.ToString(), x => GetValue(x.Data));

        return new SNMPSystemInfo(values.GetValueOrDefault(SysNameOID, string.Empty),
            values.GetValueOrDefault(SysDescrOID, string.Empty),
            values.GetValueOrDefault(SysLocationOID, string.Empty));
    }

    /// <summary>
    ///     Gets the value of a variable as text, empty for "no such object / instance" (v2c) and non-text values.
    /// </summary>
    private static string GetValue(ISnmpData data)
    {
        if (data is not OctetString octetString)
            return string.Empty;

        // Control characters (e.g. line breaks in sysDescr) are shown as spaces
        return new string([.. octetString.ToString().Select(x => char.IsControl(x) ? ' ' : x)]).Trim();
    }
}
