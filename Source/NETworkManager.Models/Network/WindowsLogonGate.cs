using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace NETworkManager.Models.Network;

/// <summary>
///     Protects the account of the Windows credentials of a scan from being locked out. With a wrong password, every
///     host in a domain makes the domain controller count a failed logon - a /24 with many Windows hosts would reach the
///     lockout threshold in seconds. The credentials are checked on one host first (the others wait); only if they are
///     accepted, the hosts run in parallel:
///     <list type="bullet">
///         <item>Accepted (success, access denied = logged on without rights, or a logon type the host doesn't grant
///         after the password was checked) on a host in the domain of the credentials: all hosts run, the logon failures
///         of the account are reset (like the domain controller does). A success on a host of another domain (or with
///         credentials without domain) proves nothing - a host that maps a failed logon to guest (e.g. a NAS with
///         <c>map to guest = bad user</c>) or a local account with the same name also answer a wrong password: only
///         this host uses them, the next host checks them.</item>
///         <item>Logon failure on a host in the domain of the credentials (the domain controller checked them), any
///         logon failure if the credentials have no domain (e.g. "admin"), or the max. logon failures (setting) on hosts
///         of other domains: rejected, the remaining hosts are not tried.</item>
///         <item>Errors that show the logon was not decided (timeout, host / RPC / domain controller of the host not
///         reachable, see <see cref="WindowsHostInfo.IsLogonNotDecided" />) or a logon failure on a computer of
///         another domain: the next host checks. Every other error counts as a logon failure (e.g. a Kerberos / SSPI
///         error of a wrong password that is not known as one).</item>
///     </list>
///     Hosts whose logon can't count at a domain controller are not gated: computers in a workgroup and local accounts
///     ("." or the computer name as domain; "." is logged on with the computer name of each host, see
///     <see cref="WindowsCredentials.ForHost" />) - a failure there only affects the host itself. Only a host whose
///     computer and domain name are known from its NTLM challenge.
///     <para>
///         Per account (e.g. "CORP\admin", also "corp.example.com\admin" = "admin@corp.example.com") for all gates
///         (tabs) until the application is closed, like the domain controller counts them: one host checks them at a
///         time, the logon failures are counted, and a rejected password stays rejected - a new gate (e.g. another tab
///         of the same profile) with it starts rejected. A new gate with another password (e.g. corrected in the
///         profile) or with credentials the user entered (also unchanged, e.g. after the account was unlocked) checks
///         them again.
///     </para>
/// </summary>
public sealed class WindowsLogonGate
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(WindowsLogonGate));

    /// <summary>
    ///     State of each account for all gates of the application, see <see cref="GetAccountKey" />.
    /// </summary>
    private static readonly ConcurrentDictionary<string, AccountState> Accounts =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Random key of the password fingerprints (<see cref="GetPasswordFingerprint" />), new for every start of the
    ///     application - the fingerprints only exist in memory.
    /// </summary>
    private static readonly byte[] FingerprintKey = RandomNumberGenerator.GetBytes(32);

    /// <summary>
    ///     State of an account, shared by all its gates.
    /// </summary>
    private sealed class AccountState
    {
        /// <summary>
        ///     One host of all gates of the account checks the credentials at a time.
        /// </summary>
        public readonly SemaphoreSlim Turn = new(1, 1);

        /// <summary>
        ///     Fingerprints of the rejected passwords: a new gate with one of them starts rejected. Locked by the state.
        /// </summary>
        public readonly HashSet<string> RejectedPasswords = new(StringComparer.Ordinal);

        /// <summary>
        ///     Logon failures of the account (reset by an accept in the domain).
        /// </summary>
        public int LogonFailures;
    }

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

    // Order: _lock, then _accountState (never the other way)
    private readonly Lock _lock = new();

    // Of the account (shared with its other gates): its turn, rejected passwords and logon failures
    private readonly AccountState _accountState;
    private readonly SemaphoreSlim _turn;

    // Keyed hash of the password of the credentials, see GetPasswordFingerprint
    private readonly string _passwordFingerprint;

    // Account as used for the logon (e.g. "CORP\admin") for the log, never the password
    private readonly string _account;

    private readonly string _domain;
    private readonly bool _isLocalAccount;

    // Hosts of the scan that were not tried (rejected / waited longer than the timeout), for the log of the scan
    private int _skippedHosts;
    private int _timedOutHosts;

    private State _state;

    // Max. logon failures on hosts of other domains (setting of the scan, see StartScan), 1 until a scan started
    private int _maxLogonFailures = 1;

    // Hosts that are not gated and rejected the credentials themselves (e.g. a workgroup computer): not tried again
    private readonly HashSet<string> _rejectedHosts = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Creates the gate for the credentials of a scan (kept by the tab for all its scans).
    /// </summary>
    /// <param name="credentials">Credentials of the scan.</param>
    /// <param name="isEnteredByUser">
    ///     The user entered the credentials (e.g. Set Windows credentials..., Scan as..., also unchanged): checked
    ///     again also if their password was rejected before. Otherwise (e.g. of a profile) a rejected password starts
    ///     rejected.
    /// </param>
    public WindowsLogonGate(WindowsCredentials credentials, bool isEnteredByUser = false)
    {
        var (username, domain) = WindowsLogon.SplitUsername(credentials.Username, credentials.Domain);

        // UPN (user@corp.example.com): the domain is the suffix
        if (domain == null && username.Contains('@'))
            domain = username[(username.LastIndexOf('@') + 1)..];

        _domain = domain ?? string.Empty;
        _isLocalAccount = _domain == ".";

        _account = credentials.DisplayName;

        _accountState = Accounts.GetOrAdd(GetAccountKey(username, _domain), _ => new AccountState());
        _turn = _accountState.Turn;
        _passwordFingerprint = GetPasswordFingerprint(credentials.Password);

        lock (_accountState)
        {
            if (isEnteredByUser)
                _accountState.RejectedPasswords.Remove(_passwordFingerprint);
            else if (_accountState.RejectedPasswords.Contains(_passwordFingerprint))
                _state = State.Rejected;
        }
    }

    /// <summary>
    ///     Gets the key of an account: "domain\user" (the suffix of a UPN as domain, so "admin@corp.example.com" is
    ///     "corp.example.com\admin"), the user without domain.
    /// </summary>
    /// <param name="username">Username as used for the logon (without the domain of "DOMAIN\user").</param>
    /// <param name="domain">Domain (also of a UPN), empty if none.</param>
    /// <returns>Key, compared ignoring the case.</returns>
    private static string GetAccountKey(string username, string domain)
    {
        if (username.Contains('@'))
            username = username[..username.LastIndexOf('@')];

        return string.IsNullOrEmpty(domain) ? username : $@"{domain}\{username}";
    }

    /// <summary>
    ///     Gets a keyed hash (HMAC-SHA256, random key per start of the application) of a password, to know if a password
    ///     was rejected without keeping it. The password is only copied to a pinned buffer that is zeroed right away.
    /// </summary>
    /// <param name="password">Password, null for an empty one.</param>
    /// <returns>Fingerprint (Base64).</returns>
    private static string GetPasswordFingerprint(SecureString password)
    {
        if (password == null || password.Length == 0)
            return Convert.ToBase64String(HMACSHA256.HashData(FingerprintKey, ReadOnlySpan<byte>.Empty));

        var length = password.Length * 2;
        var buffer = GC.AllocateArray<byte>(length, pinned: true);
        var unmanaged = Marshal.SecureStringToGlobalAllocUnicode(password);

        try
        {
            Marshal.Copy(unmanaged, buffer, 0, length);

            return Convert.ToBase64String(HMACSHA256.HashData(FingerprintKey, buffer));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(buffer);
            Marshal.ZeroFreeGlobalAllocUnicode(unmanaged);
        }
    }

    /// <summary>
    ///     Rejects the credentials (called in the lock): also for the other gates of the account with this password.
    /// </summary>
    private void SetRejected()
    {
        _state = State.Rejected;

        lock (_accountState)
            _accountState.RejectedPasswords.Add(_passwordFingerprint);
    }

    /// <summary>
    ///     Raised once when the credentials are rejected - also after the scan was completed (a check that exceeded the
    ///     timeout reports its result when Windows returns). Raised on the thread of the report.
    /// </summary>
    public event EventHandler Rejected;

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
    ///     kept (also a rejection of the password in another gate of the account since then).
    /// </summary>
    /// <param name="maxLogonFailures">
    ///     Max. logon failures on hosts of other domains before the credentials are rejected (setting, min. 1).
    /// </param>
    public void StartScan(int maxLogonFailures)
    {
        State state;

        lock (_lock)
        {
            _maxLogonFailures = Math.Max(1, maxLogonFailures);

            if (_state == State.Accepted)
                _state = State.Unchecked;

            UpdateFromAccount();

            state = _state;
        }

        Interlocked.Exchange(ref _skippedHosts, 0);
        Interlocked.Exchange(ref _timedOutHosts, 0);

        if (state == State.Rejected)
            Log.Info($"Windows credentials \"{_account}\" were rejected before (in an earlier scan or another tab), " +
                     "they are not used for the domain hosts of this scan (lockout protection).");
        else
            Log.Info($"Scan with the Windows credentials \"{_account}\": checked on one host first (lockout " +
                     $"protection, max. {_maxLogonFailures} logon failure(s) on hosts of other domains, " +
                     $"{Volatile.Read(ref _accountState.LogonFailures)} counted for the account).");
    }

    /// <summary>
    ///     Logs the hosts of the scan that didn't use the credentials (rejected, or waited longer than the timeout for
    ///     the check on another host) - once per scan instead of per host.
    /// </summary>
    public void LogScanSummary()
    {
        var skipped = Volatile.Read(ref _skippedHosts);
        var timedOut = Volatile.Read(ref _timedOutHosts);

        if (skipped > 0)
            Log.Info($"{skipped} host(s) didn't use the Windows credentials \"{_account}\": rejected (lockout " +
                     "protection).");

        if (timedOut > 0)
            Log.Info($"{timedOut} host(s) didn't use the Windows credentials \"{_account}\": waited longer than " +
                     "the timeout while they were checked on another host (lockout protection).");
    }

    /// <summary>
    ///     Name of a host for the log: IP address, computer name and domain of the NTLM challenge (e.g. "10.0.0.5
    ///     (CLIENT01, domain CORP)", "10.0.2.1 (PC01, workgroup)").
    /// </summary>
    private static string GetHostName(string host, NTLMChallengeInfo ntlm)
    {
        if (string.IsNullOrEmpty(ntlm?.NetBIOSComputerName))
            return $"{host} (no NTLM computer name)";

        return ntlm.IsDomainMember
            ? $"{host} ({ntlm.NetBIOSComputerName}, domain {ntlm.NetBIOSDomainName})"
            : $"{host} ({ntlm.NetBIOSComputerName}, workgroup)";
    }

    /// <summary>
    ///     Win32 error for the log, e.g. "1326 (The user name or password is incorrect.)".
    /// </summary>
    private static string GetError(int error)
    {
        return $"{error} ({WindowsHostInfo.GetErrorMessage(error)})";
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
        // E.g. a challenge without (or with a truncated) target info: empty names are no workgroup computer and no
        // computer of a local account - also not for ".": the logon needs the computer name of the host
        if (ntlm == null || string.IsNullOrEmpty(ntlm.NetBIOSComputerName) ||
            string.IsNullOrEmpty(ntlm.NetBIOSDomainName))
            return true;

        // Local account: logged on with the computer name of the host
        if (_isLocalAccount)
            return false;

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
            bool isRejectedHost;

            lock (_lock)
                isRejectedHost = _rejectedHosts.Contains(host);

            if (!isRejectedHost)
                return Decision.Proceed;

            Log.Debug($"Windows credentials \"{_account}\" not used for {GetHostName(host, ntlm)}: the host " +
                      "rejected them in an earlier scan (lockout protection).");

            return Decision.Skip;
        }

        var decision = GetDecision();

        if (decision == null)
        {
            if (!await _turn.WaitAsync(timeout, cancellationToken).ConfigureAwait(false))
            {
                decision = GetDecision() ?? Decision.Timeout;
            }
            else
            {
                // Another host may have decided while this one waited
                decision = GetDecision();

                if (decision == null)
                {
                    Log.Debug($"Windows credentials \"{_account}\" are checked on {GetHostName(host, ntlm)}, the " +
                              "other hosts wait (lockout protection).");

                    return Decision.Check;
                }

                _turn.Release();
            }
        }

        switch (decision)
        {
            case Decision.Skip:
                Interlocked.Increment(ref _skippedHosts);
                break;
            case Decision.Timeout:
                Interlocked.Increment(ref _timedOutHosts);
                break;
        }

        return decision.Value;
    }

    /// <summary>
    ///     Reports the result of a host that checked the credentials (<see cref="Decision.Check" />) and lets the next
    ///     host check them if it didn't decide.
    /// </summary>
    /// <param name="ntlm">NTLM challenge of the host, null if none.</param>
    /// <param name="host">IP address of the host (for the log).</param>
    /// <param name="info">Result of the NetApi requests, null if they didn't run (e.g. canceled).</param>
    public void Report(NTLMChallengeInfo ntlm, string host, WindowsNetApiInfo info)
    {
        var isRejected = false;

        // Logged outside the lock
        Action log = null;
        var hostName = GetHostName(host, ntlm);

        try
        {
            if (info == null)
            {
                log = () => Log.Debug($"Windows credentials \"{_account}\" not checked on {hostName} (canceled), " +
                                      "the next host checks them.");
                return;
            }

            lock (_lock)
            {
                if (_state != State.Unchecked)
                    return;

                // Access denied (logged on, but without rights) or a logon type the host doesn't grant (checked after
                // the password, e.g. "Deny access to this computer from the network" on a domain controller): the
                // credentials are correct. Only proven by a host in their domain: another host may have logged on as
                // guest or with a local account of the same name, also with a wrong password (only this host used them,
                // the next host checks them)
                if (info.Error is 0 or 5 or WindowsHostInfo.ErrorLogonTypeNotGranted)
                {
                    if (IsInDomain(ntlm))
                    {
                        _state = State.Accepted;

                        // A successful logon resets the failures at the domain controller (badPwdCount)
                        Interlocked.Exchange(ref _accountState.LogonFailures, 0);

                        log = () => Log.Info($"Windows credentials \"{_account}\" accepted on {hostName} (in their " +
                                             $"domain, error {GetError(info.Error)}): the other hosts use them.");
                    }
                    else
                    {
                        log = () => Log.Info($"Windows credentials \"{_account}\" logged on to {hostName} (error " +
                                             $"{GetError(info.Error)}), but not in their domain - not proven (e.g. " +
                                             "guest, local account): the next host checks them (lockout protection).");
                    }

                    return;
                }

                // Only an error that shows that no domain controller checked them, or a logon that is not allowed for
                // the account (e.g. logon hours, Protected Users with NTLM - no failed logon), lets the next host check
                // them, every other error may be a wrong password
                if (WindowsHostInfo.IsLogonNotDecided(info.Error) || WindowsHostInfo.IsLogonNotAllowed(info.Error))
                {
                    log = WindowsHostInfo.IsLogonNotAllowed(info.Error)
                        ? () => Log.Info($"Logon of \"{_account}\" to {hostName} not allowed: error " +
                                         $"{GetError(info.Error)} - no logon failure counted, the next host checks " +
                                         "the credentials.")
                        : () => Log.Debug($"Windows credentials \"{_account}\" not checked on {hostName}: error " +
                                          $"{GetError(info.Error)}, the next host checks them.");
                    return;
                }

                var logonFailures = Interlocked.Increment(ref _accountState.LogonFailures);
                var isInDomain = IsInDomain(ntlm);

                // Without a domain (e.g. "admin"), the host's domain controller may have checked them
                if (isInDomain || string.IsNullOrEmpty(_domain) || logonFailures >= _maxLogonFailures)
                {
                    SetRejected();
                    isRejected = true;
                }

                var reason = isInDomain
                    ? "a logon failure in their domain"
                    : string.IsNullOrEmpty(_domain)
                        ? "a logon failure without domain (the domain controller of the host may count it)"
                        : $"{logonFailures} of max. {_maxLogonFailures} logon failure(s) on hosts of other domains";

                log = () =>
                {
                    Log.Warn($"Logon of \"{_account}\" failed on {hostName}: error {GetError(info.Error)} " +
                             $"(logon failure {logonFailures} of the account).");

                    if (isRejected)
                        Log.Warn($"Windows credentials \"{_account}\" rejected after {reason}: not used for the " +
                                 "other hosts and the next scans of the tab (lockout protection).");
                    else
                        Log.Info($"Windows credentials \"{_account}\": {reason}, the next host checks them.");
                };
            }
        }
        finally
        {
            _turn.Release();

            log?.Invoke();
        }

        // Outside the lock (the handler may read the state)
        if (isRejected)
            Rejected?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Reports the result of a host that is not gated (<see cref="IsGated" /> false, <see cref="Decision.Proceed" />):
    ///     after a logon failure (every error except success, access denied and the errors of
    ///     <see cref="WindowsHostInfo.IsLogonNotAllowed" /> / <see cref="WindowsHostInfo.IsLogonNotDecided" />), the
    ///     host is skipped in the next scans.
    /// </summary>
    /// <param name="ntlm">NTLM challenge of the host (for the log).</param>
    /// <param name="host">IP address of the host.</param>
    /// <param name="info">Result of the NetApi requests, null if they didn't run (e.g. canceled).</param>
    public void ReportUngated(NTLMChallengeInfo ntlm, string host, WindowsNetApiInfo info)
    {
        if (info == null || info.Error is 0 or 5 || WindowsHostInfo.IsLogonNotAllowed(info.Error) ||
            WindowsHostInfo.IsLogonNotDecided(info.Error))
            return;

        lock (_lock)
            _rejectedHosts.Add(host);

        Log.Warn($"Logon of \"{_account}\" failed on {GetHostName(host, ntlm)}: error {GetError(info.Error)} - " +
                 "workgroup computer / local account (not checked by a domain controller), the host is not tried " +
                 "again in the next scans of the tab (lockout protection).");
    }

    /// <summary>
    ///     Takes a rejection of the password by another gate of the account (called in the lock): e.g. another tab
    ///     checked them while this gate waited for the turn of the account.
    /// </summary>
    private void UpdateFromAccount()
    {
        if (_state == State.Rejected)
            return;

        lock (_accountState)
        {
            if (_accountState.RejectedPasswords.Contains(_passwordFingerprint))
                _state = State.Rejected;
        }
    }

    private Decision? GetDecision()
    {
        lock (_lock)
        {
            UpdateFromAccount();

            return _state switch
            {
                State.Accepted => Decision.Proceed,
                State.Rejected => Decision.Skip,
                _ => null
            };
        }
    }
}
