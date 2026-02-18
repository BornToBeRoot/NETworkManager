using log4net;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NETworkManager.Settings;

/// <summary>
///     Manages local application settings that are stored outside the main settings file.
///     This is used for settings that control where the main settings file is located.
/// </summary>
public static class LocalSettingsManager
{
    #region Variables

    /// <summary>
    ///     Logger for logging.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(LocalSettingsManager));

    /// <summary>
    ///     Settings file name.
    /// </summary>
    private static string SettingsFileName => "Settings.json";

    /// <summary>
    ///     Settings that are currently loaded.
    /// </summary>
    public static LocalSettingsInfo Current { get; private set; }

    /// <summary>
    ///     JSON serializer options for consistent serialization/deserialization.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Converters = { new JsonStringEnumConverter() }
    };
    #endregion

    #region Methods

    /// <summary>
    ///     Method to get the path of the settings folder.
    /// </summary>
    /// <returns>Path to the settings folder.</returns>
    private static string GetSettingsFolderLocation()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AssemblyManager.Current.Name);
    }

    /// <summary>
    ///     Method to get the settings file path
    /// </summary>
    /// <returns>Settings file path.</returns>
    private static string GetSettingsFilePath()
    {
        return Path.Combine(
            GetSettingsFolderLocation(),
            SettingsFileName);
    }

    /// <summary>
    ///     Initialize new settings (<see cref="SettingsInfo" />) and save them (to a file).
    /// </summary>
    private static void Initialize()
    {
        Log.Info("Initializing new local settings.");

        Current = new LocalSettingsInfo();

        Save();
    }

    /// <summary>
    ///     Method to load the settings from a file.
    /// </summary>
    public static void Load()
    {
        var filePath = GetSettingsFilePath();

        if (File.Exists(filePath))
        {
            try
            {
                Log.Info($"Loading local settings from: {filePath}");

                var jsonString = File.ReadAllText(filePath);

                // Treat empty or JSON "null" as "no settings" instead of crashing
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    Log.Info("Local settings file is empty, initializing new local settings.");
                }
                else
                {
                    Current = JsonSerializer.Deserialize<LocalSettingsInfo>(jsonString, JsonOptions);

                    // Check if deserialization returned null (e.g., file contains "null")
                    if (Current == null)
                    {
                        Log.Warn("Local settings deserialized to null, initializing new local settings.");
                    }
                    else
                    {
                        Log.Info("Local settings loaded successfully.");

                        // Reset change tracking
                        Current.SettingsChanged = false;

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                {
                    Log.Error($"Failed to load local settings from: {filePath}", ex);
                }
            }
        }

        // Initialize new settings if loading failed or file does not exist
        Initialize();
    }

    /// <summary>
    ///     Method to save the current settings to a file.
    /// </summary>
    public static void Save()
    {
        // Create the directory if it does not exist
        Directory.CreateDirectory(GetSettingsFolderLocation());

        // Serialize to file
        var filePath = GetSettingsFilePath();

        var jsonString = JsonSerializer.Serialize(Current, JsonOptions);
        File.WriteAllText(filePath, jsonString);

        Log.Info($"Local settings saved to {filePath}");

        // Reset change tracking
        Current.SettingsChanged = false;
    }
    #endregion
}
