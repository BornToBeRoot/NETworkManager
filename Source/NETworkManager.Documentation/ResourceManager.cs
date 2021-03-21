using System.Collections.Generic;

namespace NETworkManager.Documentation
{
    /// <summary>
    /// This class provides information about resources used within the program.
    /// </summary>
    public static class ResourceManager
    {
        /// <summary>
        /// Static list with all resources that are used.
        /// </summary>
        public static List<ResourceInfo> List => new List<ResourceInfo>
        {
            new ResourceInfo("Organizationally unique identifier", "https://linuxnet.ca/ieee/oui/", string.Format(Localization.Resources.Strings.Resource_OUI_Description, "linuxnet.ca")),
            new ResourceInfo("Service names and port numbers", "https://www.iana.org/assignments/service-names-port-numbers/service-names-port-numbers.xhtml", string.Format(Localization.Resources.Strings.Resource_ServiceNamePortNumber_Description, "iana.org")),
            new ResourceInfo("flag-icon-css","https://github.com/lipis/flag-icon-css", Localization.Resources.Strings.Resource_Flag_Description),
            new ResourceInfo("List of Top-Level-Domains", "https://data.iana.org/TLD/tlds-alpha-by-domain.txt", string.Format(Localization.Resources.Strings.Resource_ListTLD_Description,"iana.org", "whois.iana.org"))
        };
    }
}
