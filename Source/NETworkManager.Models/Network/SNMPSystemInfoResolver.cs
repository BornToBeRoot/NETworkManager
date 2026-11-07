using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Reads the system information (sysName, sysDescr, sysLocation of the MIB-II system group) of a host with an SNMP
///     GET request to UDP port 161. v1 / v2c: a host with another community usually doesn't answer at all (timeout).
///     v3: a discovery request first (any SNMPv3 agent answers it with a report, also without valid credentials), then
///     the GET request with the user - wrong credentials are answered with a report (see
///     <see cref="SNMPSystemInfo.Error" />).
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
    /// <param name="credentials">Version and credentials.</param>
    /// <param name="timeout">Timeout in milliseconds to wait for an answer (v3: per request).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>System information, null if the host didn't answer or the answer is invalid.</returns>
    public static async Task<SNMPSystemInfo> ResolveAsync(IPAddress ipAddress, SNMPCredentials credentials,
        int timeout, CancellationToken cancellationToken)
    {
        if (credentials.Version == SNMPVersion.V3)
            return await ResolveV3Async(ipAddress, credentials, timeout, cancellationToken).ConfigureAwait(false);

        try
        {
            var requestId = Messenger.NextRequestId;

            var request = CreateRequest(requestId, credentials.Version, credentials.Community);

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
    ///     Reads the system information with SNMPv3: discovery (engine ID, boots and time of the agent), then the GET
    ///     request with the user.
    /// </summary>
    private static async Task<SNMPSystemInfo> ResolveV3Async(IPAddress ipAddress, SNMPCredentials credentials,
        int timeout, CancellationToken cancellationToken)
    {
        try
        {
            var discoveryMessageId = Messenger.NextMessageId;

            var discovery = new Discovery(discoveryMessageId, Messenger.NextRequestId, Messenger.MaxMessageSize);

            var discoveryResponse = await UdpRequest.SendAsync(ipAddress, SNMPPort, discovery.ToBytes(), timeout,
                cancellationToken).ConfigureAwait(false);

            if (discoveryResponse == null)
                return null;

            var report = ParseDiscoveryResponse(discoveryResponse, discoveryMessageId);

            // Not an SNMPv3 agent (e.g. only v1 / v2c, or another service on the port)
            if (report == null)
                return null;

            var privacy = SNMPV3Helper.CreatePrivacyProvider(credentials.Security,
                credentials.AuthenticationProvider, credentials.Auth, credentials.PrivacyProvider, credentials.Priv);

            var messageId = Messenger.NextMessageId;
            var requestId = Messenger.NextRequestId;

            var request = CreateRequestV3(messageId, requestId, credentials.Username, privacy, report);

            var response = await UdpRequest.SendAsync(ipAddress, SNMPPort, request.ToBytes(), timeout,
                cancellationToken).ConfigureAwait(false);

            // The agent answered the discovery (it is reachable), but not the request (e.g. a wrong privacy password
            // can't be decrypted - some agents don't report it)
            if (response == null)
                return new SNMPSystemInfo(string.Empty, string.Empty, string.Empty);

            return ParseResponseV3(response, messageId, requestId,
                new UserRegistry([new User(new OctetString(credentials.Username ?? string.Empty), privacy)])) ??
                   new SNMPSystemInfo(string.Empty, string.Empty, string.Empty);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     Gets the report of the answer to the discovery request (v3).
    /// </summary>
    /// <param name="response">SNMP message of the answer.</param>
    /// <param name="messageId">Message ID of the discovery request (repeated in the report).</param>
    /// <returns>Report (with the engine ID, boots and time of the agent), null if the answer is invalid.</returns>
    public static ReportMessage ParseDiscoveryResponse(byte[] response, int messageId)
    {
        try
        {
            return MessageFactory.ParseMessages(response, new UserRegistry()).FirstOrDefault() is ReportMessage report &&
                   report.MessageId() == messageId
                ? report
                : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     Creates the SNMPv3 GET request for sysName, sysDescr and sysLocation.
    /// </summary>
    /// <param name="messageId">Message ID (repeated in the answer and in a report).</param>
    /// <param name="requestId">Request ID (repeated in the answer).</param>
    /// <param name="username">Username.</param>
    /// <param name="privacy">Privacy provider of the credentials, see <see cref="SNMPV3Helper.CreatePrivacyProvider" />.</param>
    /// <param name="report">Report of the discovery (engine ID, boots and time of the agent).</param>
    /// <returns>SNMP message.</returns>
    public static GetRequestMessage CreateRequestV3(int messageId, int requestId, string username,
        IPrivacyProvider privacy, ISnmpMessage report)
    {
        return new GetRequestMessage(VersionCode.V3, messageId, requestId, new OctetString(username ?? string.Empty),
            OctetString.Empty, CreateVariables(), privacy, Messenger.MaxMessageSize, report);
    }

    /// <summary>
    ///     Gets the system information from the answer to <see cref="CreateRequestV3" />.
    /// </summary>
    /// <param name="response">SNMP message of the answer.</param>
    /// <param name="messageId">Message ID of the request.</param>
    /// <param name="requestId">Request ID of the request.</param>
    /// <param name="registry">User of the request (authentication and decryption of the answer).</param>
    /// <returns>
    ///     System information; of a report (e.g. unknown user, wrong password) empty values with the error. Null if the
    ///     answer is invalid or for another request.
    /// </returns>
    public static SNMPSystemInfo ParseResponseV3(byte[] response, int messageId, int requestId, UserRegistry registry)
    {
        ISnmpMessage message;

        try
        {
            message = MessageFactory.ParseMessages(response, registry).FirstOrDefault();
        }
        catch (Exception)
        {
            return null;
        }

        if (message == null || message.MessageId() != messageId)
            return null;

        // Wrong credentials: the agent answered with a report (usmStats), not with the values
        if (message is ReportMessage)
        {
            var variables = message.Pdu().Variables;

            return new SNMPSystemInfo(string.Empty, string.Empty, string.Empty,
                variables.Count > 0 && SNMPV3Helper.TryGetErrorCode(variables[0].Id, out var errorCode)
                    ? errorCode
                    : SNMPV3ErrorCode.None);
        }

        return message is ResponseMessage && message.RequestId() == requestId ? GetSystemInfo(message) : null;
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
        return new GetRequestMessage(requestId, version == SNMPVersion.V1 ? VersionCode.V1 : VersionCode.V2,
            new OctetString(community ?? string.Empty), CreateVariables());
    }

    /// <summary>
    ///     Variables of the request: sysName, sysDescr and sysLocation.
    /// </summary>
    private static List<Variable> CreateVariables()
    {
        return
        [
            new(new ObjectIdentifier(SysNameOID)),
            new(new ObjectIdentifier(SysDescrOID)),
            new(new ObjectIdentifier(SysLocationOID))
        ];
    }

    /// <summary>
    ///     Gets the system information from the answer to <see cref="CreateRequest" />.
    /// </summary>
    /// <param name="response">SNMP message of the answer.</param>
    /// <param name="requestId">ID of the request.</param>
    /// <returns>
    ///     System information (empty values if the host answered with an error, e.g. noSuchName of v1 - it is reachable),
    ///     null if the answer is invalid or for another request.
    /// </returns>
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

        if (message is not ResponseMessage || message.RequestId() != requestId)
            return null;

        return GetSystemInfo(message);
    }

    /// <summary>
    ///     Gets the system information of a response (empty values if the host answered with an error, e.g. noSuchName
    ///     of v1 - it is reachable).
    /// </summary>
    private static SNMPSystemInfo GetSystemInfo(ISnmpMessage message)
    {
        if (message.Pdu().ErrorStatus.ToInt32() != 0)
            return new SNMPSystemInfo(string.Empty, string.Empty, string.Empty);

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

        // Control characters (e.g. line breaks in sysDescr) are shown as spaces, several spaces as one
        return StringHelper.ReplaceControlCharactersWithSpace(octetString.ToString());
    }
}
