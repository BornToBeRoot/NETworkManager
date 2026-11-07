using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

/// <summary>
///     Protects the account of the Windows credentials of a scan from being locked out. With a wrong password, every
///     host in a domain makes the domain controller count a failed logon - a /24 with many Windows hosts would reach the
///     lockout threshold in seconds. The credentials are checked on one host first (the others wait); only if they are
///     accepted, the hosts run in parallel:
///     <list type="bullet">
///         <item>Accepted (success, or access denied = logged on without rights) on a host in the domain of the
///         credentials: all hosts run. A success on a host of another domain (or with credentials without domain)
///         proves nothing - a host that maps a failed logon to guest (e.g. a NAS with <c>map to guest = bad
///         user</c>) or a local account with the same name also answer a wrong password: only this host uses them, the
///         next host checks them.</item>
///         <item>Logon failure on a host in the domain of the credentials (the domain controller checked them), any
///         logon failure if the credentials have no domain (e.g. "admin"), or the max. logon failures (setting) on hosts
///         of other domains: rejected, the remaining hosts are not tried. The logon failures are counted per account for
///         all gates (tabs, credentials entered again) until the application is closed, like the domain controller
///         counts them; a new gate still checks them once (e.g. the password was corrected).</item>
///         <item>Errors that show the logon was not decided (timeout, host / RPC / domain controller of the host not
///         reachable, see <see cref="WindowsHostInfo.IsLogonNotDecided" />) or a logon failure on a computer of
///         another domain: the next host checks. Every other error counts as a logon failure (e.g. a Kerberos / SSPI
///         error of a wrong password that is not known as one).</item>
///     </list>
///     Hosts whose logon can't count at a domain controller are not gated: computers in a workgroup and local accounts
///     ("." or the computer name as domain) - a failure there only affects the host itself.
/// </summary>
public sealed class WindowsLogonGate
{
    /// <summary>
    ///     Logon failures of each account (as used for the logon, e.g. "CORP\admin") on hosts of other domains, for all
    ///     gates of the application.
    /// </summary>
    private static readonly ConcurrentDictionary<string, StrongBox<int>> LogonFailuresOfAccounts =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     What a host does with the credentials.
    /// </summary>
    public enum Decision
    {
        /// <summary>
        ///     Use them (accepted, or not gated). Don't call <see cref="Report" />.
        /// </summary>
        Proceed,

        /// <summary>
        ///     Use them and check them: call <see cref="Report" /> with the result (also on an error / cancellation).
        /// </summary>
        Check,

        /// <summary>
        ///     Don't use them, they were rejected on another host.
        /// </summary>
        Skip,

        /// <summary>
        ///     Not used: the host waited longer than the timeout for its turn (another host is still checking them).
        /// </summary>
        Timeout
    }

    private enum State
    {
        Unchecked,
        Accepted,
        Rejected
    }

    private readonly Lock _lock = new();
    private readonly SemaphoreSlim _turn = new(1, 1);

    private readonly string _domain;
    private readonly bool _isLocalAccount;

    // Shared with the other gates of the account
    private readonly StrongBox<int> _logonFailures;

    private State _state;

    // Max. logon failures on hosts of other domains (setting of the scan, see StartScan), 1 until a scan started
    private int _maxLogonFailures = 1;

    // Hosts that are not gated and rejected the credentials themselves (e.g. a workgroup computer): not tried again
    private readonly HashSet<string> _rejectedHosts = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Creates the gate for the credentials of a scan.
    /// </summary>
    /// <param name="credentials">Credentials of the scan.</param>
    public WindowsLogonGate(WindowsCredentials credentials)
    {
        var (username, domain) = WindowsLogon.SplitUsername(credentials.Username, credentials.Domain);

        // UPN (user@corp.example.com): the domain is the suffix
        if (domain == null && username.Contains('@'))
            domain = username[(username.LastIndexOf('@') + 1)..];

        _domain = domain ?? string.Empty;
        _isLocalAccount = _domain == ".";

        _logonFailures = LogonFailuresOfAccounts.GetOrAdd(credentials.DisplayName, _ => new StrongBox<int>());
    }

    /// <summary>
    ///     The credentials were rejected (wrong username or password).
    /// </summary>
    public bool IsRejected
    {
        get
        {
            lock (_lock)
                return _state == State.Rejected;
        }
    }

    /// <summary>
    ///     Starts a scan with the gate (kept by the tab for all its scans): credentials accepted in an earlier scan are
    ///     checked again (e.g. the password was changed or expired since then), the logon failures and a rejection are
    ///     kept.
    /// </summary>
    /// <param name="maxLogonFailures">
    ///     Max. logon failures on hosts of other domains before the credentials are rejected (setting, min. 1).
    /// </param>
    public void StartScan(int maxLogonFailures)
    {
        lock (_lock)
        {
            _maxLogonFailures = Math.Max(1, maxLogonFailures);

            if (_state == State.Accepted)
                _state = State.Unchecked;
        }
    }

