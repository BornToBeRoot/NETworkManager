using System;
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
///         credentials, or on the third host of other domains (a host that maps a failed logon to guest, e.g. a NAS
///         with <c>map to guest = bad user</c>, also answers a wrong password): all hosts run.</item>
///         <item>Logon failure on a host in the domain of the credentials (the domain controller checked them), any
///         logon failure if the credentials have no domain (e.g. "admin"), or the third logon failure on hosts of
///         other domains: rejected, the remaining hosts are not tried.</item>
///         <item>Other errors (timeout, no RPC) or a logon failure on a computer of another domain: the next host
///         checks.</item>
///     </list>
///     Hosts whose logon can't count at a domain controller are not gated: computers in a workgroup and local accounts
///     ("." or the computer name as domain) - a failure there only affects the host itself.
/// </summary>
public sealed class WindowsLogonGate
{
    /// <summary>
    ///     Max. logon failures on hosts that are not in the domain of the credentials before they are rejected.
    /// </summary>
    public const int MaxLogonFailures = 3;

    /// <summary>
    ///     Accepts on hosts that are not in the domain of the credentials before they are accepted (e.g. the domain of
    ///     a UPN suffix is not the domain of the hosts) - one could be a host that maps a failed logon to guest.
    /// </summary>
    public const int MinAcceptsOutsideDomain = 3;

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

    private State _state;
    private int _logonFailures;
    private int _acceptsOutsideDomain;

    /// <summary>
    ///     Creates the gate for the credentials of a scan.
    /// </summary>
    /// <param name="credentials">Credentials of the scan.</param>
    /// <param name="isRejected">The credentials were already rejected in an earlier scan of the tab.</param>
    public WindowsLogonGate(WindowsCredentials credentials, bool isRejected = false)
    {
        var (username, domain) = WindowsLogon.SplitUsername(credentials.Username, credentials.Domain);

        // UPN (user@corp.example.com): the domain is the suffix
        if (domain == null && username.Contains('@'))
            domain = username[(username.LastIndexOf('@') + 1)..];

        _domain = domain ?? string.Empty;
        _isLocalAccount = _domain == ".";
        _state = isRejected ? State.Rejected : State.Unchecked;
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
    ///     Checks if the logon to a host can count at a domain controller: not for a local account and not for a
    ///     computer in a workgroup (known from the NTLM challenge; without challenge it is unknown, so it is gated).
    /// </summary>
    /// <param name="ntlm">NTLM challenge of the host, null if none.</param>
    /// <returns>True if the host is gated.</returns>
    public bool IsGated(NTLMChallengeInfo ntlm)
    {
        if (_isLocalAccount)
            return false;

        if (ntlm == null)
            return true;

        // Local account of the host (its computer name as domain)
        if (string.Equals(_domain, ntlm.NetBIOSComputerName, StringComparison.OrdinalIgnoreCase))
            return false;

        return ntlm.IsDomainMember;
    }

    /// <summary>
    ///     Checks if a host is in the domain of the credentials (NetBIOS or DNS name).
    /// </summary>
    /// <param name="ntlm">NTLM challenge of the host, null if none.</param>
    /// <returns>True if the domain controller of the credentials checks the logon.</returns>
    public bool IsInDomain(NTLMChallengeInfo ntlm)
    {
        return ntlm != null && !string.IsNullOrEmpty(_domain) &&
               (string.Equals(_domain, ntlm.NetBIOSDomainName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_domain, ntlm.DNSDomainName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Waits until the host can use the credentials: right away if it is not gated or they were accepted, otherwise
    ///     until it is its turn to check them (one host at a time) or they were rejected.
    /// </summary>
    /// <param name="ntlm">NTLM challenge of the host, null if none.</param>
    /// <param name="timeout">Max. time to wait for the turn (a slow host checking them doesn't stall the scan).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>What the host does with the credentials.</returns>
    public async Task<Decision> WaitAsync(NTLMChallengeInfo ntlm, TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        if (!IsGated(ntlm))
            return Decision.Proceed;

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
                // their domain: another host may have logged on as guest, also with a wrong password
                if (info.Error is 0 or 5)
                {
                    if (IsInDomain(ntlm) || ++_acceptsOutsideDomain >= MinAcceptsOutsideDomain)
                        _state = State.Accepted;

                    return;
                }

                if (!WindowsHostInfo.IsLogonError(info.Error))
                    return;

                _logonFailures++;

                // Without a domain (e.g. "admin"), the host's domain controller may have checked them
                if (IsInDomain(ntlm) || string.IsNullOrEmpty(_domain) || _logonFailures >= MaxLogonFailures)
                    _state = State.Rejected;
            }
        }
        finally
        {
            _turn.Release();
        }
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
