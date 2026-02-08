using System.Text.Json.Serialization;

namespace NETworkManager.Settings;

/// <summary>
///     Class that represents system-wide policies that override user settings.
///     This configuration is loaded from a config.json file in the application directory.
/// </summary>
public class PolicyInfo
{
    [JsonPropertyName("Update_CheckForUpdatesAtStartup")]
    public bool? Update_CheckForUpdatesAtStartup { get; set; }
}
