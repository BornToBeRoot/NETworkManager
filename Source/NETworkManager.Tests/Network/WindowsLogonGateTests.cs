using System;
using System.Threading;
using System.Threading.Tasks;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using Xunit;

namespace NETworkManager.Tests.Network;

/// <summary>
///     Protection of the Windows credentials of a scan against an account lockout: checked on one host before the
///     others use them, rejected after a logon failure in their domain.
/// </summary>
public class WindowsLogonGateTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private static readonly NTLMChallengeInfo CorpHost = new(new Version(10, 0, 26200), "CLIENT01", "CORP",
        "client01.corp.example.com", "corp.example.com", "example.com", null);

    private static readonly NTLMChallengeInfo OtherDomainHost = new(new Version(10, 0, 26200), "SRV01", "LAB",
        "srv01.lab.example.com", "lab.example.com", "lab.example.com", null);

    private static readonly NTLMChallengeInfo WorkgroupHost =
        new(new Version(10, 0, 26200), "PC01", "PC01", "PC01", "PC01", string.Empty, null);

    private static WindowsLogonGate CreateGate(string username = "admin", string domain = "CORP")
    {
        return new WindowsLogonGate(
            new WindowsCredentials(username, domain, SecureStringHelper.ConvertToSecureString("password")));
    }

    private static readonly WindowsNetApiInfo LogonFailure = WindowsNetApiInfo.FromError(1326);

    private static readonly WindowsNetApiInfo Success =
        new(0, WindowsHostRole.Workstation, true, string.Empty, "CORP", [], 0, null);

    [Fact]
    public async Task FirstHostChecks_OthersWait_ThenProceed()
    {
        var gate = CreateGate();

        Assert.Equal(WindowsLogonGate.Decision.Check, await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));

        // The next host waits for the result of the first one
        var second = gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        Assert.False(second.IsCompleted);

        gate.Report(CorpHost, Success);

        Assert.Equal(WindowsLogonGate.Decision.Proceed, await second);
        Assert.Equal(WindowsLogonGate.Decision.Proceed, await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));
        Assert.False(gate.IsRejected);
    }

    [Fact]
    public async Task AccessDenied_CredentialsAccepted()
    {
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, WindowsNetApiInfo.FromError(5));

        Assert.Equal(WindowsLogonGate.Decision.Proceed, await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));
    }

    [Fact]
    public async Task SuccessesInAnotherDomain_NeverAccepted_NextHostChecks()
    {
        // E.g. a NAS that maps a failed logon to guest: its success doesn't prove the password - also not of several
        // hosts, or of the same host in several scans
        var gate = CreateGate();

        for (var scan = 0; scan < 5; scan++)
        {
            gate.StartScan();

            Assert.Equal(WindowsLogonGate.Decision.Check,
                await gate.WaitAsync(OtherDomainHost, "10.0.1.1", Timeout, CancellationToken.None));

            gate.Report(OtherDomainHost, Success);

            // The next host of the scan still checks them (one at a time)
            Assert.Equal(WindowsLogonGate.Decision.Check,
                await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));

            gate.Report(CorpHost, null);
        }
    }

    [Fact]
    public async Task WithoutDomain_SuccessesNeverAccepted()
    {
        // "admin" without domain: the local admin of some hosts (same password) doesn't prove the password of a
        // domain account "admin" that the domain controller checks on the hosts without this local account
        var gate = CreateGate("admin", "");

        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(WindowsLogonGate.Decision.Check,
                await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));

            gate.Report(CorpHost, Success);
        }

        Assert.Equal(WindowsLogonGate.Decision.Check,
            await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));
    }

    [Fact]
    public async Task SuccessInAnotherDomain_ThenLogonFailureInTheDomain_Rejected()
    {
        var gate = CreateGate();

        await gate.WaitAsync(OtherDomainHost, "10.0.1.1", Timeout, CancellationToken.None);
        gate.Report(OtherDomainHost, Success);

        Assert.Equal(WindowsLogonGate.Decision.Check, await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));
        gate.Report(CorpHost, LogonFailure);

        Assert.True(gate.IsRejected);
    }

    [Fact]
    public async Task LogonFailureInTheDomain_Rejected()
    {
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        var second = gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);

        gate.Report(CorpHost, LogonFailure);

        Assert.Equal(WindowsLogonGate.Decision.Skip, await second);
        Assert.True(gate.IsRejected);
    }

    [Fact]
    public async Task AcceptedInAnEarlierScan_CheckedAgain()
    {
        // E.g. the password was changed in the domain since the last scan of the tab
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, Success);
        Assert.Equal(WindowsLogonGate.Decision.Proceed, await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));

        gate.StartScan();

        Assert.Equal(WindowsLogonGate.Decision.Check, await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));
        gate.Report(CorpHost, LogonFailure);

        Assert.True(gate.IsRejected);
    }

    [Fact]
    public async Task RejectedInAnEarlierScan_StaysRejected()
    {
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, LogonFailure);

        gate.StartScan();

        Assert.Equal(WindowsLogonGate.Decision.Skip, await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));
    }

    [Fact]
    public async Task UngatedHost_SkippedAfterItsOwnLogonFailure()
    {
        // Not gated (workgroup): its own failure only skips this host in the next scans
        var gate = CreateGate();

        Assert.Equal(WindowsLogonGate.Decision.Proceed,
            await gate.WaitAsync(WorkgroupHost, "10.0.2.1", Timeout, CancellationToken.None));
        gate.ReportUngated("10.0.2.1", LogonFailure);

        Assert.Equal(WindowsLogonGate.Decision.Skip,
            await gate.WaitAsync(WorkgroupHost, "10.0.2.1", Timeout, CancellationToken.None));
        Assert.Equal(WindowsLogonGate.Decision.Proceed,
            await gate.WaitAsync(WorkgroupHost, "10.0.2.2", Timeout, CancellationToken.None));
        Assert.False(gate.IsRejected);
    }

    [Fact]
    public async Task LogonFailuresOfEarlierScansCount()
    {
        // Same gate in the next scan (e.g. Automatically scan every): 2 failures per scan don't stay below the limit
        var gate = CreateGate();

        for (var scan = 0; scan < 2; scan++)
        for (var i = 0; i < 2 && !gate.IsRejected; i++)
        {
            await gate.WaitAsync(OtherDomainHost, "10.0.1.1", Timeout, CancellationToken.None);
            gate.Report(OtherDomainHost, LogonFailure);
        }

        Assert.True(gate.IsRejected);
    }

    [Fact]
    public async Task LogonFailureInAnotherDomain_NextHostChecks_RejectedAfterMaxFailures()
    {
        var gate = CreateGate();

        for (var i = 0; i < WindowsLogonGate.MaxLogonFailures; i++)
        {
            Assert.False(gate.IsRejected);
            Assert.Equal(WindowsLogonGate.Decision.Check,
                await gate.WaitAsync(OtherDomainHost, "10.0.1.1", Timeout, CancellationToken.None));

            gate.Report(OtherDomainHost, LogonFailure);
        }

        Assert.True(gate.IsRejected);
    }

    [Fact]
    public async Task LogonFailureWithoutDomain_Rejected()
    {
        // "admin" without domain: the domain controller of the host may have counted it
        var gate = CreateGate("admin", "");

        await gate.WaitAsync(OtherDomainHost, "10.0.1.1", Timeout, CancellationToken.None);
        gate.Report(OtherDomainHost, LogonFailure);

        Assert.True(gate.IsRejected);
    }

    [Fact]
    public async Task TimeoutOrCancellation_NotDecided_NextHostChecks()
    {
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, WindowsNetApiInfo.FromError(WindowsHostInfo.ErrorTimeout));

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, null);

        Assert.Equal(WindowsLogonGate.Decision.Check, await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));
        Assert.False(gate.IsRejected);
    }

    [Theory]
    [InlineData(1793)] // account expired
    [InlineData(86)] // ERROR_INVALID_PASSWORD (STATUS_WRONG_PASSWORD)
    [InlineData(1244)] // not authenticated
    [InlineData(unchecked((int)0x8009030C))] // SEC_E_LOGON_DENIED
    [InlineData(1265)] // any other error: not known as "no logon"
    public async Task UnknownErrorInTheDomain_CountsAsLogonFailure(int error)
    {
        // A wrong password reported with an error that is not in the list of logon errors (e.g. Kerberos / SSPI) must
        // not let every host try it
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, WindowsNetApiInfo.FromError(error));

        Assert.True(gate.IsRejected);
    }

    [Theory]
    [InlineData(53)] // network path not found
    [InlineData(1231)] // network unreachable
    [InlineData(1722)] // RPC server unavailable
    [InlineData(1311)] // no logon servers (domain controller of the host unreachable)
    [InlineData(1937)] // NTLM blocked
    [InlineData(WindowsHostInfo.ErrorTimeout)]
    public async Task NoLogon_NotDecided_NextHostChecks(int error)
    {
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, WindowsNetApiInfo.FromError(error));

        Assert.False(gate.IsRejected);
        Assert.Equal(WindowsLogonGate.Decision.Check,
            await gate.WaitAsync(CorpHost, "10.0.0.2", Timeout, CancellationToken.None));
    }

    [Fact]
    public async Task UngatedHost_UnknownError_Skipped()
    {
        var gate = CreateGate();

        await gate.WaitAsync(WorkgroupHost, "10.0.2.1", Timeout, CancellationToken.None);
        gate.ReportUngated("10.0.2.1", WindowsNetApiInfo.FromError(1793));

        Assert.Equal(WindowsLogonGate.Decision.Skip,
            await gate.WaitAsync(WorkgroupHost, "10.0.2.1", Timeout, CancellationToken.None));
    }

    [Fact]
    public async Task WaitingLongerThanTheTimeout()
    {
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);

        Assert.Equal(WindowsLogonGate.Decision.Timeout,
            await gate.WaitAsync(CorpHost, "10.0.0.1", TimeSpan.FromMilliseconds(10), CancellationToken.None));
    }

    [Fact]
    public async Task WorkgroupHost_NotGated()
    {
        // A computer in a workgroup can't check domain credentials (logon failure), no domain controller counts it
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);

        Assert.False(gate.IsGated(WorkgroupHost));
        Assert.Equal(WindowsLogonGate.Decision.Proceed,
            await gate.WaitAsync(WorkgroupHost, "10.0.2.1", Timeout, CancellationToken.None));
    }

    [Theory]
    [InlineData("admin", ".")]
    [InlineData(@".\admin", "")]
    public void LocalAccount_NotGated(string username, string domain)
    {
        var gate = CreateGate(username, domain);

        Assert.False(gate.IsGated(CorpHost));
        Assert.False(gate.IsGated(null));
    }

    [Fact]
    public void LocalAccountOfTheHost_NotGated()
    {
        Assert.False(CreateGate("admin", "CLIENT01").IsGated(CorpHost));
    }

    [Fact]
    public void WithoutChallenge_Gated()
    {
        // Not known if the host is in a domain
        Assert.True(CreateGate().IsGated(null));
    }

    [Theory]
    [InlineData("admin", "CORP", "", "")]
    [InlineData("admin", "", "", "")]
    [InlineData(@"CORP\admin", "", "", "")]
    [InlineData("admin", "CORP", "NAS01", "")]
    [InlineData("admin", "", "", "CORP")]
    public void ChallengeWithoutNames_Gated(string username, string domain, string computerName, string domainName)
    {
        // E.g. a challenge without (or with a truncated) target info: not known if the host is in a domain - the empty
        // names are neither a workgroup computer nor the computer of a local account
        var ntlm = new NTLMChallengeInfo(new Version(10, 0, 26200), computerName, domainName, string.Empty,
            string.Empty, string.Empty, null);

        Assert.True(CreateGate(username, domain).IsGated(ntlm));
    }

    [Theory]
    [InlineData("admin", "CORP", true)]
    [InlineData("admin", "corp.example.com", true)]
    [InlineData(@"CORP\admin", "", true)]
    [InlineData("admin@corp.example.com", "", true)]
    [InlineData("admin@example.com", "", true)] // UPN suffix of the forest (root domain)
    [InlineData("admin", "example.com", true)]
    [InlineData("admin@contoso.com", "", false)] // other UPN suffix: not known as the domain
    [InlineData("admin", "LAB", false)]
    [InlineData("admin", "", false)]
    public void IsInDomain(string username, string domain, bool expected)
    {
        Assert.Equal(expected, CreateGate(username, domain).IsInDomain(CorpHost));
    }

    [Fact]
    public async Task RejectedInAnEarlierScan_Skip()
    {
        // The tab keeps the gate for all its scans
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, LogonFailure);

        Assert.Equal(WindowsLogonGate.Decision.Skip, await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));

        // Not for a host that is not gated (e.g. the local admin of a workgroup computer)
        Assert.Equal(WindowsLogonGate.Decision.Proceed,
            await gate.WaitAsync(WorkgroupHost, "10.0.2.1", Timeout, CancellationToken.None));
    }
}