    /// <summary>
    ///     Checks if the logon to a host can count at a domain controller: not for a local account and not for a
    ///     computer in a workgroup (known from the NTLM challenge; without challenge or without its computer / domain
    ///     name it is unknown, so it is gated).
    /// </summary>
    /// <param name="ntlm">NTLM challenge of the host, null if none.</param>
    /// <returns>True if the host is gated.</returns>
    public bool IsGated(NTLMChallengeInfo ntlm)
    {
        if (_isLocalAccount)
            return false;

        // E.g. a challenge without (or with a truncated) target info: empty names are no workgroup computer and no
        // computer of a local account
        if (ntlm == null || string.IsNullOrEmpty(ntlm.NetBIOSComputerName) ||
            string.IsNullOrEmpty(ntlm.NetBIOSDomainName))
            return true;

        // Local account of the host (its computer name as domain)
        if (string.Equals(_domain, ntlm.NetBIOSComputerName, StringComparison.OrdinalIgnoreCase))
            return false;

        return ntlm.IsDomainMember;
    }

    /// <summary>
    ///     Checks if a host is in the domain of the credentials (NetBIOS or DNS name), or in the forest whose root domain
    ///     is the domain of the credentials (DNS name, e.g. the UPN suffix "example.com" of a host in
    ///     "corp.example.com"): a domain controller of the forest checks the logon.
    /// </summary>
    /// <param name="ntlm">NTLM challenge of the host, null if none.</param>
    /// <returns>True if the domain controller of the credentials checks the logon.</returns>
    public bool IsInDomain(NTLMChallengeInfo ntlm)
    {
        return ntlm != null && !string.IsNullOrEmpty(_domain) &&
               (string.Equals(_domain, ntlm.NetBIOSDomainName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_domain, ntlm.DNSDomainName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_domain, ntlm.DNSTreeName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Waits until the host can use the credentials: right away if it is not gated or they were accepted, otherwise
    ///     until it is its turn to check them (one host at a time) or they were rejected.
    /// </summary>
    /// <param name="ntlm">NTLM challenge of the host, null if none.</param>
    /// <param name="host">IP address of the host (a host that is not gated is skipped after its own logon failure).</param>
    /// <param name="timeout">Max. time to wait for the turn (a slow host checking them doesn't stall the scan).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>What the host does with the credentials.</returns>
    public async Task<Decision> WaitAsync(NTLMChallengeInfo ntlm, string host, TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        // Not gated (workgroup computer, local account): independent of a rejection by the domain hosts (e.g. the
        // local admin of workgroup computers), but not tried again after its own logon failure (its account would
        // count the wrong password in every scan). Call ReportUngated with the result.
        if (!IsGated(ntlm))
        {
            lock (_lock)
                return _rejectedHosts.Contains(host) ? Decision.Skip : Decision.Proceed;
        }

        var decision = GetDecision();

        if (decision != null)
            return decision.Value;

        if (!await _turn.WaitAsync(timeout, cancellationToken).ConfigureAwait(false))
            return GetDecision() ?? Decision.Timeout;

        // Another host may have decided while this one waited
        decision = GetDecision();

        if (decision == null)
            return Decision.Check;

        _turn.Release();

        return decision.Value;
    }

    /// <summary>
    ///     Reports the result of a host that checked the credentials (<see cref="Decision.Check" />) and lets the next
    ///     host check them if it didn't decide.
    /// </summary>
    /// <param name="ntlm">NTLM challenge of the host, null if none.</param>
    /// <param name="info">Result of the NetApi requests, null if they didn't run (e.g. canceled).</param>
    public void Report(NTLMChallengeInfo ntlm, WindowsNetApiInfo info)
    {
        try
        {
            if (info == null)
                return;

            lock (_lock)
            {
                if (_state != State.Unchecked)
                    return;

                // Access denied: logged on, but without rights - the credentials are correct. Only proven by a host in
                // their domain: another host may have logged on as guest or with a local account of the same name,
                // also with a wrong password (only this host used them, the next host checks them)
                if (info.Error is 0 or 5)
                {
                    if (IsInDomain(ntlm))
                        _state = State.Accepted;

                    return;
                }

                // Only an error that shows that no domain controller checked them lets the next host check them,
                // every other error may be a wrong password
                if (WindowsHostInfo.IsLogonNotDecided(info.Error))
                    return;

                var logonFailures = Interlocked.Increment(ref _logonFailures.Value);

                // Without a domain (e.g. "admin"), the host's domain controller may have checked them
                if (IsInDomain(ntlm) || string.IsNullOrEmpty(_domain) || logonFailures >= _maxLogonFailures)
                    _state = State.Rejected;
            }
        }
        finally
        {
            _turn.Release();
        }
    }

    /// <summary>
    ///     Reports the result of a host that is not gated (<see cref="IsGated" /> false, <see cref="Decision.Proceed" />):
    ///     after a logon failure (every error except success, access denied and the errors of
    ///     <see cref="WindowsHostInfo.IsLogonNotDecided" />), the host is skipped in the next scans.
    /// </summary>
    /// <param name="host">IP address of the host.</param>
    /// <param name="info">Result of the NetApi requests, null if they didn't run (e.g. canceled).</param>
    public void ReportUngated(string host, WindowsNetApiInfo info)
    {
        if (info == null || info.Error is 0 or 5 || WindowsHostInfo.IsLogonNotDecided(info.Error))
            return;

        lock (_lock)
            _rejectedHosts.Add(host);
    }

    private Decision? GetDecision()
    {
        lock (_lock)
        {
            return _state switch
            {
                State.Accepted => Decision.Proceed,
                State.Rejected => Decision.Skip,
                _ => null
            };
        }
    }
}
