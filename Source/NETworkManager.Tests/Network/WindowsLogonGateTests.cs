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

    private const int MaxLogonFailures = 2;

    /// <summary>
    ///     Creates the gate and starts a scan. Without a username: a new account (the logon failures are counted per
    ///     account for all gates of the application, also of other tests).
    /// </summary>
    private static WindowsLogonGate CreateGate(string username = null, string domain = "CORP",
        string password = "password", bool isEnteredByUser = false)
    {
        var gate = new WindowsLogonGate(new WindowsCredentials(username ?? $"admin-{Guid.NewGuid():N}", domain,
            SecureStringHelper.ConvertToSecureString(password)), isEnteredByUser);

        gate.StartScan(MaxLogonFailures);

        return gate;
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

        gate.Report(CorpHost, "10.0.0.1", Success);

        Assert.Equal(WindowsLogonGate.Decision.Proceed, await second);
        Assert.Equal(WindowsLogonGate.Decision.Proceed, await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));
        Assert.False(gate.IsRejected);
    }

    [Theory]
    [InlineData(6, 1, 0)] // Samba
    [InlineData(null, null, null)] // no version (e.g. a NAS)
    public async Task SuccessOnNonWindowsHostInTheDomain_NotAccepted(int? major, int? minor, int? build)
    {
        // E.g. a Samba / NAS member of the domain with "map to guest = bad password": a wrong password is a guest
        // logon - only a Windows host proves the password
        var samba = new NTLMChallengeInfo(major == null ? null : new Version(major.Value, minor.Value, build.Value),
            "NAS01", "CORP", "nas01.corp.example.com", "corp.example.com", "example.com", null);

        var gate = CreateGate();

        await gate.WaitAsync(samba, "10.0.0.9", Timeout, CancellationToken.None);
        gate.Report(samba, "10.0.0.9", Success);

        Assert.Equal(WindowsLogonGate.Decision.Check,
            await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));

        // A logon failure on such a host still rejects them (the domain controller may have counted it)
        gate.Report(CorpHost, "10.0.0.1", null);
        await gate.WaitAsync(samba, "10.0.0.9", Timeout, CancellationToken.None);
        gate.Report(samba, "10.0.0.9", LogonFailure);

        Assert.True(gate.IsRejected);
    }

    [Fact]
    public async Task AccessDenied_CredentialsAccepted()
    {
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, "10.0.0.1", WindowsNetApiInfo.FromError(5));

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
            gate.StartScan(MaxLogonFailures);

            Assert.Equal(WindowsLogonGate.Decision.Check,
                await gate.WaitAsync(OtherDomainHost, "10.0.1.1", Timeout, CancellationToken.None));

            gate.Report(OtherDomainHost, "10.0.0.1", Success);

            // The next host of the scan still checks them (one at a time)
            Assert.Equal(WindowsLogonGate.Decision.Check,
                await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));

            gate.Report(CorpHost, "10.0.0.1", null);
        }
    }

    [Fact]
    public async Task WithoutDomain_SuccessesNeverAccepted()
    {
        // "admin" without domain: the local admin of some hosts (same password) doesn't prove the password of a
        // domain account "admin" that the domain controller checks on the hosts without this local account
        var gate = CreateGate($"admin-{Guid.NewGuid():N}", "");

        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(WindowsLogonGate.Decision.Check,
                await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));

            gate.Report(CorpHost, "10.0.0.1", Success);
        }

        Assert.Equal(WindowsLogonGate.Decision.Check,
            await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));
    }

    [Fact]
    public async Task SuccessInAnotherDomain_ThenLogonFailureInTheDomain_Rejected()
    {
        var gate = CreateGate();

        await gate.WaitAsync(OtherDomainHost, "10.0.1.1", Timeout, CancellationToken.None);
        gate.Report(OtherDomainHost, "10.0.0.1", Success);

        Assert.Equal(WindowsLogonGate.Decision.Check, await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));
        gate.Report(CorpHost, "10.0.0.1", LogonFailure);

        Assert.True(gate.IsRejected);
    }

    [Fact]
    public async Task LogonFailureInTheDomain_Rejected()
    {
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        var second = gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);

        gate.Report(CorpHost, "10.0.0.1", LogonFailure);

        Assert.Equal(WindowsLogonGate.Decision.Skip, await second);
        Assert.True(gate.IsRejected);
    }

    [Fact]
    public async Task AcceptedInAnEarlierScan_CheckedAgain()
    {
        // E.g. the password was changed in the domain since the last scan of the tab
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, "10.0.0.1", Success);
        Assert.Equal(WindowsLogonGate.Decision.Proceed, await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));

        gate.StartScan(MaxLogonFailures);

        Assert.Equal(WindowsLogonGate.Decision.Check, await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));
        gate.Report(CorpHost, "10.0.0.1", LogonFailure);

        Assert.True(gate.IsRejected);
    }

    [Fact]
    public async Task RejectedInAnEarlierScan_StaysRejected()
    {
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, "10.0.0.1", LogonFailure);

        gate.StartScan(MaxLogonFailures);

        Assert.Equal(WindowsLogonGate.Decision.Skip, await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));
    }

    [Fact]
    public async Task UngatedHost_SkippedAfterItsOwnLogonFailure()
    {
        // Not gated (workgroup): its own failure only skips this host in the next scans
        var gate = CreateGate();

        Assert.Equal(WindowsLogonGate.Decision.Proceed,
            await gate.WaitAsync(WorkgroupHost, "10.0.2.1", Timeout, CancellationToken.None));
        gate.ReportUngated(WorkgroupHost, "10.0.2.1", LogonFailure);

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
            gate.Report(OtherDomainHost, "10.0.0.1", LogonFailure);
        }

        Assert.True(gate.IsRejected);
    }

    [Fact]
    public async Task LogonFailureInAnotherDomain_NextHostChecks_RejectedAfterMaxFailures()
    {
        var gate = CreateGate();

        for (var i = 0; i < MaxLogonFailures; i++)
        {
            Assert.False(gate.IsRejected);
            Assert.Equal(WindowsLogonGate.Decision.Check,
                await gate.WaitAsync(OtherDomainHost, "10.0.1.1", Timeout, CancellationToken.None));

            gate.Report(OtherDomainHost, "10.0.0.1", LogonFailure);
        }

        Assert.True(gate.IsRejected);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public async Task MaxLogonFailuresOfTheScan(int maxLogonFailures)
    {
        var gate = CreateGate();
        gate.StartScan(maxLogonFailures);

        for (var i = 0; i < maxLogonFailures; i++)
        {
            Assert.False(gate.IsRejected);

            await gate.WaitAsync(OtherDomainHost, "10.0.1.1", Timeout, CancellationToken.None);
            gate.Report(OtherDomainHost, "10.0.0.1", LogonFailure);
        }

        Assert.True(gate.IsRejected);
    }

    [Fact]
    public async Task LogonFailuresCountedForAllGatesOfTheAccount()
    {
        // E.g. another tab (Scan as..., the same profile) or the credentials entered again: the domain controller
        // counts the failures of the account, not of the tab
        var username = $"admin-{Guid.NewGuid():N}";

        var first = CreateGate(username);
        await first.WaitAsync(OtherDomainHost, "10.0.1.1", Timeout, CancellationToken.None);
        first.Report(OtherDomainHost, "10.0.0.1", LogonFailure);
        Assert.False(first.IsRejected);

        var second = CreateGate(username);
        await second.WaitAsync(OtherDomainHost, "10.0.1.1", Timeout, CancellationToken.None);
        second.Report(OtherDomainHost, "10.0.0.1", LogonFailure);
        Assert.True(second.IsRejected);

        // Entered again by the user (e.g. after the account was unlocked): checked once, rejected by its first failure
        var third = CreateGate(username, isEnteredByUser: true);
        Assert.Equal(WindowsLogonGate.Decision.Check,
            await third.WaitAsync(OtherDomainHost, "10.0.1.1", Timeout, CancellationToken.None));
        third.Report(OtherDomainHost, "10.0.0.1", LogonFailure);
        Assert.True(third.IsRejected);
    }

    [Fact]
    public async Task Rejected_RaisedOnce()
    {
        // Also after the scan (a check that exceeded the timeout reports later), so the tab can show the message
        var gate = CreateGate();
        var raised = 0;
        gate.Rejected += (_, _) => raised++;

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, "10.0.0.1", Success with { Error = WindowsHostInfo.ErrorTimeout });
        Assert.Equal(0, raised);

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, "10.0.0.1", LogonFailure);
        Assert.Equal(1, raised);

        // The next scans skip them without raising it again
        gate.StartScan(MaxLogonFailures);
        Assert.Equal(WindowsLogonGate.Decision.Skip,
            await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));
        Assert.Equal(1, raised);
    }

    [Fact]
    public async Task OneHostAtATimeForAllGatesOfTheAccount()
    {
        // E.g. several tabs of profiles of the same group: one check at a time, not one per tab
        var username = $"admin-{Guid.NewGuid():N}";

        var first = CreateGate(username);
        var second = CreateGate(username);

        Assert.Equal(WindowsLogonGate.Decision.Check,
            await first.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));

        var waiting = second.WaitAsync(CorpHost, "10.0.0.2", Timeout, CancellationToken.None);
        Assert.False(waiting.IsCompleted);

        // Rejected by the first tab: the second one doesn't try the same password
        first.Report(CorpHost, "10.0.0.1", LogonFailure);

        Assert.Equal(WindowsLogonGate.Decision.Skip, await waiting);
        Assert.True(second.IsRejected);
    }

    [Fact]
    public async Task RejectedPassword_NewGateStartsRejected()
    {
        // E.g. another tab of the same profile after the rejection
        var username = $"admin-{Guid.NewGuid():N}";

        var first = CreateGate(username);
        await first.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        first.Report(CorpHost, "10.0.0.1", LogonFailure);

        var second = CreateGate(username);
        Assert.True(second.IsRejected);
        Assert.Equal(WindowsLogonGate.Decision.Skip,
            await second.WaitAsync(CorpHost, "10.0.0.2", Timeout, CancellationToken.None));
    }

    [Fact]
    public async Task RejectedPassword_OtherPasswordOrEnteredByUser_CheckedAgain()
    {
        var username = $"admin-{Guid.NewGuid():N}";

        var first = CreateGate(username);
        await first.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        first.Report(CorpHost, "10.0.0.1", LogonFailure);

        // E.g. the password was corrected in the profile
        Assert.False(CreateGate(username, password: "corrected").IsRejected);

        // Entered by the user (also unchanged, e.g. after the account was unlocked): checked again, also by the next
        // gates (the rejection of the password is removed)
        var entered = CreateGate(username, isEnteredByUser: true);
        Assert.False(entered.IsRejected);
        Assert.Equal(WindowsLogonGate.Decision.Check,
            await entered.WaitAsync(CorpHost, "10.0.0.2", Timeout, CancellationToken.None));
        entered.Report(CorpHost, "10.0.0.2", Success);
    }

    [Fact]
    public async Task UpnAndDnsDomain_SameAccount()
    {
        var username = $"admin-{Guid.NewGuid():N}";

        var upn = CreateGate($"{username}@corp.example.com", "");
        await upn.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        upn.Report(CorpHost, "10.0.0.1", LogonFailure);

        Assert.True(CreateGate(username, "corp.example.com").IsRejected);
        Assert.True(CreateGate($@"corp.example.com\{username}", "").IsRejected);
    }

    [Fact]
    public async Task LogonFailureWithoutDomain_Rejected()
    {
        // "admin" without domain: the domain controller of the host may have counted it
        var gate = CreateGate($"admin-{Guid.NewGuid():N}", "");

        await gate.WaitAsync(OtherDomainHost, "10.0.1.1", Timeout, CancellationToken.None);
        gate.Report(OtherDomainHost, "10.0.0.1", LogonFailure);

        Assert.True(gate.IsRejected);
    }

    [Fact]
    public async Task TimeoutOrCancellation_NotDecided_NextHostChecks()
    {
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, "10.0.0.1", WindowsNetApiInfo.FromError(WindowsHostInfo.ErrorTimeout));

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, "10.0.0.1", null);

        Assert.Equal(WindowsLogonGate.Decision.Check, await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));
        Assert.False(gate.IsRejected);
    }

    [Theory]
    [InlineData(1793)] // account expired
    [InlineData(86)] // ERROR_INVALID_PASSWORD (STATUS_WRONG_PASSWORD)
    [InlineData(1244)] // not authenticated
    [InlineData(unchecked((int)0x8009030C))] // SEC_E_LOGON_DENIED
    [InlineData(1265)] // any other error: not known as "no logon"
    [InlineData(31)] // ERROR_GEN_FAILURE of the host (e.g. NT_STATUS_UNSUCCESSFUL of Samba)
    public async Task UnknownErrorInTheDomain_CountsAsLogonFailure(int error)
    {
        // A wrong password reported with an error that is not in the list of logon errors (e.g. Kerberos / SSPI) must
        // not let every host try it
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, "10.0.0.1", WindowsNetApiInfo.FromError(error));

        Assert.True(gate.IsRejected);
    }

    [Fact]
    public async Task LogonTypeNotGrantedInTheDomain_PasswordCorrect_Accepted()
    {
        // E.g. "Deny access to this computer from the network" on a domain controller: checked by the host after the
        // domain controller accepted the password
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, "10.0.0.1", WindowsNetApiInfo.FromError(1385));

        Assert.False(gate.IsRejected);
        Assert.Equal(WindowsLogonGate.Decision.Proceed,
            await gate.WaitAsync(CorpHost, "10.0.0.2", Timeout, CancellationToken.None));
    }

    [Theory]
    [InlineData(1327)] // account restriction (e.g. Protected Users with NTLM)
    [InlineData(1328)] // logon hours
    [InlineData(1329)] // workstation not allowed
    public async Task LogonNotAllowedInTheDomain_NotRejected_NextHostChecks(int error)
    {
        // No failed logon at the domain controller, but the password is not proven (the restriction may be checked
        // before it)
        var gate = CreateGate();

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, "10.0.0.1", WindowsNetApiInfo.FromError(error));

        Assert.False(gate.IsRejected);
        Assert.Equal(WindowsLogonGate.Decision.Check,
            await gate.WaitAsync(CorpHost, "10.0.0.2", Timeout, CancellationToken.None));
    }

    [Fact]
    public async Task LogonNotAllowedInAnotherDomain_NotCounted()
    {
        var gate = CreateGate();
        gate.StartScan(1);

        await gate.WaitAsync(OtherDomainHost, "10.0.1.1", Timeout, CancellationToken.None);
        gate.Report(OtherDomainHost, "10.0.0.1", WindowsNetApiInfo.FromError(1385));

        Assert.False(gate.IsRejected);
        Assert.Equal(WindowsLogonGate.Decision.Check,
            await gate.WaitAsync(OtherDomainHost, "10.0.1.2", Timeout, CancellationToken.None));
    }

    [Fact]
    public async Task UngatedHost_LogonNotAllowed_NotSkipped()
    {
        var gate = CreateGate();

        await gate.WaitAsync(WorkgroupHost, "10.0.2.1", Timeout, CancellationToken.None);
        gate.ReportUngated(WorkgroupHost, "10.0.2.1", WindowsNetApiInfo.FromError(1385));

        Assert.Equal(WindowsLogonGate.Decision.Proceed,
            await gate.WaitAsync(WorkgroupHost, "10.0.2.1", Timeout, CancellationToken.None));
    }

    [Fact]
    public async Task AcceptedInTheDomain_ResetsTheLogonFailures()
    {
        // A successful logon resets the failures at the domain controller (badPwdCount)
        var gate = CreateGate();

        await gate.WaitAsync(OtherDomainHost, "10.0.1.1", Timeout, CancellationToken.None);
        gate.Report(OtherDomainHost, "10.0.0.1", LogonFailure);

        await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None);
        gate.Report(CorpHost, "10.0.0.1", Success);

        // Next scan: one failure is below the limit again
        gate.StartScan(MaxLogonFailures);

        await gate.WaitAsync(OtherDomainHost, "10.0.1.1", Timeout, CancellationToken.None);
        gate.Report(OtherDomainHost, "10.0.0.1", LogonFailure);

        Assert.False(gate.IsRejected);
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
        gate.Report(CorpHost, "10.0.0.1", WindowsNetApiInfo.FromError(error));

        Assert.False(gate.IsRejected);
        Assert.Equal(WindowsLogonGate.Decision.Check,
            await gate.WaitAsync(CorpHost, "10.0.0.2", Timeout, CancellationToken.None));
    }

    [Fact]
    public async Task UngatedHost_UnknownError_Skipped()
    {
        var gate = CreateGate();

        await gate.WaitAsync(WorkgroupHost, "10.0.2.1", Timeout, CancellationToken.None);
        gate.ReportUngated(WorkgroupHost, "10.0.2.1", WindowsNetApiInfo.FromError(1793));

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
        Assert.False(gate.IsGated(WorkgroupHost));

        // Without the computer name of the host it can't be logged on as its local account
        Assert.True(gate.IsGated(null));
        Assert.True(gate.IsGated(new NTLMChallengeInfo(null, string.Empty, string.Empty, string.Empty, string.Empty,
            string.Empty, null)));
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
        gate.Report(CorpHost, "10.0.0.1", LogonFailure);

        Assert.Equal(WindowsLogonGate.Decision.Skip, await gate.WaitAsync(CorpHost, "10.0.0.1", Timeout, CancellationToken.None));

        // Not for a host that is not gated (e.g. the local admin of a workgroup computer)
        Assert.Equal(WindowsLogonGate.Decision.Proceed,
            await gate.WaitAsync(WorkgroupHost, "10.0.2.1", Timeout, CancellationToken.None));
    }
}
