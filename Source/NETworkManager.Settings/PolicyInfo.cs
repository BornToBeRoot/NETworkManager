using System.Text.Json.Serialization;

namespace NETworkManager.Settings;

/// <summary>
///     Class that represents system-wide policies that override user settings.
///     This configuration is loaded from a config.json file in the application directory.
/// </summary>
public class PolicyInfo
{
    /// <summary>
    ///     Controls the "Check for updates at startup" setting for all users.
    ///     When set, this value overrides the user's "Update_CheckForUpdatesAtStartup" setting.
    ///     Set to true to enable update checks, false to disable them.
    /// </summary>
    [JsonPropertyName("Update_CheckForUpdatesAtStartup")]
    public bool? Update_CheckForUpdatesAtStartup { get; set; }
}
