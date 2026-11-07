using System;
using System.Collections.Generic;

namespace NETworkManager.Models.Network;

/// <summary>
///     Name of a Windows version from its version number (major.minor.build), e.g. 10.0.26200 -> "Windows 11 25H2".
///     Windows 10 / 11 and Windows Server 2016 and later all report 10.0, only the build tells them apart. Some builds
///     are shared by a client and a server version (e.g. 26100: Windows 11 24H2 / Server 2025) - the role of the host
///     decides, if known.
/// </summary>
public static class WindowsVersionHelper
{
    /// <summary>
    ///     Builds of Windows 10 / 11 / Server 2016 and later: client name, server name (null if there is only one of
    ///     them).
    /// </summary>
    private static readonly Dictionary<int, (string Client, string Server)> Builds = new()
    {
        { 10240, ("Windows 10 1507", null) },
        { 10586, ("Windows 10 1511", null) },
        { 14393, ("Windows 10 1607", "Windows Server 2016") },
        { 15063, ("Windows 10 1703", null) },
        { 16299, ("Windows 10 1709", "Windows Server 1709") },
        { 17134, ("Windows 10 1803", "Windows Server 1803") },
        { 17763, ("Windows 10 1809", "Windows Server 2019") },
        { 18362, ("Windows 10 1903", "Windows Server 1903") },
        { 18363, ("Windows 10 1909", "Windows Server 1909") },
        { 19041, ("Windows 10 2004", "Windows Server 2004") },
        { 19042, ("Windows 10 20H2", "Windows Server 20H2") },
        { 19043, ("Windows 10 21H1", null) },
        { 19044, ("Windows 10 21H2", null) },
        { 19045, ("Windows 10 22H2", null) },
        { 20348, (null, "Windows Server 2022") },
        { 22000, ("Windows 11 21H2", null) },
        { 22621, ("Windows 11 22H2", null) },
        { 22631, ("Windows 11 23H2", null) },
        { 25398, (null, "Windows Server 23H2") },
        { 26100, ("Windows 11 24H2", "Windows Server 2025") },
        { 26200, ("Windows 11 25H2", null) }
    };

    /// <summary>
    ///     Gets the name of a Windows version.
    /// </summary>
    /// <param name="version">Version (major.minor.build).</param>
    /// <param name="isServer">
    ///     The host is a server (incl. domain controller), false for a workstation, null if unknown (a build of a client
    ///     and a server version gets both names).
    /// </param>
    /// <returns>Name, null if the version is not a known Windows version (e.g. Samba, which reports build 0).</returns>
    public static string GetName(Version version, bool? isServer)
    {
        // Samba and other SMB servers report a Windows version without build
        if (version == null || version.Build <= 0)
            return null;

        if (version.Major == 10 && version.Minor == 0)
        {
            if (Builds.TryGetValue(version.Build, out var names))
                return Choose(names.Client, names.Server, isServer);

            // Unknown build (e.g. a newer release): Windows 11 from build 22000 for a workstation - not if the role is
            // unknown (usual without credentials), it could be a newer Windows Server
            return version.Build >= 22000 && isServer == false
                ? $"Windows 11 (build {version.Build})"
                : $"Windows (build {version.Build})";
        }

        // Windows Vista / Server 2008 and later: earlier versions (XP, Server 2003) only know SMB1, the version is read
        // over SMB2 only
        return (version.Major, version.Minor) switch
        {
            (6, 0) => Choose("Windows Vista", "Windows Server 2008", isServer),
            (6, 1) => Choose("Windows 7", "Windows Server 2008 R2", isServer),
            (6, 2) => Choose("Windows 8", "Windows Server 2012", isServer),
            (6, 3) => Choose("Windows 8.1", "Windows Server 2012 R2", isServer),
            _ => null
        };
    }

    /// <summary>
    ///     Chooses the client or server name by the role, both if the role is unknown.
    /// </summary>
    private static string Choose(string client, string server, bool? isServer)
    {
        if (client == null)
            return server;

        if (server == null)
            return client;

        return isServer switch
        {
            true => server,
            false => client,
            null => $"{client} / {server.Replace("Windows ", string.Empty)}"
        };
    }
}
