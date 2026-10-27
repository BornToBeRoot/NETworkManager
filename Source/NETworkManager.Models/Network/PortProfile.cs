using System;
using System.Collections.Generic;
using System.Linq;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

public static class PortProfile
{
    /// <summary>
    ///     Separator of the entries of a ports input (e.g. "Webserver; 8080").
    /// </summary>
    private const char EntrySeparator = ';';

    /// <summary>
    ///     Finds a port profile by its name (case-insensitive, leading / trailing spaces are ignored).
    /// </summary>
    /// <param name="portProfiles">Port profiles.</param>
    /// <param name="name">Name of the port profile.</param>
    /// <returns>Port profile, or null if there is none with the name.</returns>
    public static PortProfileInfo FindByName(IEnumerable<PortProfileInfo> portProfiles, string name)
    {
        var trimmedName = name?.Trim();

        return string.IsNullOrEmpty(trimmedName)
            ? null
            : portProfiles.FirstOrDefault(x =>
                string.Equals(x.Name?.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Checks if a name can be used for a port profile: the name can be entered in a ports input, so it must not
    ///     contain the entry separator ";" and must not be a port or port range (e.g. "80" would be read as port).
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <returns>True if the name can be used.</returns>
    public static bool IsValidName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && !name.Contains(EntrySeparator) &&
               !PortRangeHelper.IsValidPortOrPortRange(name);
    }

    /// <summary>
    ///     Resolves a ports input into ports. Every entry (separated by ";") is a port (e.g. "80"), a port range (e.g.
    ///     "1-1024") or the name of a port profile (e.g. "Webserver"), which is replaced by its ports - e.g.
    ///     "Webserver; 8080" -> "80; 443; 8080".
    /// </summary>
    /// <param name="input">Ports input.</param>
    /// <param name="portProfiles">Port profiles that can be used by their name.</param>
    /// <param name="ports">Ports (ports and port ranges separated by ";"), null if the input is not valid.</param>
    /// <returns>True if every entry is a port, port range or port profile.</returns>
    public static bool TryResolvePorts(string input, IEnumerable<PortProfileInfo> portProfiles, out string ports)
    {
        ports = null;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        List<string> resolvedEntries = [];

        foreach (var entry in input.Split(EntrySeparator))
        {
            if (PortRangeHelper.IsValidPortOrPortRange(entry))
            {
                resolvedEntries.Add(entry.Replace(" ", ""));
                continue;
            }

            var portProfile = FindByName(portProfiles, entry);

            if (portProfile == null)
                return false;

            resolvedEntries.Add(portProfile.Ports);
        }

        ports = string.Join("; ", resolvedEntries);

        return true;
    }

    public static List<PortProfileInfo> GetDefaultList()
    {
        return new List<PortProfileInfo>
        {
            new("DNS (via TCP)", "53"),
            new("NTP (via TCP)", "123"),
            new("Webserver", "80; 443"),
            new("Webserver (Other)", "80; 443; 8080; 8443"),
            new("Remote access", "22; 23; 3389; 5900"),
            new("Mailserver", "25; 110; 143; 465; 587; 993; 995"),
            new("Filetransfer", "20-21; 22; 989-990; 2049"),
            new("Database", "1433-1434; 1521; 1830; 3306; 5432"),
            new("SMB", "139; 445"),
            new("LDAP", "389; 636"),
            new("HTTP proxy", "3128"),
            new("Well-known ports", "1-1024")
        };
    }
}