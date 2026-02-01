using System.Text.Json.Serialization;

namespace NETworkManager.Settings;

/// <summary>
///     Class that represents system-wide configuration that overrides user settings.
///     This configuration is loaded from a config.json file in the application directory.
/// </summary>
public class ConfigInfo
{
    /// <summary>
    ///     Disable update check for all users. When set to true, the application will not check for updates at startup.
    ///     This overrides the user's "Update_CheckForUpdatesAtStartup" setting.
    /// </summary>
    [JsonPropertyName("Update_DisableUpdateCheck")]
    public bool? Update_DisableUpdateCheck { get; set; }
}
