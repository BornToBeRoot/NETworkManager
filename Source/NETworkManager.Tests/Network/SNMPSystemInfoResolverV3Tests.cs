using System.Collections.Generic;
using System.Linq;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

/// <summary>
///     SNMPv3 of <see cref="SNMPSystemInfoResolver" />: discovery, request, answer and report (wrong credentials).
/// </summary>
public class SNMPSystemInfoResolverV3Tests
{
    private const int MessageId = 1234;
    private const int RequestId = 4711;

    private const string UnknownUserNameOID = "1.3.6.1.6.3.15.1.1.3.0";
    private const string WrongDigestOID = "1.3.6.1.6.3.15.1.1.5.0";
    private const string UnknownEngineIdOID = "1.3.6.1.6.3.15.1.1.4.0";

    private static readonly OctetString EngineId = new(new byte[] { 0x80, 0x00, 0x1F, 0x88, 0x80, 0x01, 0x02, 0x03 });

    private static ReportMessage CreateReport(int messageId, string oid, string username = "")
    {
        return new ReportMessage(VersionCode.V3,
            new Header(new Integer32(messageId), new Integer32(Messenger.MaxMessageSize), default(Levels)),
            new SecurityParameters(EngineId, new Integer32(1), new Integer32(100), new OctetString(username),
                OctetString.Empty, OctetString.Empty),
            new Scope(EngineId, OctetString.Empty,
                new ReportPdu(RequestId, ErrorCode.NoError, 0,
                    new List<Variable> { new(new ObjectIdentifier(oid), new Counter32(1)) })),
            DefaultPrivacyProvider.DefaultPair, null);
    }

    private static byte[] CreateResponse(int messageId, int requestId, string username,
        params (string oid, ISnmpData data)[] values)
    {
        var variables = values.Select(x => new Variable(new ObjectIdentifier(x.oid), x.data)).ToList();

        return new ResponseMessage(VersionCode.V3,
                new Header(new Integer32(messageId), new Integer32(Messenger.MaxMessageSize), default(Levels)),
                new SecurityParameters(EngineId, new Integer32(1), new Integer32(100), new OctetString(username),
                    OctetString.Empty, OctetString.Empty),
                new Scope(EngineId, OctetString.Empty, new ResponsePdu(requestId, ErrorCode.NoError, 0, variables)),
                DefaultPrivacyProvider.DefaultPair, false, null)
            .ToBytes();
    }

    private static UserRegistry CreateRegistry(string username)
    {
        return new UserRegistry([new User(new OctetString(username), DefaultPrivacyProvider.DefaultPair)]);
    }

    [Fact]
    public void ParseDiscoveryResponse()
    {
        var report = SNMPSystemInfoResolver.ParseDiscoveryResponse(
            CreateReport(MessageId, UnknownEngineIdOID).ToBytes(), MessageId);

        Assert.NotNull(report);
        Assert.Equal(EngineId, report.Parameters.EngineId);
    }

    [Fact]
    public void ParseDiscoveryResponse_OtherMessage_Null()
    {
        Assert.Null(SNMPSystemInfoResolver.ParseDiscoveryResponse(
            CreateReport(MessageId + 1, UnknownEngineIdOID).ToBytes(), MessageId));
    }

    [Fact]
    public void ParseDiscoveryResponse_NoReport_Null()
    {
        // An answer of v1 / v2c (e.g. another service on the port) is not a report
        var response = new ResponseMessage(RequestId, VersionCode.V2, new OctetString("public"), ErrorCode.NoError, 0,
            []).ToBytes();

        Assert.Null(SNMPSystemInfoResolver.ParseDiscoveryResponse(response, MessageId));
    }

    [Theory]
    [InlineData(SNMPV3Security.NoAuthNoPriv)]
    [InlineData(SNMPV3Security.AuthNoPriv)]
    [InlineData(SNMPV3Security.AuthPriv)]
    public void CreateRequestV3(SNMPV3Security security)
    {
        var privacy = SNMPV3Helper.CreatePrivacyProvider(security, SNMPV3AuthenticationProvider.SHA256,
            "authpassword", SNMPV3PrivacyProvider.AES, "privpassword");

        var request = SNMPSystemInfoResolver.CreateRequestV3(MessageId, RequestId, "monitor", privacy,
            CreateReport(MessageId, UnknownEngineIdOID));

        Assert.Equal(VersionCode.V3, request.Version);
        Assert.Equal(MessageId, request.MessageId());
        Assert.Equal("monitor", request.Parameters.UserName.ToString());

        // The engine of the agent from the discovery
        Assert.Equal(EngineId, request.Parameters.EngineId);

        // Encrypted, the variables can only be checked without privacy
        if (security != SNMPV3Security.AuthPriv)
            Assert.Equal(
                [SNMPSystemInfoResolver.SysNameOID, SNMPSystemInfoResolver.SysDescrOID, SNMPSystemInfoResolver.SysLocationOID],
                request.Variables().Select(x => x.Id.ToString()));

        // Can be sent
        Assert.NotEmpty(request.ToBytes());
    }

