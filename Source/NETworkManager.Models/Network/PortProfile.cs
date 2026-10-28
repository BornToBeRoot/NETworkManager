using System.Collections.Generic;
using System.Linq;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

public static class PortProfile
{
    /// <summary>
    ///     Separator of the entries of a ports input (e.g. "Webserver; 8080").
    /// </summary>
    private const char EntrySeparator = NamedReferenceInput.EntrySeparator;

    /// <summary>
    ///     Finds a port profile by its name (case-insensitive, leading / trailing spaces are ignored).
    /// </summary>
    /// <param name="portProfiles">Port profiles.</param>
    /// <param name="name">Name of the port profile.</param>
    /// <returns>Port profile, or null if there is none with the name.</returns>
    public static PortProfileInfo FindByName(IEnumerable<PortProfileInfo> portProfiles, string name)
    {
        return NamedReferenceInput.FindByName(portProfiles, name);
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

    /// <summary>
    ///     Replaces the names of port profiles in a ports input by references to their ID (e.g. "Webserver; 8080" ->
    ///     "{8f1c...}; 8080"), so a profile still finds the port profile after it was renamed. Entries that are no
    ///     port profile (ports, unknown names) are kept.
    /// </summary>
    /// <param name="input">Ports input (see <see cref="TryResolvePorts" />).</param>
    /// <param name="portProfiles">Port profiles.</param>
    /// <returns>Input with references, null if it contains no port profile.</returns>
    public static string EncodeReferences(string input, IEnumerable<PortProfileInfo> portProfiles)
    {
        var portProfileList = portProfiles.ToList();

        // A port or port range is never a port profile (see TryResolvePorts)
        return NamedReferenceInput.EncodeReferences(input,
            entry => PortRangeHelper.IsValidPortOrPortRange(entry) ? null : FindByName(portProfileList, entry));
    }

    /// <summary>
    ///     Replaces the references in a ports input (see <see cref="EncodeReferences" />) by the current names of the
    ///     port profiles (e.g. "{8f1c...}; 8080" -> "Webserver; 8080").
    /// </summary>
    /// <param name="inputWithReferences">Ports input with references.</param>
    /// <param name="portProfiles">Port profiles.</param>
    /// <param name="input">Ports input with the names of the port profiles, null if a port profile was not found.</param>
    /// <returns>True if every referenced port profile was found.</returns>
    public static bool TryDecodeReferences(string inputWithReferences, IEnumerable<PortProfileInfo> portProfiles,
        out string input)
    {
        return NamedReferenceInput.TryDecodeReferences(inputWithReferences, portProfiles, out input);
    }

    /// <summary>
    ///     Converts a ports input (e.g. of a profile or group) to the values that are saved: the ports (port profiles
    ///     replaced by their ports, used if a port profile can't be found later, e.g. a profile file from another
    ///     computer) and the input with references to the port profiles (see <see cref="EncodeReferences" />).
    /// </summary>
    /// <param name="input">Ports input.</param>
    /// <param name="portProfiles">Port profiles.</param>
    /// <returns>Ports and ports input with references (null if it contains no port profile).</returns>
    public static (string Ports, string PortsInput) Encode(string input, IEnumerable<PortProfileInfo> portProfiles)
    {
        var portProfileList = portProfiles.ToList();

        return (TryResolvePorts(input, portProfileList, out var ports) ? ports : input?.Trim(),
            EncodeReferences(input, portProfileList));
    }

    /// <summary>
    ///     Converts the saved values (see <see cref="Encode" />) back to the ports input: the port profiles with their
    ///     current name (also after a rename), or the saved ports if a port profile can't be found.
    /// </summary>
    /// <param name="ports">Saved ports.</param>
    /// <param name="portsInput">Saved ports input with references.</param>
    /// <param name="portProfiles">Port profiles.</param>
    /// <param name="portProfileNotFound">True if a port profile can't be found.</param>
    /// <returns>Ports input.</returns>
    public static string Decode(string ports, string portsInput, IEnumerable<PortProfileInfo> portProfiles,
        out bool portProfileNotFound)
    {
        portProfileNotFound = false;

        if (string.IsNullOrEmpty(portsInput))
            return ports;

        if (TryDecodeReferences(portsInput, portProfiles, out var input))
            return input;

        portProfileNotFound = true;

        return ports;
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