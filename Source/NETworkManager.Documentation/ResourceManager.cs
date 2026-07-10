using System.Collections.Generic;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Documentation;

/// <summary>
///     This class provides information's about resources used within the program.
/// </summary>
public static class ResourceManager
{
    /// <summary>
    ///     Static list with all resources that are used.
    /// </summary>
    public static List<ResourceInfo> List =>
    [
        new("Organizationally unique identifier", "https://standards-oui.ieee.org/oui/oui.txt",
            Strings.Resource_OUI_Description),

        new("Service names and port numbers",
            "https://www.iana.org/assignments/service-names-port-numbers/service-names-port-numbers.xhtml",
            Strings.Resource_ServiceNamePortNumber_Description),

        new("flag-icon-css", "https://github.com/lipis/flag-icon-css",
            Strings.Resource_Flag_Description),

        new("List of Top-Level-Domains", "https://data.iana.org/TLD/tlds-alpha-by-domain.txt",
            Strings.Resource_ListTLD_Description),

        new("natural-earth-geojson", "https://github.com/martynafford/natural-earth-geojson",
            Strings.Resource_NaturalEarth_Description)
    ];
}