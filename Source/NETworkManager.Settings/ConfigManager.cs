using log4net;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NETworkManager.Settings;

/// <summary>
///     Manager for system-wide configuration that is loaded from a config.json file 
///     in the application directory. This configuration overrides user settings.
/// </summary>
public static class ConfigManager
{
    #region Variables

    /// <summary>
    ///     Logger for logging.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigManager));

    /// <summary>
    ///     Config file name.
    /// </summary>
    private static string ConfigFileName => "config.json";

    /// <summary>
    ///     System-wide configuration that is currently loaded.
    /// </summary>
    public static ConfigInfo Current { get; private set; }

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
    ///     Method to load the system-wide configuration from config.json file in the application directory.
    /// </summary>
    public static void Load()
    {
        var filePath = GetConfigFilePath();

        // Check if config file exists
        if (File.Exists(filePath))
        {
            try
            {
                Log.Info($"Loading system-wide configuration from: {filePath}");
                
                var jsonString = File.ReadAllText(filePath);
                Current = JsonSerializer.Deserialize<ConfigInfo>(jsonString, JsonOptions);
                
                Log.Info("System-wide configuration loaded successfully.");
                
                // Log enabled settings
                if (Current.Update_DisableUpdateCheck.HasValue)
                {
                    Log.Info($"System-wide setting - Update_DisableUpdateCheck: {Current.Update_DisableUpdateCheck.Value}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load system-wide configuration from: {filePath}", ex);
                Current = new ConfigInfo();
            }
        }
        else
        {
            Log.Debug($"No system-wide configuration file found at: {filePath}");
            Current = new ConfigInfo();
        }
    }

    #endregion
}
