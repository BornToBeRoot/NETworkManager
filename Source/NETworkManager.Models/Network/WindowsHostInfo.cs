using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NETworkManager.Models.Network;

/// <summary>
///     Windows host information of the IP Scanner: what the host reveals without credentials in the SMB negotiation
///     (<see cref="SMBInfo" />) and what is read with the NetApi / remote registry with the credentials of the scan
///     (<see cref="WindowsNetApiInfo" />). The values for the result combine both (the more precise one first).
/// </summary>
/// <param name="SMB">SMB / NTLM information, null if the host didn't answer on TCP 445.</param>
/// <param name="NetApi">NetApi information, null if it was not read (no SMB answer).</param>
public sealed record WindowsHostInfo(SMBInfo SMB, WindowsNetApiInfo NetApi)
{
    /// <summary>
    ///     Win32 error of a request that timed out (<c>ERROR_TIMEOUT</c>).
    /// </summary>
    public const int ErrorTimeout = 1460;

    /// <summary>
    ///     Error of a host whose NetApi requests were not sent: the credentials were rejected on another host (wrong
    ///     username or password), see <see cref="WindowsLogonGate" />. Not a Win32 error.
    /// </summary>
    public const int ErrorLogonSkipped = -1;

    /// <summary>
    ///     Error of a host whose NetApi requests were not sent: it waited longer than the timeout while the credentials
    ///     were checked on other hosts, see <see cref="WindowsLogonGate" />. Not a Win32 error.
    /// </summary>
    public const int ErrorLogonNotChecked = -2;

    /// <summary>
    ///     Win32 error of a logon type that is not granted on the host (<c>ERROR_LOGON_TYPE_NOT_GRANTED</c>, e.g. "Deny
    ///     access to this computer from the network"): checked by the host after the domain controller accepted the
    ///     password.
    /// </summary>
    public const int ErrorLogonTypeNotGranted = 1385;

    /// <summary>
    ///     Win32 errors of a logon that is not allowed for the account: account restriction (e.g. Protected Users with
    ///     NTLM), logon hours, workstation, logon type not granted. No failed logon is counted for them (checked after
    ///     the password, or the password was not checked).
    /// </summary>
    private static readonly HashSet<int> LogonNotAllowedErrors = [1327, 1328, 1329, ErrorLogonTypeNotGranted];

    /// <summary>
    ///     Win32 errors of NetApi requests whose logon was not decided: the host (or its SMB / RPC service) can't be
    ///     reached, the connection was closed, the domain controller of the host can't be reached or NTLM is blocked -
    ///     no domain controller checked the password. Every other error may be a failed logon (e.g. a Kerberos / SSPI
    ///     error of a wrong password), see <see cref="IsLogonNotDecided" />.
    /// </summary>
    private static readonly HashSet<int> LogonNotDecidedErrors =
    [
        31, // ERROR_GEN_FAILURE (exception of the call, e.g. the logon was disposed)
        50, // ERROR_NOT_SUPPORTED
        53, // ERROR_BAD_NETPATH
        59, // ERROR_UNEXP_NET_ERR
        64, // ERROR_NETNAME_DELETED
        67, // ERROR_BAD_NET_NAME
        121, // ERROR_SEM_TIMEOUT
        1214, // ERROR_INVALID_NETNAME
        1225, // ERROR_CONNECTION_REFUSED
        1231, // ERROR_NETWORK_UNREACHABLE
        1232, // ERROR_HOST_UNREACHABLE
        1236, // ERROR_CONNECTION_ABORTED
        1311, // ERROR_NO_LOGON_SERVERS
        ErrorTimeout,
        1722, // RPC_S_SERVER_UNAVAILABLE
        1726, // RPC_S_CALL_FAILED
        1727, // RPC_S_CALL_FAILED_DNE
        1753, // EPT_S_NOT_REGISTERED
        1788, // ERROR_TRUSTED_DOMAIN_FAILURE
        1789, // ERROR_TRUSTED_RELATIONSHIP_FAILURE
        1937, // ERROR_NTLM_BLOCKED
        2102, // NERR_WkstaNotStarted
        2114 // NERR_ServerNotStarted
    ];

    /// <summary>
    ///     Version of Windows: of the registry (with patch level), otherwise of the NTLM challenge (major.minor.build),
    ///     null if none is known (or it has no build, e.g. Samba).
    /// </summary>
    public Version Version => NetApi?.Registry?.Version ?? (SMB?.NTLM?.Version is { Build: > 0 } version ? version : null);

    /// <summary>
    ///     Role of the host (NetApi), unknown if it was not read.
    /// </summary>
    public WindowsHostRole Role => NetApi?.Role ?? WindowsHostRole.Unknown;

    /// <summary>
    ///     The host is a server (incl. domain controller): of the role, otherwise of the installation type of the
    ///     registry, null if unknown.
    /// </summary>
    public bool? IsServer => Role switch
    {
        WindowsHostRole.Server or WindowsHostRole.DomainController => true,
        WindowsHostRole.Workstation => false,
        _ => NetApi?.Registry?.IsServer
    };

