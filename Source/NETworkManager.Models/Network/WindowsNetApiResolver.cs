using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Reads the information of a Windows host with the NetApi (RPC over SMB, TCP 445) with the credentials of the
///     calling thread (the user running the application, or a <see cref="WindowsLogon" />):
///     <list type="bullet">
///         <item><c>NetServerGetInfo</c> level 101: role (workstation / server / domain controller), domain member,
///         description - any authenticated user.</item>
///         <item><c>NetWkstaGetInfo</c> level 100: workgroup / NetBIOS domain name.</item>
///         <item><c>NetWkstaUserEnum</c> level 1: logged-on users - admin rights on the host.</item>
///         <item>Remote registry (only if the users could be read = admin rights): build with patch level, display
///         version (e.g. 25H2) and edition - needs the Remote Registry service (disabled by default on clients).</item>
///     </list>
///     The calls are blocking and can't be canceled (SMB / RPC timeouts of 20 s and more): run them on a limited number
///     of threads with a timeout (see <see cref="IPScanner" />).
/// </summary>
public static class WindowsNetApiResolver
{
    private const int NERR_Success = 0;
    private const int ERROR_MORE_DATA = 234;
    private const int MAX_PREFERRED_LENGTH = -1;

    // Server type flags (SV_TYPE_*)
    private const int SV_TYPE_WORKSTATION = 0x00000001;
    private const int SV_TYPE_DOMAIN_CTRL = 0x00000008;
    private const int SV_TYPE_DOMAIN_BAKCTRL = 0x00000010;
    private const int SV_TYPE_DOMAIN_MEMBER = 0x00000100;
    private const int SV_TYPE_SERVER_NT = 0x00008000;

    private const string CurrentVersionKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

    /// <summary>
    ///     Reads the information of a host (blocking).
    /// </summary>
    /// <param name="host">DNS hostname or IP address of the host.</param>
    /// <param name="onNetApiRead">
    ///     Called with the values of the NetApi before the remote registry is read (it can be slow, e.g. a service that
    ///     is started on demand), so a caller with a timeout can keep them; optional.
    /// </param>
    /// <returns>Information, with the Win32 error if the first request failed.</returns>
    public static WindowsNetApiInfo Resolve(string host, Action<WindowsNetApiInfo> onNetApiRead = null)
    {
        var server = $@"\\{ToServerName(host)}";

        var (error, role, isDomainMember, description) = GetServerInfo(server);

        if (error != NERR_Success)
            return WindowsNetApiInfo.FromError(error);

        var workgroup = GetWorkgroup(server);

        var (users, usersError) = GetUsers(server);

        var info = new WindowsNetApiInfo(NERR_Success, role, isDomainMember, description, workgroup, users,
            usersError, null);

        // The registry needs admin rights like the users - not tried without them (one RPC connection less)
        if (usersError != NERR_Success)
            return info;

        onNetApiRead?.Invoke(info);

        var registry = GetRegistryVersion(ToServerName(host));

        return registry == null ? info : info with { Registry = registry };
    }

