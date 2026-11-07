using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using Xunit;

namespace NETworkManager.Tests.Network;

/// <summary>
///     Log of the lockout protection: failed logons with account and host, the rejection - never the password.
/// </summary>
public class WindowsLogonGateLogTests
{
    private const string Password = "S3cret-Passw0rd";

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private static readonly NTLMChallengeInfo CorpHost = new(new Version(10, 0, 26200), "CLIENT01", "CORP",
        "client01.corp.example.com", "corp.example.com", "example.com", null);

    private static readonly NTLMChallengeInfo WorkgroupHost =
        new(new Version(10, 0, 26200), "PC01", "PC01", "PC01", "PC01", string.Empty, null);

    /// <summary>
    ///     Runs the scenario with a memory appender on the logger of the gate, returns the messages of the account.
    /// </summary>
    private static async Task<string[]> CaptureAsync(string account, Func<Task> scenario)
    {
        var logger = (Logger)LogManager.GetLogger(typeof(WindowsLogonGate)).Logger;
        var appender = new MemoryAppender();

        logger.Level = Level.Debug;
        logger.Repository.Configured = true;
        logger.AddAppender(appender);

        try
        {
            await scenario();
        }
        finally
        {
            logger.RemoveAppender(appender);
        }

        // Other tests may log at the same time (other accounts)
        return appender.GetEvents().Select(x => $"{x.Level} {x.RenderedMessage}")
            .Where(x => x.Contains(account, StringComparison.Ordinal)).ToArray();
    }

    [Fact]
    public async Task LogonFailureInTheDomain_FailureAndRejectionLogged()
    {
        var username = $"admin-{Guid.NewGuid():N}";
        var gate = new WindowsLogonGate(new WindowsCredentials(username, "CORP",
            SecureStringHelper.ConvertToSecureString(Password)));

        var messages = await CaptureAsync(username, async () =>
        {
            gate.StartScan(2);

            await gate.WaitAsync(CorpHost, "10.0.0.5", Timeout, CancellationToken.None);
            gate.Report(CorpHost, "10.0.0.5", WindowsNetApiInfo.FromError(1326));

            // Not tried: counted for the summary of the scan, not logged per host
            await gate.WaitAsync(CorpHost, "10.0.0.6", Timeout, CancellationToken.None);
            gate.LogScanSummary();
        });

        Assert.Contains(messages, x => x.StartsWith("WARN") && x.Contains($@"CORP\{username}") &&
                                       x.Contains("10.0.0.5 (CLIENT01, domain CORP)") && x.Contains("1326"));
        Assert.Contains(messages, x => x.StartsWith("WARN") && x.Contains("rejected") &&
                                       x.Contains("lockout protection"));
        Assert.Contains(messages, x => x.StartsWith("INFO") && x.Contains("1 host(s)"));
        Assert.DoesNotContain(messages, x => x.Contains(Password));
    }

    [Fact]
    public async Task UngatedHost_FailureLogged()
    {
        var username = $"admin-{Guid.NewGuid():N}";
        var gate = new WindowsLogonGate(new WindowsCredentials(username, "CORP",
            SecureStringHelper.ConvertToSecureString(Password)));

        var messages = await CaptureAsync(username, async () =>
        {
            gate.StartScan(2);

            await gate.WaitAsync(WorkgroupHost, "10.0.2.1", Timeout, CancellationToken.None);
            gate.ReportUngated(WorkgroupHost, "10.0.2.1", WindowsNetApiInfo.FromError(1326));
        });

        Assert.Contains(messages, x => x.StartsWith("WARN") && x.Contains("10.0.2.1 (PC01, workgroup)") &&
                                       x.Contains("not tried again"));
        Assert.DoesNotContain(messages, x => x.Contains(Password));
    }
}
