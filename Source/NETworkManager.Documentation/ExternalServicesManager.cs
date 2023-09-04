using System.Collections.Generic;

namespace NETworkManager.Documentation;

/// <summary>
/// This class provides information's about external services used within the program.
/// </summary>
public static class ExternalServicesManager
{
    /// <summary>
    /// Static list with all external services that are used.
    /// </summary>
    public static List<ExternalServicesInfo> List => new()
    {
        new ExternalServicesInfo("ip-api", "https://ip-api.com/", Localization.Resources.Strings.ExternalService_ip_api_Description),
        new ExternalServicesInfo("ipify", "https://www.ipify.org/", Localization.Resources.Strings.ExternalService_ipify_Description),
    };
}
