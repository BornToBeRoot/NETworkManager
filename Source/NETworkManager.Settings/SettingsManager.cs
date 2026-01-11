using log4net;
using NETworkManager.Models;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace NETworkManager.Settings;

public static class SettingsManager
{
    #region Variables

    /// <summary>
    ///     Logger for logging.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(SettingsManager));

    /// <summary>
    ///     Settings directory name.
    /// </summary>
    private static string SettingsFolderName => "Settings";

    /// <summary>
    ///     Settings backups directory name.
    /// </summary>
    private static string BackupFolderName => "Backups";

    /// <summary>
    ///     Settings file name.
    /// </summary>
    private static string SettingsFileName => "Settings";

    /// <summary>
    ///     Settings file extension.
    /// </summary>
    private static string SettingsFileExtension => ".json";

    /// <summary>
    ///     Legacy XML settings file extension.
    /// </summary>
    [Obsolete("Legacy XML settings are no longer used, but the extension is kept for migration purposes.")]
    private static string LegacySettingsFileExtension => ".xml";

    /// <summary>
    ///     Settings that are currently loaded.
    /// </summary>
    public static SettingsInfo Current { get; private set; }

    /// <summary>
    ///     Indicates if the HotKeys have changed. May need to be reworked if we add more HotKeys.
    /// </summary>
    public static bool HotKeysChanged { get; set; }

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

    #region Settings location, default paths and file names

