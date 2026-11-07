using System;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class WindowsHostInfoTests
{
    private static readonly NTLMChallengeInfo DomainChallenge = new(new Version(10, 0, 26100), "SRV01", "CORP",
        "srv01.corp.example.com", "corp.example.com", "example.com", null);

    private static readonly NTLMChallengeInfo WorkgroupChallenge =
        new(new Version(10, 0, 26200), "PC01", "PC01", "PC01", "PC01", string.Empty, null);

    private static SMBInfo CreateSMB(NTLMChallengeInfo ntlm, TimeSpan? offset = null)
    {
        return new SMBInfo("3.1.1", true, Guid.NewGuid(), DateTime.UtcNow, offset, ntlm);
    }

    private static WindowsNetApiInfo CreateNetApi(WindowsHostRole role, WindowsRegistryVersion registry = null,
        string workgroup = "WORKGROUP")
    {
        return new WindowsNetApiInfo(0, role, role == WindowsHostRole.DomainController, "File server", workgroup,
            ["CORP\\alice"], 0, registry);
    }

    [Theory]
    [InlineData("admin", "CORP", "admin", "CORP")]
    [InlineData(@"CORP\admin", "", "admin", "CORP")]
    [InlineData(@".\admin", "", "admin", ".")]
    [InlineData("admin@corp.example.com", "", "admin@corp.example.com", null)]
    [InlineData("admin", "", "admin", null)]
    [InlineData(@"CORP\admin", "CORP", "admin", "CORP")] // domain also in the username
    [InlineData(@"CORP\admin", "LAB", "admin", "CORP")] // the domain of the username wins
    [InlineData("admin@corp.example.com", "CORP", "admin@corp.example.com", null)] // UPN: no domain
    public void WindowsLogon_SplitUsername(string username, string domain, string expectedUsername,
        string expectedDomain)
    {
        Assert.Equal((expectedUsername, expectedDomain), WindowsLogon.SplitUsername(username, domain));
    }

    [Theory]
    [InlineData("10.0.0.1", "10.0.0.1")]
    [InlineData("client01.corp.example.com", "client01.corp.example.com")]
    [InlineData("fe80::1%4", "fe80--1s4.ipv6-literal.net")]
    [InlineData("2001:db8::1", "2001-db8--1.ipv6-literal.net")]
    public void NetApi_ToServerName(string host, string expected)
    {
        Assert.Equal(expected, WindowsNetApiResolver.ToServerName(host));
    }

    [Theory]
    [InlineData(0x00001003, WindowsHostRole.Workstation)] // workstation, server (file sharing), ...
    [InlineData(0x00009003, WindowsHostRole.Server)] // + SV_TYPE_SERVER_NT
    [InlineData(0x0000901B, WindowsHostRole.DomainController)] // + SV_TYPE_DOMAIN_CTRL
    [InlineData(0x00009013, WindowsHostRole.DomainController)] // + SV_TYPE_DOMAIN_BAKCTRL
    [InlineData(0, WindowsHostRole.Unknown)]
    public void NetApi_GetRole(int type, WindowsHostRole expected)
    {
        Assert.Equal(expected, WindowsNetApiResolver.GetRole(type));
    }

    [Fact]
    public void NetApi_FilterUsers()
    {
        var users = WindowsNetApiResolver.FilterUsers([
            ("bob", "CORP"), ("CLIENT01$", "CORP"), ("DWM-1", "Window Manager"), ("UMFD-0", "Font Driver Host"),
            ("alice", "CORP"), ("Bob", "corp"), ("localadmin", "CLIENT01"), ("", "CORP"), ("guest", "")
        ]);

        Assert.Equal([@"CLIENT01\localadmin", @"CORP\alice", @"CORP\bob", "guest"], users);
    }

    [Theory]
    [InlineData("Professional", "Pro")]
    [InlineData("Core", "Home")]
    [InlineData("Enterprise", "Enterprise")]
    [InlineData("EnterpriseS", "Enterprise LTSC")]
    [InlineData("ServerStandard", "Standard")]
    [InlineData("ServerDatacenterEval", "Datacenter (Evaluation)")]
    [InlineData("ServerAzureEdition", "Datacenter: Azure Edition")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void NetApi_GetEditionName(string editionID, string expected)
    {
        Assert.Equal(expected, WindowsNetApiResolver.GetEditionName(editionID));
    }

    [Fact]
    public void WithoutCredentials_NTLMOnly()
    {
        // Access denied for the NetApi: the values of the NTLM challenge are still shown
        var info = new WindowsHostInfo(CreateSMB(DomainChallenge, TimeSpan.FromMinutes(-3)),
            WindowsNetApiInfo.FromError(5));

        Assert.Equal("Windows 11 24H2 / Server 2025", info.OS);
        Assert.Equal("10.0.26100", info.Version.ToString());
        Assert.Equal(string.Empty, info.Edition);
        Assert.Equal("corp.example.com", info.Domain);
        Assert.Equal("srv01.corp.example.com", info.ComputerName);
        Assert.Equal(TimeSpan.FromMinutes(-3), info.TimeOffset);
        Assert.Equal(WindowsHostRole.Unknown, info.Role);
        Assert.Null(info.Users);
        Assert.Equal(5, info.Error);
        Assert.Equal(5, info.UsersError);
        Assert.False(WindowsHostInfo.IsLogonNotDecided(info.Error));
    }

    [Fact]
    public void Role_DecidesSharedBuild()
    {
        var info = new WindowsHostInfo(CreateSMB(DomainChallenge), CreateNetApi(WindowsHostRole.DomainController));

        Assert.Equal("Windows Server 2025", info.OS);
        Assert.True(info.IsServer);
    }

    [Fact]
    public void Registry_PatchLevelAndEdition()
    {
        var registry = new WindowsRegistryVersion(new Version(10, 0, 26200, 6584), "25H2", "Professional", false);

        var info = new WindowsHostInfo(CreateSMB(WorkgroupChallenge), CreateNetApi(WindowsHostRole.Workstation, registry));

        Assert.Equal("Windows 11 25H2", info.OS);
        Assert.Equal("Pro", info.Edition);
        Assert.Equal("10.0.26200.6584", info.Version.ToString());
        Assert.Equal([@"CORP\alice"], info.Users);
        Assert.Equal("File server", info.Description);
    }

    [Fact]
    public void Registry_NewerBuild_DisplayVersion()
    {
        var registry = new WindowsRegistryVersion(new Version(10, 0, 28000, 1), "26H2", "Enterprise", false);

        var info = new WindowsHostInfo(null, CreateNetApi(WindowsHostRole.Workstation, registry));

        Assert.Equal("Windows 11 26H2", info.OS);
    }

    [Fact]
    public void Registry_NewerServerBuild_KeepsTheBuild()
    {
        // The display version of a server doesn't name the release (no client-like "Windows 25H2")
        var registry = new WindowsRegistryVersion(new Version(10, 0, 28000, 1), "25H2", "ServerStandard", true);

        var info = new WindowsHostInfo(null, CreateNetApi(WindowsHostRole.Server, registry));

        Assert.Equal("Windows (build 28000)", info.OS);
    }

    [Fact]
    public void Workgroup_FromNetApi()
    {
        // A computer in a workgroup sends its own name as domain in the NTLM challenge
        Assert.Equal("WORKGROUP",
            new WindowsHostInfo(CreateSMB(WorkgroupChallenge), CreateNetApi(WindowsHostRole.Workstation)).Domain);
        Assert.Equal(string.Empty, new WindowsHostInfo(CreateSMB(WorkgroupChallenge), null).Domain);
        Assert.False(new WindowsHostInfo(CreateSMB(WorkgroupChallenge), null).IsDomainMember);
    }

    [Fact]
    public void Samba_NoName()
    {
        var samba = new NTLMChallengeInfo(new Version(6, 1, 0), "NAS", "NAS", "nas", string.Empty, string.Empty, null);

        var info = new WindowsHostInfo(CreateSMB(samba), CreateNetApi(WindowsHostRole.Server));

        Assert.Null(info.Version);
        Assert.Equal(string.Empty, info.OS);
        Assert.Equal("nas", info.ComputerName);
    }

    [Theory]
    [InlineData(WindowsHostInfo.ErrorTimeout, true)]
    [InlineData(53, true)] // network path not found
    [InlineData(1722, true)] // RPC server unavailable
    [InlineData(1326, false)] // wrong username or password
    [InlineData(1909, false)] // locked
    [InlineData(1265, false)] // unknown: may be a failed logon
    [InlineData(31, false)] // ERROR_GEN_FAILURE: a request that was not sent is not reported (see IPScanner)
    [InlineData(5, false)] // access denied
    public void IsLogonNotDecided(int error, bool expected)
    {
        Assert.Equal(expected, WindowsHostInfo.IsLogonNotDecided(error));
    }

    [Theory]
    [InlineData(1385, true)] // logon type not granted
    [InlineData(1328, true)] // logon hours
    [InlineData(1326, false)] // wrong username or password
    [InlineData(1909, false)] // locked
    [InlineData(0, false)]
    public void IsLogonNotAllowed(int error, bool expected)
    {
        Assert.Equal(expected, WindowsHostInfo.IsLogonNotAllowed(error));
    }

    [Fact]
    public void GetErrorMessage_NetApiError_Message()
    {
        // NERR_ServerNotStarted: from netmsg.dll on Windows, otherwise the message of the system - never empty
        Assert.False(string.IsNullOrEmpty(WindowsHostInfo.GetErrorMessage(2114)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(WindowsHostInfo.ErrorLogonSkipped)]
    [InlineData(WindowsHostInfo.ErrorLogonNotChecked)]
    public void GetErrorMessage_NoWin32Error_Empty(int error)
    {
        // Not tried: translated by the converter, no Windows message ("Unknown error (0xfffffffe)")
        Assert.Equal(string.Empty, WindowsHostInfo.GetErrorMessage(error));
    }
}
