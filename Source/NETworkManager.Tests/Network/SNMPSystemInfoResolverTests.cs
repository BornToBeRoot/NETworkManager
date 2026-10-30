using System.Collections.Generic;
using System.Linq;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class SNMPSystemInfoResolverTests
{
    private const int RequestId = 4711;

    private static byte[] CreateResponse(int requestId, ErrorCode errorCode,
        params (string oid, ISnmpData data)[] values)
    {
        var variables = new List<Variable>();

        foreach (var (oid, data) in values)
            variables.Add(new Variable(new ObjectIdentifier(oid), data));

        return new ResponseMessage(requestId, VersionCode.V2, new OctetString("public"), errorCode, 0, variables)
            .ToBytes();
    }

    [Fact]
    public void CreateRequest()
    {
        var request = SNMPSystemInfoResolver.CreateRequest(RequestId, SNMPVersion.V1, "private");

        Assert.Equal(VersionCode.V1, request.Version);
        Assert.Equal("private", request.Community().ToString());
        Assert.Equal(RequestId, request.RequestId());
        Assert.Equal(
            [SNMPSystemInfoResolver.SysNameOID, SNMPSystemInfoResolver.SysDescrOID, SNMPSystemInfoResolver.SysLocationOID],
            request.Variables().Select(x => x.Id.ToString()));
    }

    [Fact]
    public void ParseResponse()
    {
        var response = CreateResponse(RequestId, ErrorCode.NoError,
            (SNMPSystemInfoResolver.SysNameOID, new OctetString("switch-01")),
            (SNMPSystemInfoResolver.SysDescrOID, new OctetString("Switch\r\nFirmware 1.2")),
            (SNMPSystemInfoResolver.SysLocationOID, new OctetString("Server room")));

        Assert.Equal(new SNMPSystemInfo("switch-01", "Switch  Firmware 1.2", "Server room"),
            SNMPSystemInfoResolver.ParseResponse(response, RequestId));
    }

    [Fact]
    public void ParseResponse_NoSuchObject_Empty()
    {
        var response = CreateResponse(RequestId, ErrorCode.NoError,
            (SNMPSystemInfoResolver.SysNameOID, new OctetString("printer")),
            (SNMPSystemInfoResolver.SysDescrOID, new NoSuchObject()),
            (SNMPSystemInfoResolver.SysLocationOID, new NoSuchInstance()));

        Assert.Equal(new SNMPSystemInfo("printer", string.Empty, string.Empty),
            SNMPSystemInfoResolver.ParseResponse(response, RequestId));
    }

    [Fact]
    public void ParseResponse_OtherRequest_Null()
    {
        var response = CreateResponse(RequestId + 1, ErrorCode.NoError,
            (SNMPSystemInfoResolver.SysNameOID, new OctetString("switch-01")));

        Assert.Null(SNMPSystemInfoResolver.ParseResponse(response, RequestId));
    }

    [Fact]
    public void ParseResponse_Error_Null()
    {
        var response = CreateResponse(RequestId, ErrorCode.NoSuchName,
            (SNMPSystemInfoResolver.SysNameOID, new Null()));

        Assert.Null(SNMPSystemInfoResolver.ParseResponse(response, RequestId));
    }

    [Theory]
    [InlineData(new byte[] { })]
    [InlineData(new byte[] { 0x30, 0x03, 0x02, 0x01 })]
    [InlineData(new byte[] { 0xFF, 0xFF, 0xFF })]
    public void ParseResponse_Invalid_Null(byte[] response)
    {
        Assert.Null(SNMPSystemInfoResolver.ParseResponse(response, RequestId));
    }
}