    /// <summary>
    ///     Converts an IPv6 address to its UNC name (e.g. "fe80--1s4.ipv6-literal.net"), other hosts are returned as
    ///     they are.
    /// </summary>
    /// <param name="host">DNS hostname or IP address.</param>
    /// <returns>Name for UNC paths / NetApi calls.</returns>
    public static string ToServerName(string host)
    {
        if (IPAddress.TryParse(host, out var ipAddress) && ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            return ipAddress.ToString().Replace(':', '-').Replace('%', 's') + ".ipv6-literal.net";

        return host;
    }

    /// <summary>
    ///     Gets the role of a host from its server type flags.
    /// </summary>
    /// <param name="type">Server type (SV_TYPE_* flags).</param>
    /// <returns>Role.</returns>
    public static WindowsHostRole GetRole(int type)
    {
        if ((type & (SV_TYPE_DOMAIN_CTRL | SV_TYPE_DOMAIN_BAKCTRL)) != 0)
            return WindowsHostRole.DomainController;

        if ((type & SV_TYPE_SERVER_NT) != 0)
            return WindowsHostRole.Server;

        return (type & SV_TYPE_WORKSTATION) != 0 ? WindowsHostRole.Workstation : WindowsHostRole.Unknown;
    }

    /// <summary>
    ///     Filters the logged-on users: no computer accounts ("NAME$") and no virtual accounts of the desktop window
    ///     manager / font driver ("DWM-1", "UMFD-0"), each user once ("DOMAIN\user"), sorted.
    /// </summary>
    /// <param name="entries">Username and logon domain of the entries.</param>
    /// <returns>Users.</returns>
    public static List<string> FilterUsers(IEnumerable<(string username, string domain)> entries)
    {
        return entries
            .Where(x => !string.IsNullOrEmpty(x.username) && !x.username.EndsWith('$') &&
                        !x.username.StartsWith("DWM-", StringComparison.OrdinalIgnoreCase) &&
                        !x.username.StartsWith("UMFD-", StringComparison.OrdinalIgnoreCase))
            .Select(x => string.IsNullOrEmpty(x.domain) ? x.username : $@"{x.domain}\{x.username}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    ///     Gets the edition for display, e.g. "Professional" -> "Pro", "ServerDatacenter" -> "Datacenter".
    /// </summary>
    /// <param name="editionID">EditionID of the registry.</param>
    /// <returns>Edition, empty if none.</returns>
    public static string GetEditionName(string editionID)
    {
        if (string.IsNullOrEmpty(editionID))
            return string.Empty;

        return editionID switch
        {
            "Core" or "CoreSingleLanguage" or "CoreCountrySpecific" => "Home",
            "Professional" => "Pro",
            "ProfessionalWorkstation" => "Pro for Workstations",
            "ProfessionalEducation" => "Pro Education",
            "EnterpriseS" => "Enterprise LTSC",
            "IoTEnterpriseS" => "IoT Enterprise LTSC",
            _ when editionID.StartsWith("Server", StringComparison.Ordinal) && editionID.Length > 6 =>
                editionID[6..] switch
                {
                    "StandardEval" => "Standard (Evaluation)",
                    "DatacenterEval" => "Datacenter (Evaluation)",
                    "AzureEdition" => "Datacenter: Azure Edition",
                    var x => x
                },
            _ => editionID
        };
    }

    #region NetApi

    private static (int error, WindowsHostRole role, bool? isDomainMember, string description) GetServerInfo(
        string server)
    {
        var error = NetServerGetInfo(server, 101, out var buffer);

        try
        {
            if (error != NERR_Success)
                return (error, WindowsHostRole.Unknown, null, string.Empty);

            var info = Marshal.PtrToStructure<SERVER_INFO_101>(buffer);

            var role = GetRole(info.sv101_type);

            return (NERR_Success, role,
                role == WindowsHostRole.DomainController || (info.sv101_type & SV_TYPE_DOMAIN_MEMBER) != 0,
                Clean(info.sv101_comment));
        }
        finally
        {
            if (buffer != IntPtr.Zero)
                NetApiBufferFree(buffer);
        }
    }

    private static string GetWorkgroup(string server)
    {
        var error = NetWkstaGetInfo(server, 100, out var buffer);

        try
        {
            return error == NERR_Success
                ? Clean(Marshal.PtrToStructure<WKSTA_INFO_100>(buffer).wki100_langroup)
                : string.Empty;
        }
        finally
        {
            if (buffer != IntPtr.Zero)
                NetApiBufferFree(buffer);
        }
    }

    private static (List<string> users, int error) GetUsers(string server)
    {
        var entries = new List<(string, string)>();
        var resumeHandle = 0;
        int error;

        do
        {
            error = NetWkstaUserEnum(server, 1, out var buffer, MAX_PREFERRED_LENGTH, out var entriesRead, out _,
                ref resumeHandle);

            try
            {
                if (error is not (NERR_Success or ERROR_MORE_DATA))
                    return (null, error);

                var size = Marshal.SizeOf<WKSTA_USER_INFO_1>();

                for (var i = 0; i < entriesRead; i++)
                {
                    var info = Marshal.PtrToStructure<WKSTA_USER_INFO_1>(buffer + i * size);

                    entries.Add((Clean(info.wkui1_username), Clean(info.wkui1_logon_domain)));
                }
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                    NetApiBufferFree(buffer);
            }
        } while (error == ERROR_MORE_DATA);

        return (FilterUsers(entries), NERR_Success);
    }

    #endregion

    #region Registry

    /// <summary>
    ///     Reads the version from the remote registry, null if it can't be read (no admin rights, service stopped /
    ///     disabled).
    /// </summary>
    private static WindowsRegistryVersion GetRegistryVersion(string server)
    {
        try
        {
            using var baseKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, server);
            using var key = baseKey.OpenSubKey(CurrentVersionKey);

            if (key == null || !int.TryParse(key.GetValue("CurrentBuild") as string ?? key.GetValue("CurrentBuildNumber") as string,
                    out var build))
                return null;

            // Windows 10 and later: CurrentMajor/MinorVersionNumber (CurrentVersion is "6.3" for compatibility)
            int major, minor;

            if (key.GetValue("CurrentMajorVersionNumber") is int majorValue &&
                key.GetValue("CurrentMinorVersionNumber") is int minorValue)
            {
                major = majorValue;
                minor = minorValue;
            }
            else if (Version.TryParse(key.GetValue("CurrentVersion") as string, out var currentVersion))
            {
                major = currentVersion.Major;
                minor = currentVersion.Minor;
            }
            else
            {
                return null;
            }

            var ubr = key.GetValue("UBR") is int ubrValue ? ubrValue : 0;

            var installationType = key.GetValue("InstallationType") as string;

            return new WindowsRegistryVersion(new Version(major, minor, build, ubr),
                Clean(key.GetValue("DisplayVersion") as string), Clean(key.GetValue("EditionID") as string),
                string.IsNullOrEmpty(installationType)
                    ? null
                    : installationType.StartsWith("Server", StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception)
        {
            return null;
        }
    }

    #endregion

    /// <summary>
    ///     Trims a value and removes control characters, empty for null.
    /// </summary>
    private static string Clean(string value)
    {
        return StringHelper.RemoveControlCharacters(value).Trim();
    }

    #region P/Invoke

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SERVER_INFO_101
    {
        public int sv101_platform_id;
        [MarshalAs(UnmanagedType.LPWStr)] public string sv101_name;
        public int sv101_version_major;
        public int sv101_version_minor;
        public int sv101_type;
        [MarshalAs(UnmanagedType.LPWStr)] public string sv101_comment;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WKSTA_INFO_100
    {
        public int wki100_platform_id;
        [MarshalAs(UnmanagedType.LPWStr)] public string wki100_computername;
        [MarshalAs(UnmanagedType.LPWStr)] public string wki100_langroup;
        public int wki100_ver_major;
        public int wki100_ver_minor;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WKSTA_USER_INFO_1
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string wkui1_username;
        [MarshalAs(UnmanagedType.LPWStr)] public string wkui1_logon_domain;
        [MarshalAs(UnmanagedType.LPWStr)] public string wkui1_oth_domains;
        [MarshalAs(UnmanagedType.LPWStr)] public string wkui1_logon_server;
    }

    [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
    private static extern int NetServerGetInfo(string servername, int level, out IntPtr bufptr);

    [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
    private static extern int NetWkstaGetInfo(string servername, int level, out IntPtr bufptr);

    [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
    private static extern int NetWkstaUserEnum(string servername, int level, out IntPtr bufptr, int prefmaxlen,
        out int entriesread, out int totalentries, ref int resumeHandle);

    [DllImport("netapi32.dll")]
    private static extern int NetApiBufferFree(IntPtr buffer);

    #endregion
}
