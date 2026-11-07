using System;
using System.Collections.Generic;

namespace NETworkManager.Models.Network;

/// <summary>
///     Role of a Windows host (server type of <c>NetServerGetInfo</c>).
/// </summary>
public enum WindowsHostRole
{
    /// <summary>
    ///     Not known (not read or access denied).
    /// </summary>
    Unknown,

    /// <summary>
    ///     Workstation (client).
    /// </summary>
    Workstation,

    /// <summary>
    ///     Server.
    /// </summary>
    Server,

    /// <summary>
    ///     Domain controller.
    /// </summary>
    DomainController
}

/// <summary>
///     Information of a Windows host read with the NetApi (SMB / RPC, with the credentials of the scan), see
///     <see cref="WindowsNetApiResolver" />.
/// </summary>
/// <param name="Error">
///     Win32 error of the first request (<c>NetServerGetInfo</c>), 0 if it succeeded - e.g. 5 (access denied), 1326
///     (wrong username or password). Nothing else is read after an error.
/// </param>
/// <param name="Role">Role of the host.</param>
/// <param name="IsDomainMember">The host is in a domain (null if not known).</param>
/// <param name="Description">Description of the computer (empty if none).</param>
/// <param name="Workgroup">Workgroup or NetBIOS domain name (empty if not read).</param>
/// <param name="Users">Logged-on users ("DOMAIN\user"), null if they could not be read.</param>
/// <param name="UsersError">Win32 error of the logged-on users (e.g. 5: needs admin rights), 0 if read.</param>
/// <param name="Registry">Version of the remote registry, null if it could not be read (admin rights, service).</param>
public sealed record WindowsNetApiInfo(
    int Error,
    WindowsHostRole Role,
    bool? IsDomainMember,
    string Description,
    string Workgroup,
    IReadOnlyList<string> Users,
    int UsersError,
    WindowsRegistryVersion Registry)
{
    /// <summary>
    ///     Creates the result of a request that failed.
    /// </summary>
    /// <param name="error">Win32 error.</param>
    public static WindowsNetApiInfo FromError(int error)
    {
        return new WindowsNetApiInfo(error, WindowsHostRole.Unknown, null, string.Empty, string.Empty, null, error,
            null);
    }
}

/// <summary>
///     Version of Windows from the remote registry (<c>HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion</c>).
/// </summary>
/// <param name="Version">Version with the patch level (update build revision), e.g. 10.0.26200.6584.</param>
/// <param name="DisplayVersion">Display version, e.g. "25H2" (empty for older versions).</param>
/// <param name="EditionID">Edition, e.g. "Professional" or "ServerStandard".</param>
/// <param name="IsServer">Installation type is a server (null if not set).</param>
public sealed record WindowsRegistryVersion(
    Version Version,
    string DisplayVersion,
    string EditionID,
    bool? IsServer);
