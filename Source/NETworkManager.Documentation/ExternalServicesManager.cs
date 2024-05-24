using System.Collections.Generic;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Documentation;

/// <summary>
///     This class provides information's about external services used within the program.
/// </summary>
public static class ExternalServicesManager
{
    /// <summary>
    ///     Static list with all external services that are used.
    /// </summary>
    public static List<ExternalServicesInfo> List => new()
    {
        new ExternalServicesInfo("ip-api.com", "https://ip-api.com/",
            Strings.ExternalService_ip_api_Description),
        new ExternalServicesInfo("ipify.org", "https://www.ipify.org/",
            Strings.ExternalService_ipify_Description)
    };
}