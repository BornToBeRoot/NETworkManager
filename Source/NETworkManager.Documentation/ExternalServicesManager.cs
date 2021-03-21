using System.Collections.Generic;

namespace NETworkManager.Documentation
{
    /// <summary>
    /// This class provides information about external services used within the program.
    /// </summary>
    public static class ExternalServicesManager
    {
        /// <summary>
        /// Static list with all external services that are used.
        /// </summary>
        public static List<ExternalServicesInfo> List => new List<ExternalServicesInfo>
        {
            new ExternalServicesInfo("ipify", "https://www.ipify.org/", Localization.Resources.Strings.ExternalService_ipify_Description)
        };
    }
}