    /// <summary>
    ///     Name of the operating system, e.g. "Windows 11 25H2" (the display version of the registry for a build that
    ///     is not known yet), empty if not known.
    /// </summary>
    public string OS
    {
        get
        {
            var name = WindowsVersionHelper.GetName(Version, IsServer);

            if (name == null)
                return string.Empty;

            var displayVersion = NetApi?.Registry?.DisplayVersion;

            // Newer Windows 11 build than the list: e.g. "Windows 11 (build 27000)" -> "Windows 11 26H1". Not for other
            // unknown builds: the display version of a server (e.g. 24H2 for Server 2025) doesn't name the release, a
            // name without "Server" would look like a client - they keep the build
            return !string.IsNullOrEmpty(displayVersion) && Version != null &&
                   name == $"Windows 11 (build {Version.Build})"
                ? $"Windows 11 {displayVersion}"
                : name;
        }
    }

    /// <summary>
    ///     Edition (registry), e.g. "Pro" or "Datacenter", empty if not read.
    /// </summary>
    public string Edition => WindowsNetApiResolver.GetEditionName(NetApi?.Registry?.EditionID);

    /// <summary>
    ///     The host is in a domain (NTLM challenge, otherwise NetApi), null if unknown.
    /// </summary>
    public bool? IsDomainMember => SMB?.NTLM != null ? SMB.NTLM.IsDomainMember : NetApi?.IsDomainMember;

    /// <summary>
    ///     Domain (DNS name, otherwise NetBIOS name) or workgroup of the host, empty if not known.
    /// </summary>
    public string Domain
    {
        get
        {
            var ntlm = SMB?.NTLM;

            if (IsDomainMember == true)
            {
                if (!string.IsNullOrEmpty(ntlm?.DNSDomainName))
                    return ntlm.DNSDomainName;

                if (!string.IsNullOrEmpty(ntlm?.NetBIOSDomainName))
                    return ntlm.NetBIOSDomainName;
            }

            // A computer in a workgroup sends its own name as domain in the challenge - the workgroup is only known
            // from the NetApi
            return NetApi?.Workgroup ?? string.Empty;
        }
    }

    /// <summary>
    ///     Computer name of the host (DNS name, otherwise NetBIOS name of the NTLM challenge), empty if not known.
    /// </summary>
    public string ComputerName => !string.IsNullOrEmpty(SMB?.NTLM?.DNSComputerName)
        ? SMB.NTLM.DNSComputerName
        : SMB?.NTLM?.NetBIOSComputerName ?? string.Empty;

    /// <summary>
    ///     Time of the host minus the local time, null if not known.
    /// </summary>
    public TimeSpan? TimeOffset => SMB?.TimeOffset;

    /// <summary>
    ///     Logged-on users, null if they could not be read (see <see cref="UsersError" />).
    /// </summary>
    public IReadOnlyList<string> Users => NetApi?.Users;

    /// <summary>
    ///     Description of the computer, empty if none.
    /// </summary>
    public string Description => NetApi?.Description ?? string.Empty;

    /// <summary>
    ///     Win32 error of the NetApi (0 if it succeeded or was not read).
    /// </summary>
    public int Error => NetApi?.Error ?? 0;

    /// <summary>
    ///     Win32 error of the logged-on users (0 if they were read or the NetApi was not read).
    /// </summary>
    public int UsersError => NetApi?.UsersError ?? 0;

    /// <summary>
    ///     Checks if a Win32 error of the NetApi requests is a logon that is not allowed for the account (e.g. on this
    ///     host or at this time): no failed logon at the domain controller, but the password is not proven either
    ///     (except <see cref="ErrorLogonTypeNotGranted" />).
    /// </summary>
    /// <param name="error">Win32 error.</param>
    /// <returns>True if the logon is not allowed.</returns>
    public static bool IsLogonNotAllowed(int error)
    {
        return LogonNotAllowedErrors.Contains(error);
    }

    /// <summary>
    ///     Checks if a Win32 error of the NetApi requests shows that the logon was not decided (host, network, RPC or
    ///     the domain controller of the host not reachable). The lockout protection counts every other error as a
    ///     failed logon, so an unknown error of a wrong password can't let the next hosts try it.
    /// </summary>
    /// <param name="error">Win32 error.</param>
    /// <returns>True if no domain controller checked the password.</returns>
    public static bool IsLogonNotDecided(int error)
    {
        return LogonNotDecidedErrors.Contains(error);
    }

    /// <summary>
    ///     Gets the message of a Win32 error (of Windows, in the language of the system).
    /// </summary>
    /// <param name="error">Win32 error.</param>
    /// <returns>
    ///     Message, empty for 0, <see cref="ErrorLogonSkipped" /> and <see cref="ErrorLogonNotChecked" /> (translated
    ///     by the caller).
    /// </returns>
    public static string GetErrorMessage(int error)
    {
        return error is 0 or ErrorLogonSkipped or ErrorLogonNotChecked ? string.Empty : new Win32Exception(error).Message;
    }
}
