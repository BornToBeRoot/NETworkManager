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
    ///     Win32 errors of a failed logon with the credentials (wrong username or password, disabled / locked / expired
    ///     account, logon not allowed).
    /// </summary>
    private static readonly HashSet<int> LogonErrors = [1326, 1327, 1328, 1329, 1330, 1331, 1385, 1907, 1909];

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
    ///     The logon to the host with the credentials of the scan failed (e.g. wrong password).
    /// </summary>
    public bool IsLogonFailure => IsLogonError(Error);

    /// <summary>
    ///     Checks if a Win32 error of the NetApi requests is a failed logon with the credentials (wrong username or
    ///     password, disabled / locked / expired account, logon not allowed).
    /// </summary>
    /// <param name="error">Win32 error.</param>
    /// <returns>True for a failed logon.</returns>
    public static bool IsLogonError(int error)
    {
        return LogonErrors.Contains(error);
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
