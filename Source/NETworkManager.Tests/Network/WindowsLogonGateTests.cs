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

    private static WindowsLogonGate CreateGate(string username = "admin", string domain = "CORP",
        bool isRejected = false)
    {
        return new WindowsLogonGate(
            new WindowsCredentials(username, domain, SecureStringHelper.ConvertToSecureString("password")),
            isRejected);
    }

    private static readonly WindowsNetApiInfo LogonFailure = WindowsNetApiInfo.FromError(1326);

    private static readonly WindowsNetApiInfo Success =
        new(0, WindowsHostRole.Workstation, true, string.Empty, "CORP", [], 0, null);

    [Fact]
    public async Task FirstHostChecks_OthersWait_ThenProceed()
    {
        var gate = CreateGate();

        Assert.Equal(WindowsLogonGate.Decision.Check, await gate.WaitAsync(CorpHost, Timeout, CancellationToken.None));

        // The next host waits for the result of the first one
        var second = gate.WaitAsync(CorpHost, Timeout, CancellationToken.None);
        Assert.False(second.IsCompleted);

        gate.Report(CorpHost, Success);

        Assert.Equal(WindowsLogonGate.Decision.Proceed, await second);
        Assert.Equal(WindowsLogonGate.Decision.Proceed, await gate.WaitAsync(CorpHost, Timeout, CancellationToken.None));
        Assert.False(gate.IsRejected);
    }

    [Fact]
    public async Task AccessDenied_CredentialsAccepted()
    {
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, Timeout, CancellationToken.None);
        gate.Report(CorpHost, WindowsNetApiInfo.FromError(5));

        Assert.Equal(WindowsLogonGate.Decision.Proceed, await gate.WaitAsync(CorpHost, Timeout, CancellationToken.None));
    }

    [Fact]
    public async Task SuccessInAnotherDomain_NextHostChecks_AcceptedAfterMinAccepts()
    {
        // E.g. a NAS that maps a failed logon to guest: its success doesn't prove the password
        var gate = CreateGate();

        for (var i = 0; i < WindowsLogonGate.MinAcceptsOutsideDomain; i++)
        {
            Assert.Equal(WindowsLogonGate.Decision.Check,
                await gate.WaitAsync(OtherDomainHost, Timeout, CancellationToken.None));

            gate.Report(OtherDomainHost, Success);
        }

        Assert.Equal(WindowsLogonGate.Decision.Proceed,
            await gate.WaitAsync(CorpHost, Timeout, CancellationToken.None));
    }

    [Fact]
    public async Task SuccessInAnotherDomain_ThenLogonFailureInTheDomain_Rejected()
    {
        var gate = CreateGate();

        await gate.WaitAsync(OtherDomainHost, Timeout, CancellationToken.None);
        gate.Report(OtherDomainHost, Success);

        Assert.Equal(WindowsLogonGate.Decision.Check, await gate.WaitAsync(CorpHost, Timeout, CancellationToken.None));
        gate.Report(CorpHost, LogonFailure);

        Assert.True(gate.IsRejected);
    }

    [Fact]
    public async Task LogonFailureInTheDomain_Rejected()
    {
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, Timeout, CancellationToken.None);
        var second = gate.WaitAsync(CorpHost, Timeout, CancellationToken.None);

        gate.Report(CorpHost, LogonFailure);

        Assert.Equal(WindowsLogonGate.Decision.Skip, await second);
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
                await gate.WaitAsync(OtherDomainHost, Timeout, CancellationToken.None));

            gate.Report(OtherDomainHost, LogonFailure);
        }

        Assert.True(gate.IsRejected);
    }

    [Fact]
    public async Task TimeoutOrCancellation_NotDecided_NextHostChecks()
    {
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, Timeout, CancellationToken.None);
        gate.Report(CorpHost, WindowsNetApiInfo.FromError(WindowsHostInfo.ErrorTimeout));

        await gate.WaitAsync(CorpHost, Timeout, CancellationToken.None);
        gate.Report(CorpHost, null);

        Assert.Equal(WindowsLogonGate.Decision.Check, await gate.WaitAsync(CorpHost, Timeout, CancellationToken.None));
        Assert.False(gate.IsRejected);
    }

    [Fact]
    public async Task WaitingLongerThanTheTimeout()
    {
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, Timeout, CancellationToken.None);

        Assert.Equal(WindowsLogonGate.Decision.Timeout,
            await gate.WaitAsync(CorpHost, TimeSpan.FromMilliseconds(10), CancellationToken.None));
    }

    [Fact]
    public async Task WorkgroupHost_NotGated()
    {
        // A computer in a workgroup can't check domain credentials (logon failure), no domain controller counts it
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, Timeout, CancellationToken.None);

        Assert.False(gate.IsGated(WorkgroupHost));
        Assert.Equal(WindowsLogonGate.Decision.Proceed,
            await gate.WaitAsync(WorkgroupHost, Timeout, CancellationToken.None));
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
    [InlineData("admin", "CORP", true)]
    [InlineData("admin", "corp.example.com", true)]
    [InlineData(@"CORP\admin", "", true)]
    [InlineData("admin@corp.example.com", "", true)]
    [InlineData("admin@example.com", "", false)] // other UPN suffix: not known as the domain
    [InlineData("admin", "LAB", false)]
    [InlineData("admin", "", false)]
    public void IsInDomain(string username, string domain, bool expected)
    {
        Assert.Equal(expected, CreateGate(username, domain).IsInDomain(CorpHost));
    }

    [Fact]
    public async Task RejectedInAnEarlierScan_Skip()
    {
        var gate = CreateGate(isRejected: true);

        Assert.Equal(WindowsLogonGate.Decision.Skip, await gate.WaitAsync(CorpHost, Timeout, CancellationToken.None));
        Assert.Equal(WindowsLogonGate.Decision.Proceed,
            await gate.WaitAsync(WorkgroupHost, Timeout, CancellationToken.None));
    }
}
