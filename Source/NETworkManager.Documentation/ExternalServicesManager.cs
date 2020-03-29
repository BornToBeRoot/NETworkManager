using System.Collections.Generic;

namespace NETworkManager.Documentation
{
    public static class ExternalServicesManager
    {
        public static List<ExternalServicesInfo> List => new List<ExternalServicesInfo>
        {
            new ExternalServicesInfo("ipify", "https://www.ipify.org/", "A Simple Public IP Address API")
        };
    }
}
