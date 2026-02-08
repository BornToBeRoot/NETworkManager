using log4net;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NETworkManager.Settings;

/// <summary>
///     Manager for system-wide policies that are loaded from a config.json file 
///     in the application directory. These policies override user settings.
/// </summary>
public static class PolicyManager
{
    #region Variables

    /// <summary>
    ///     Logger for logging.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(PolicyManager));

    /// <summary>
    ///     Config file name.
    /// </summary>
    private static string ConfigFileName => "config.json";

    /// <summary>
    ///     System-wide policies that are currently loaded.
    /// </summary>
    public static PolicyInfo Current { get; private set; }

    /// <summary>
    ///     JSON serializer options for consistent serialization/deserialization.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    #endregion

    #region Methods

    /// <summary>
    ///     Method to get the config file path in the application directory.
    /// </summary>
    /// <returns>Config file path.</returns>
    private static string GetConfigFilePath()
    {
        return Path.Combine(AssemblyManager.Current.Location, ConfigFileName);
    }

    /// <summary>
    ///     Method to load the system-wide policies from config.json file in the application directory.
    /// </summary>
    public static void Load()
    {
        var filePath = GetConfigFilePath();

        // Check if config file exists
        if (File.Exists(filePath))
        {
            try
            {
                Log.Info($"Loading system-wide policies from: {filePath}");

                var jsonString = File.ReadAllText(filePath);
                Current = JsonSerializer.Deserialize<PolicyInfo>(jsonString, JsonOptions);

                Log.Info("System-wide policies loaded successfully.");

                // Log enabled settings
                Log.Info($"System-wide policy - Update_CheckForUpdatesAtStartup: {Current.Update_CheckForUpdatesAtStartup?.ToString() ?? "Not set"}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load system-wide policies from: {filePath}", ex);
                Current = new PolicyInfo();
            }
        }
        else
        {
            Log.Debug($"No system-wide policy file found at: {filePath}");
            Current = new PolicyInfo();
        }
    }

    #endregion
}