    /// <summary>
    ///     Method to get the path of the settings folder.
    /// </summary>
    /// <returns>Path to the settings folder.</returns>
    public static string GetSettingsFolderLocation()
    {
        return ConfigurationManager.Current.IsPortable
            ? Path.Combine(AssemblyManager.Current.Location, SettingsFolderName)
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                AssemblyManager.Current.Name, SettingsFolderName);
    }

    /// <summary>
    ///     Method to get the path of the settings backup folder.
    /// </summary>
    /// <returns>Path to the settings backup folder.</returns>
    public static string GetSettingsBackupFolderLocation()
    {
        return Path.Combine(GetSettingsFolderLocation(), BackupFolderName);
    }

    /// <summary>
    ///     Method to get the settings file name.
    /// </summary>
    /// <returns>Settings file name.</returns>
    public static string GetSettingsFileName()
    {
        return $"{SettingsFileName}{SettingsFileExtension}";
    }

    /// <summary>
    ///     Method to get the legacy settings file name.
    /// </summary>
    /// <returns>Legacy settings file name.</returns>
    [Obsolete("Legacy XML settings are no longer used, but the method is kept for migration purposes.")]
    public static string GetLegacySettingsFileName()
    {
        return $"{SettingsFileName}{LegacySettingsFileExtension}";
    }

    /// <summary>
    ///     Method to get the settings file path.
    /// </summary>
    /// <returns>Settings file path.</returns>
    public static string GetSettingsFilePath()
    {
        return Path.Combine(GetSettingsFolderLocation(), GetSettingsFileName());
    }

    /// <summary>
    ///     Method to get the legacy XML settings file path.
    /// </summary>
    /// <returns>Legacy XML settings file path.</returns>
    [Obsolete("Legacy XML settings are no longer used, but the method is kept for migration purposes.")]
    private static string GetLegacySettingsFilePath()
    {
        return Path.Combine(GetSettingsFolderLocation(), GetLegacySettingsFileName());
    }

    #endregion

    #region Initialize, load and save

    /// <summary>
    ///     Initialize new settings (<see cref="SettingsInfo" />) and save them (to a file).
    /// </summary>
    public static void Initialize()
    {
        Current = new SettingsInfo
        {
            Version = AssemblyManager.Current.Version.ToString()
        };

        Save();
    }

    /// <summary>
    ///     Method to load the settings (from a file).
    /// </summary>
    public static void Load()
    {
        var filePath = GetSettingsFilePath();
        var legacyFilePath = GetLegacySettingsFilePath();

        // Check if JSON file exists
        if (File.Exists(filePath))
        {
            Current = DeserializeFromFile(filePath);

            Current.SettingsChanged = false;

            return;
        }

        // Check if legacy XML file exists and migrate it
        if (File.Exists(legacyFilePath))
        {
            Log.Info("Legacy XML settings file found. Migrating to JSON format...");

            Current = DeserializeFromXmlFile(legacyFilePath);

            Current.SettingsChanged = false;

            // Save in new JSON format
            Save();

            // Create a backup of the legacy XML file and delete the original
            Backup(legacyFilePath,
                GetSettingsBackupFolderLocation(),
                TimestampHelper.GetTimestampFilename(GetLegacySettingsFileName()));

            File.Delete(legacyFilePath);

            Log.Info("Settings migration from XML to JSON completed successfully.");

            return;
        }

        // Initialize the default settings if there is no settings file.
        Initialize();
    }

    /// <summary>
    ///     Method to deserialize the settings from a JSON file.
    /// </summary>
    /// <param name="filePath">Path to the settings file.</param>
    /// <returns>Settings as <see cref="SettingsInfo" />.</returns>
    private static SettingsInfo DeserializeFromFile(string filePath)
    {
        var jsonString = File.ReadAllText(filePath);

        var settingsInfo = JsonSerializer.Deserialize<SettingsInfo>(jsonString, JsonOptions);

        return settingsInfo;
    }

    /// <summary>
    ///     Method to deserialize the settings from a legacy XML file.
    /// </summary>
    /// <param name="filePath">Path to the XML settings file.</param>
    /// <returns>Settings as <see cref="SettingsInfo" />.</returns>
    [Obsolete("Legacy XML settings are no longer used, but the method is kept for migration purposes.")]
    private static SettingsInfo DeserializeFromXmlFile(string filePath)
    {
        var xmlSerializer = new XmlSerializer(typeof(SettingsInfo));

        using var fileStream = new FileStream(filePath, FileMode.Open);

        var settingsInfo = xmlSerializer.Deserialize(fileStream) as SettingsInfo;

        return settingsInfo;
    }

    /// <summary>
    ///     Method to save the currently loaded settings (to a file).
    /// </summary>
    public static void Save()
    {
        // Create the directory if it does not exist
        Directory.CreateDirectory(GetSettingsFolderLocation());

        // Create backup before modifying
        CreateDailyBackupIfNeeded();

        // Serialize the settings to a file
        SerializeToFile(GetSettingsFilePath());

        // Set the setting changed to false after saving them as file...
        Current.SettingsChanged = false;
    }

    /// <summary>
    ///     Method to serialize the settings to a JSON file.
    /// </summary>
    /// <param name="filePath">Path to the settings file.</param>
    private static void SerializeToFile(string filePath)
    {
        var jsonString = JsonSerializer.Serialize(Current, JsonOptions);

        File.WriteAllText(filePath, jsonString);
    }

    #endregion

    #region Backup
    /// <summary>
    /// Creates a backup of the settings file if a backup has not already been created for the current day.
    /// </summary>
    /// <remarks>This method checks whether a backup for the current date exists and, if not, creates a new
    /// backup of the settings file. It also removes old backups according to the configured maximum number of backups.
    /// If the settings file does not exist, no backup is created and a warning is logged. This method is intended to be
    /// called as part of a daily maintenance routine.</remarks>
    private static void CreateDailyBackupIfNeeded()
    {
        var maxBackups = Current.Settings_MaximumNumberOfBackups;

        // Check if backups are disabled
        if (maxBackups == 0)
        {
            Log.Debug("Daily backups are disabled. Skipping backup creation...");

            return;
        }

        var currentDate = DateTime.Now.Date;

        if (Current.LastBackup < currentDate)
        {
            // Check if settings file exists
            if (!File.Exists(GetSettingsFilePath()))
            {
                Log.Warn("Settings file does not exist yet. Skipping backup creation...");
                return;
            }

            // Create backup
            Backup(GetSettingsFilePath(),
                GetSettingsBackupFolderLocation(),
                TimestampHelper.GetTimestampFilename(GetSettingsFileName()));

            // Cleanup old backups
            CleanupBackups(GetSettingsBackupFolderLocation(),
                GetSettingsFileName(),
                maxBackups);

            Current.LastBackup = currentDate;
        }
    }

    /// <summary>
    /// Deletes older backup files in the specified folder to ensure that only the most recent backups, up to the
    /// specified maximum, are retained.
    /// </summary>
    /// <remarks>This method removes the oldest backup files first, keeping only the most recent backups as
    /// determined by the timestamp in the filename. It is intended to prevent excessive accumulation of backup files and manage
    /// disk space usage.</remarks>
    /// <param name="backupFolderPath">The full path to the directory containing the backup files to be managed. Cannot be null or empty.</param>
    /// <param name="settingsFileName">The file name pattern used to identify backup files for cleanup.</param>
    /// <param name="maxBackupFiles">The maximum number of backup files to retain. Must be greater than zero.</param>
    private static void CleanupBackups(string backupFolderPath, string settingsFileName, int maxBackupFiles)
    {
        // Get all backup files sorted by timestamp (newest first)
        var backupFiles = Directory.GetFiles(backupFolderPath)
            .Where(f => (f.EndsWith(settingsFileName) || f.EndsWith(GetLegacySettingsFileName())) && TimestampHelper.IsTimestampedFilename(Path.GetFileName(f)))
            .OrderByDescending(f => TimestampHelper.ExtractTimestampFromFilename(Path.GetFileName(f)))
            .ToList();

        if (backupFiles.Count > maxBackupFiles)
            Log.Info($"Cleaning up old backup files... Found {backupFiles.Count} backups, keeping the most recent {maxBackupFiles}.");

        // Delete oldest backups until the maximum number is reached
        while (backupFiles.Count > maxBackupFiles)
        {
            var fileToDelete = backupFiles.Last();

            File.Delete(fileToDelete);

            backupFiles.RemoveAt(backupFiles.Count - 1);

            Log.Info($"Backup deleted: {fileToDelete}");
        }
    }

    /// <summary>
    /// Creates a backup of the specified settings file in the given backup folder with the provided backup file name.
    /// </summary>
    /// <param name="filePath">The full path to the settings file to back up. Cannot be null or empty.</param>
    /// <param name="backupFolderPath">The directory path where the backup file will be stored. If the directory does not exist, it will be created.</param>
    /// <param name="backupFileName">The name to use for the backup file within the backup folder. Cannot be null or empty.</param>
    private static void Backup(string filePath, string backupFolderPath, string backupFileName)
    {
        // Create the backup directory if it does not exist
        Directory.CreateDirectory(backupFolderPath);

        // Create the backup file path
        var backupFilePath = Path.Combine(backupFolderPath, backupFileName);

        // Copy the current settings file to the backup location
        File.Copy(filePath, backupFilePath, true);

        Log.Info($"Backup created: {backupFilePath}");
    }

    #endregion

    #region Upgrade

    /// <summary>
    ///     Method to upgrade the settings.
    /// </summary>
    /// <param name="fromVersion">Previous used version.</param>
    /// <param name="toVersion">Target version.</param>
    public static void Upgrade(Version fromVersion, Version toVersion)
    {
        Log.Info($"Start settings upgrade from {fromVersion} to {toVersion}...");

        // 2023.3.7.0
        if (fromVersion < new Version(2023, 3, 7, 0))
            UpgradeTo_2023_3_7_0();

        // 2023.4.26.0
        if (fromVersion < new Version(2023, 4, 26, 0))
            UpgradeTo_2023_4_26_0();

        // 2023.6.27.0
        if (fromVersion < new Version(2023, 6, 27, 0))
            UpgradeTo_2023_6_27_0();

        // 2023.11.28.0
        if (fromVersion < new Version(2023, 11, 28, 0))
            UpgradeTo_2023_11_28_0();

        // 2024.11.11.0
        if (fromVersion < new Version(2024, 11, 11, 0))
            UpgradeTo_2024_11_11_0();

        // 2025.8.11.0
        if (fromVersion < new Version(2025, 8, 11, 0))
            UpgradeTo_2025_8_11_0();

        // Latest
        if (fromVersion < toVersion)
            UpgradeToLatest(toVersion);

        // Update to the latest version and save
        Current.UpgradeDialog_Show = true;
        Current.Version = toVersion.ToString();

        Save();

        Log.Info("Settings upgrade finished!");
    }

    /// <summary>
    ///     Method to apply changes for version 2023.3.7.0.
    /// </summary>
    private static void UpgradeTo_2023_3_7_0()
    {
        Log.Info("Apply update to 2023.3.7.0...");

        // Add NTP Lookup application
        Log.Info("Add new app \"SNTPLookup\"...");
        Current.General_ApplicationList.Add(ApplicationManager.GetDefaultList()
            .First(x => x.Name == ApplicationName.SNTPLookup));
        Current.SNTPLookup_SNTPServers =
            [.. SNTPServer.GetDefaultList()];

        // Add IP Scanner custom commands
        foreach (var customCommand in from customCommand in IPScannerCustomCommand.GetDefaultList()
                                      let customCommandFound =
                                          Current.IPScanner_CustomCommands.FirstOrDefault(x => x.Name == customCommand.Name)
                                      where customCommandFound == null
                                      select customCommand)
        {
            Log.Info($"Add \"{customCommand.Name}\" to \"IPScanner_CustomCommands\"...");
            Current.IPScanner_CustomCommands.Add(customCommand);
        }

        // Add or update Port Scanner port profiles
        foreach (var portProfile in PortProfile.GetDefaultList())
        {
            var portProfileFound = Current.PortScanner_PortProfiles.FirstOrDefault(x => x.Name == portProfile.Name);

            if (portProfileFound == null)
            {
                Log.Info($"Add \"{portProfile.Name}\" to \"PortScanner_PortProfiles\"...");
                Current.PortScanner_PortProfiles.Add(portProfile);
            }
            else if (!portProfile.Ports.Equals(portProfileFound.Ports))
            {
                Log.Info($"Update \"{portProfile.Name}\" in \"PortScanner_PortProfiles\"...");
                Current.PortScanner_PortProfiles.Remove(portProfileFound);
                Current.PortScanner_PortProfiles.Add(portProfile);
            }
        }

        // Add new DNS lookup profiles
        Log.Info("Init \"DNSLookup_DNSServers_v2\" with default DNS servers...");
        Current.DNSLookup_DNSServers =
            [.. DNSServer.GetDefaultList()];
    }

    /// <summary>
    ///     Method to apply changes for version 2023.4.26.0.
    /// </summary>
    private static void UpgradeTo_2023_4_26_0()
    {
        Log.Info("Apply update to 2023.4.26.0...");

        // Add SNMP OID profiles
        Log.Info("Add SNMP OID profiles...");
        Current.SNMP_OidProfiles = [.. SNMPOIDProfile.GetDefaultList()];
    }

    /// <summary>
    ///     Method to apply changes for version 2023.6.27.0.
    /// </summary>
    private static void UpgradeTo_2023_6_27_0()
    {
        Log.Info("Apply update to 2023.6.27.0...");

        // Update Wake on LAN settings
        Log.Info($"Update \"WakeOnLAN_Port\" to {GlobalStaticConfiguration.WakeOnLAN_Port}");
        Current.WakeOnLAN_Port = GlobalStaticConfiguration.WakeOnLAN_Port;
    }

    /// <summary>
    ///     Method to apply changes for version 2023.11.28.0.
    /// </summary>
    private static void UpgradeTo_2023_11_28_0()
    {
        Log.Info("Apply upgrade to 2023.11.28.0...");

        // First run is required due to the new settings
        Log.Info("Set \"FirstRun\" to true...");
        Current.WelcomeDialog_Show = true;

        // Add IP geolocation application
        Log.Info("Add new app \"IP Geolocation\"...");
        Current.General_ApplicationList.Add(ApplicationManager.GetDefaultList()
            .First(x => x.Name == ApplicationName.IPGeolocation));

        // Add DNS lookup profiles after refactoring
        Log.Info("Init \"DNSLookup_DNSServers\" with default DNS servers...");
        Current.DNSLookup_DNSServers =
            [.. DNSServer.GetDefaultList()];
    }

    /// <summary>
    ///     Method to apply changes for version 2024.11.11.0.
    /// </summary>
    private static void UpgradeTo_2024_11_11_0()
    {
        Log.Info("Apply upgrade to 2024.11.11.0...");

        Log.Info("Reset ApplicationList to default...");
        Current.General_ApplicationList =
            [.. ApplicationManager.GetDefaultList()];
    }

    /// <summary>
    ///     Method to apply changes for version 2025.8.11.0.
    /// </summary>
    private static void UpgradeTo_2025_8_11_0()
    {
        Log.Info("Apply upgrade to 2025.8.11.0...");

        // Add Hosts editor application
        Log.Info("Add new app \"Hosts File Editor\"...");

        Current.General_ApplicationList.Insert(
            ApplicationManager.GetDefaultList().ToList().FindIndex(x => x.Name == ApplicationName.HostsFileEditor),
            ApplicationManager.GetDefaultList().First(x => x.Name == ApplicationName.HostsFileEditor));
    }

    /// <summary>
    ///     Method to apply changes for the latest version.
    /// </summary>
    /// <param name="version">Latest version.</param>
    private static void UpgradeToLatest(Version version)
    {
        Log.Info($"Apply upgrade to {version}...");

        // DNS Lookup

        Log.Info("Migrate DNS Lookup settings to new structure...");

        Current.DNSLookup_SelectedDNSServer_v2 = Current.DNSLookup_SelectedDNSServer?.Name;

        Log.Info($"Selected DNS server set to \"{Current.DNSLookup_SelectedDNSServer_v2}\"");

        // AWS Session Manager

        Log.Info("Removing deprecated app \"AWS Session Manager\", if it exists...");

        var appToRemove = Current.General_ApplicationList
            .FirstOrDefault(x => x.Name == ApplicationName.AWSSessionManager);

        if (appToRemove != null)
        {
            if (appToRemove.IsDefault)
            {
                Log.Info("\"AWS Session Manager\" is set as the default app. Setting the new default app to the first visible app...");

                var newDefaultApp = Current.General_ApplicationList.FirstOrDefault(x => x.IsVisible);

                if (newDefaultApp != null)
                {
                    Log.Info($"Set \"{newDefaultApp.Name}\" as the new default app");
                    newDefaultApp.IsDefault = true;
                }
                else
                {
                    Log.Error("No visible app found to set as the new default app.");
                }
            }

            Current.General_ApplicationList.Remove(appToRemove);
        }
    }
    #endregion
}