    [Fact]
    public void ParseResponseV3()
    {
        var response = CreateResponse(MessageId, RequestId, "monitor",
            (SNMPSystemInfoResolver.SysNameOID, new OctetString("switch-01")),
            (SNMPSystemInfoResolver.SysDescrOID, new OctetString("Switch")),
            (SNMPSystemInfoResolver.SysLocationOID, new OctetString("Server room")));

        Assert.Equal(new SNMPSystemInfo("switch-01", "Switch", "Server room"),
            SNMPSystemInfoResolver.ParseResponseV3(response, MessageId, RequestId, CreateRegistry("monitor")));
    }

    [Fact]
    public void ParseResponseV3_OtherMessage_Null()
    {
        var response = CreateResponse(MessageId + 1, RequestId, "monitor",
            (SNMPSystemInfoResolver.SysNameOID, new OctetString("switch-01")));

        Assert.Null(SNMPSystemInfoResolver.ParseResponseV3(response, MessageId, RequestId, CreateRegistry("monitor")));
    }

    [Theory]
    [InlineData(UnknownUserNameOID, SNMPV3ErrorCode.UnknownUserName)]
    [InlineData(WrongDigestOID, SNMPV3ErrorCode.AuthenticationFailed)]
    [InlineData(UnknownEngineIdOID, SNMPV3ErrorCode.None)]
    public void ParseResponseV3_Report_Error(string oid, SNMPV3ErrorCode expected)
    {
        // Wrong credentials: the host answered (it is reachable), empty values with the error
        var report = CreateReport(MessageId, oid, "monitor").ToBytes();

        Assert.Equal(new SNMPSystemInfo(string.Empty, string.Empty, string.Empty, expected),
            SNMPSystemInfoResolver.ParseResponseV3(report, MessageId, RequestId, CreateRegistry("monitor")));
    }

    [Theory]
    [InlineData(new byte[] { })]
    [InlineData(new byte[] { 0x30, 0x03, 0x02, 0x01 })]
    public void ParseResponseV3_Invalid_Null(byte[] response)
    {
        Assert.Null(SNMPSystemInfoResolver.ParseResponseV3(response, MessageId, RequestId, CreateRegistry("monitor")));
        Assert.Null(SNMPSystemInfoResolver.ParseDiscoveryResponse(response, MessageId));
    }

    [Theory]
    [InlineData(SNMPV3Security.NoAuthNoPriv, typeof(DefaultPrivacyProvider))]
    [InlineData(SNMPV3Security.AuthNoPriv, typeof(DefaultPrivacyProvider))]
    [InlineData(SNMPV3Security.AuthPriv, typeof(AESPrivacyProvider))]
    public void CreatePrivacyProvider(SNMPV3Security security, System.Type expected)
    {
        var privacy = SNMPV3Helper.CreatePrivacyProvider(security, SNMPV3AuthenticationProvider.SHA1, "authpassword",
            SNMPV3PrivacyProvider.AES, "privpassword");

        Assert.IsType(expected, privacy);
        Assert.Equal(security == SNMPV3Security.NoAuthNoPriv,
            privacy.AuthenticationProvider is DefaultAuthenticationProvider);
    }

    [Fact]
    public void Credentials_ToString_WithoutSecrets()
    {
        var credentials = new SNMPCredentials(SNMPVersion.V3)
        {
            Username = "monitor",
            Auth = "secret-auth",
            Priv = "secret-priv",
            Community = "secret-community"
        };

        var text = credentials.ToString();

        Assert.Contains("monitor", text);
        Assert.DoesNotContain("secret", text);
        Assert.DoesNotContain("secret", SNMPCredentials.FromCommunity(SNMPVersion.V2C, "secret-community").ToString());
    }
}
