using System.Collections.Generic;
using NETworkManager.Localization;

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
