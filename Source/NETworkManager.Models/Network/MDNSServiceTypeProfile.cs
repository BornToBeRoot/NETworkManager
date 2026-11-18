using System;
using System.Collections.Generic;
using System.Linq;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Service type profiles of the mDNS / DNS-SD tool and the service type input (e.g. "Printers; _http._tcp"), like
///     the port profiles of the Port Scanner (see <see cref="PortProfile" />).
/// </summary>
public static class MDNSServiceTypeProfile
{
    /// <summary>
    ///     Separator of the entries of a service type input (e.g. "Printers; _http._tcp").
    /// </summary>
    private const char EntrySeparator = NamedReferenceInput.EntrySeparator;

    /// <summary>
    ///     Finds a service type profile by its name (case-insensitive, leading / trailing spaces are ignored).
    /// </summary>
    /// <param name="profiles">Service type profiles.</param>
    /// <param name="name">Name of the service type profile.</param>
    /// <returns>Service type profile, or null if there is none with the name.</returns>
    public static MDNSServiceTypeProfileInfo FindByName(IEnumerable<MDNSServiceTypeProfileInfo> profiles, string name)
    {
        return NamedReferenceInput.FindByName(profiles, name);
    }

    /// <summary>
    ///     Checks if a name can be used for a service type profile: the name can be entered in the service type input,
    ///     so it must not contain the entry separator ";" and must not be "*" or a service type (e.g. "_ipp._tcp").
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <returns>True if the name can be used.</returns>
    public static bool IsValidName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && !name.Contains(EntrySeparator) &&
               !MDNSServiceType.TryNormalize(name, out _);
    }

    /// <summary>
    ///     Parses the service types of a service type profile (e.g. "_ipp._tcp; _ipps._tcp"): service types separated
    ///     by ";", no "*" (all service types) and no names of other profiles.
    /// </summary>
    /// <param name="input">Service types of the profile.</param>
    /// <param name="serviceTypes">Normalized service types (e.g. "_ipp._tcp.local."), null if the input is not valid.</param>
    /// <returns>True if every entry is a service type.</returns>
    public static bool TryParseServiceTypes(string input, out IReadOnlyList<string> serviceTypes)
    {
        serviceTypes = null;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        List<string> result = [];

        foreach (var entry in input.Split(EntrySeparator))
        {
            if (!MDNSServiceType.TryNormalize(entry, out var serviceType) || serviceType == "*")
                return false;

            if (!result.Contains(serviceType))
                result.Add(serviceType);
        }

        serviceTypes = result;

        return true;
    }

    /// <summary>
    ///     Resolves a service type input into the service types to browse. Every entry (separated by ";") is "*" (all
    ///     service types), a service type (e.g. "_http._tcp") or the name of a service type profile (e.g. "Printers"),
    ///     which is replaced by its service types - e.g. "Printers; _http._tcp" -> "_ipp._tcp.local.", ...,
    ///     "_http._tcp.local.". With "*" only "*" is returned (it includes the others).
    /// </summary>
    /// <param name="input">Service type input.</param>
    /// <param name="profiles">Service type profiles that can be used by their name.</param>
    /// <param name="serviceTypes">Normalized service types without duplicates, null if the input is not valid.</param>
    /// <returns>True if every entry is "*", a service type or a service type profile.</returns>
    public static bool TryResolve(string input, IEnumerable<MDNSServiceTypeProfileInfo> profiles,
        out IReadOnlyList<string> serviceTypes)
    {
        serviceTypes = null;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var profileList = profiles?.ToList() ?? [];
        List<string> result = [];

        foreach (var entry in input.Split(EntrySeparator))
        {
            if (MDNSServiceType.TryNormalize(entry, out var serviceType))
            {
                if (!result.Contains(serviceType))
                    result.Add(serviceType);

                continue;
            }

            var profile = FindByName(profileList, entry);

            // A profile edited in the settings file may contain invalid service types
            if (profile == null || !TryParseServiceTypes(profile.ServiceTypes, out var profileServiceTypes))
                return false;

            foreach (var profileServiceType in profileServiceTypes.Where(x => !result.Contains(x)))
                result.Add(profileServiceType);
        }

        serviceTypes = result.Contains("*") ? ["*"] : result;

        return true;
    }

    /// <summary>
    ///     Gets the default service type profiles (new installation, added on upgrade if missing).
    /// </summary>
    public static List<MDNSServiceTypeProfileInfo> GetDefaultList()
    {
        return
        [
            new("Printers", "_ipp._tcp; _ipps._tcp; _printer._tcp; _pdl-datastream._tcp"),
            new("Scanners", "_uscan._tcp; _uscans._tcp; _scanner._tcp"),
            new("Web", "_http._tcp; _https._tcp"),
            new("File sharing", "_smb._tcp; _afpovertcp._tcp; _nfs._tcp; _ftp._tcp; _webdav._tcp; _webdavs._tcp"),
            new("Remote access", "_ssh._tcp; _sftp-ssh._tcp; _rfb._tcp; _rdp._tcp; _telnet._tcp"),
            new("Media", "_airplay._tcp; _raop._tcp; _googlecast._tcp; _spotify-connect._tcp; _sonos._tcp"),
            new("Smart home", "_hap._tcp; _hap._udp; _matter._tcp; _matterc._udp; _home-assistant._tcp; _hue._tcp"),
            new("Apple devices", "_companion-link._tcp; _device-info._tcp; _airplay._tcp; _apple-mobdev2._tcp")
        ];
    }
}
