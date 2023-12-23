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
    public static List<ResourceInfo> List => new()
    {
        new ResourceInfo("Organizationally unique identifier", "https://standards-oui.ieee.org/oui/oui.txt",
            Strings.Resource_OUI_Description),
        new ResourceInfo("Service names and port numbers",
            "https://www.iana.org/assignments/service-names-port-numbers/service-names-port-numbers.xhtml",
            Strings.Resource_ServiceNamePortNumber_Description),
        new ResourceInfo("flag-icon-css", "https://github.com/lipis/flag-icon-css",
            Strings.Resource_Flag_Description),
        new ResourceInfo("List of Top-Level-Domains", "https://data.iana.org/TLD/tlds-alpha-by-domain.txt",
            Strings.Resource_ListTLD_Description)
    };
}