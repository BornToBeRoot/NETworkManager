using System.Collections.Generic;

namespace NETworkManager.Models.Documentation
{
    public static class ResourceManager
    {
        public static List<ResourceInfo> List => new List<ResourceInfo>
        {
            new ResourceInfo("Organizationally unique identifier", "https://linuxnet.ca/ieee/oui/", "Sanitized IEEE OUI Data from linuxnet.ca"),
            new ResourceInfo("Service names and port numbers", "https://www.iana.org/assignments/service-names-port-numbers/service-names-port-numbers.xhtml", "Service Name and Transport Protocol Port Number Registry from iana.org"),
            new ResourceInfo("flag-icon-css","https://github.com/lipis/flag-icon-css","A collection of all country flags in SVG")
        };
    }
}
