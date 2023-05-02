using log4net;
using NETworkManager.Models;
using NETworkManager.Models.Network;
using NETworkManager.Models.PowerShell;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace NETworkManager.Settings;

public static class SettingsManager
{
    #region Variables
    /// <summary>
    /// Logger for logging.
    /// </summary>
    private static readonly ILog _log = LogManager.GetLogger(typeof(SettingsManager));

    /// <summary>
    /// Settings directory name.
    /// </summary>
    private static string SettingsFolderName => "Settings";

    /// <summary>
    /// Settings file name.
    /// </summary>
    private static string SettingsFileName => "Settings";

    /// <summary>
    /// Settings file extension.
    /// </summary>
    private static string SettingsFileExtension => ".xml";

    /// <summary>
    /// Settings that are currently loaded.
    /// </summary>
    public static SettingsInfo Current { get; set; }

    /// <summary>
    /// Indicates if the HotKeys have changed. May need to be reworked if we add more HotKeys.
    /// </summary>
    public static bool HotKeysChanged { get; set; }
    #endregion

    #region Settings location, default paths and file names
    /// <summary>
    /// Method to get the path of the settings folder.
    /// </summary>
    /// <returns>Path to the settings folder.</returns>
    public static string GetSettingsFolderLocation()
    {
        return ConfigurationManager.Current.IsPortable ?
            Path.Combine(AssemblyManager.Current.Location, SettingsFolderName) :
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AssemblyManager.Current.Name, SettingsFolderName);
    }

    /// <summary>
    /// Method to get the settings file name.
    /// </summary>
    /// <returns>Settings file name.</returns>
    public static string GetSettingsFileName()
    {
        return $"{SettingsFileName}{SettingsFileExtension}";
    }

    /// <summary>
    /// Method to get the settings file path.
    /// </summary>
    /// <returns>Settings file path.</returns>
    public static string GetSettingsFilePath()
    {
        return Path.Combine(GetSettingsFolderLocation(), GetSettingsFileName());
    }
    #endregion

    #region Load, Save
    /// <summary>
    /// Method to load the settings (from a file).
    /// </summary>
    public static void Load()
    {
        var filePath = GetSettingsFilePath();

        if (File.Exists(filePath))
        {
            Current = DeserializeFromFile(filePath);

            Current.SettingsChanged = false;

            return;
        }

        // Initialize the default settings if there is no settings file.
        InitDefault();
    }

    /// <summary>
    /// Method to deserialize the settings from a file.
    /// </summary>
    /// <param name="filePath">Path to the settings file.</param>
    /// <returns>Settings as <see cref="SettingsInfo"/>.</returns>
    private static SettingsInfo DeserializeFromFile(string filePath)
    {
        SettingsInfo settingsInfo;

        var xmlSerializer = new XmlSerializer(typeof(SettingsInfo));

        using (var fileStream = new FileStream(filePath, FileMode.Open))
        {
            settingsInfo = (SettingsInfo)xmlSerializer.Deserialize(fileStream);
        }

        return settingsInfo;
    }

    /// <summary>
    /// Method to save the currently loaded settings (to a file).
    /// </summary>
    public static void Save()
    {
        // Create the directory if it does not exist
        Directory.CreateDirectory(GetSettingsFolderLocation());

        // Serialize the settings to a file
        SerializeToFile(GetSettingsFilePath());

        // Set the setting changed to false after saving them as file...
        Current.SettingsChanged = false;
    }

    /// <summary>
    /// Method to serialize the settings to a file.
    /// </summary>
    /// <param name="filePath">Path to the settings file.</param>
    private static void SerializeToFile(string filePath)
    {
        var xmlSerializer = new XmlSerializer(typeof(SettingsInfo));

        using var fileStream = new FileStream(filePath, FileMode.Create);

        xmlSerializer.Serialize(fileStream, Current);
    }
    #endregion

    #region Init (and reset)
    /// <summary>
    /// Initialize new settings (<see cref="SettingsInfo"/>) and save them (to a file).
    /// </summary>
    public static void InitDefault()
    {
        // Init new Settings with default data
        Current = new SettingsInfo
        {
            SettingsChanged = true
        };

        Save();
    }
    #endregion

    #region Upgrade
    /// <summary>
    /// Method to upgrade the settings.
    /// </summary>
    /// <param name="fromVersion">Previous used version.</param>
    /// <param name="toVersion">Target version.</param>
    public static void Upgrade(Version fromVersion, Version toVersion)
    {
        _log.Info($"Start settings upgrade from {fromVersion} to {toVersion}...");

        // 2022.12.20.0
        if (fromVersion < new Version(2022, 12, 20, 0))
            UpgradeTo_2022_12_20_0();

        // 2023.3.7.0
        if (fromVersion < new Version(2023, 3, 7, 0))
            UpgradeTo_2023_3_7_0();

        // 2023.4.26.0
        if (fromVersion < new Version(2023, 4, 26, 0))
            UpgradeTo_2023_4_26_0();

        // Latest
        if (fromVersion < toVersion)
            UpgradeToLatest(toVersion);

        // Update to the latest version and save
        Current.Version = toVersion.ToString();
        Save();

        _log.Info("Settings upgrade finished!");
    }

    /// <summary>
    /// Method to apply changes for version 2022.12.20.0.
    /// </summary>
    private static void UpgradeTo_2022_12_20_0()
    {
        _log.Info("Apply update to 2022.12.20.0...");

        // Add AWS Session Manager application
        _log.Info("Add new app \"AWSSessionManager\"...");
        Current.General_ApplicationList.Add(ApplicationManager.GetList().First(x => x.Name == ApplicationName.AWSSessionManager));

        var powerShellPath = "";
        foreach (var file in PowerShell.GetDefaultIntallationPaths)
        {
            if (File.Exists(file))
            {
                powerShellPath = file;
                break;
            }
        }

        _log.Info($"Set \"AWSSessionManager_ApplicationFilePath\" to \"{powerShellPath}\"...");
        Current.AWSSessionManager_ApplicationFilePath = powerShellPath;

        // Add Bit Calculator application
        _log.Info("Add new app \"BitCalculator\"...");
        Current.General_ApplicationList.Add(ApplicationManager.GetList().First(x => x.Name == ApplicationName.BitCalculator));
    }

    /// <summary>
    /// Method to apply changes for version 2023.3.7.0.
    /// </summary>
    private static void UpgradeTo_2023_3_7_0()
    {
        _log.Info("Apply update to 2023.3.7.0...");
        
        // Add NTP Lookup application
        _log.Info($"Add new app \"SNTPLookup\"...");
        Current.General_ApplicationList.Add(ApplicationManager.GetList().First(x => x.Name == ApplicationName.SNTPLookup));
        Current.SNTPLookup_SNTPServers = new ObservableCollection<ServerConnectionInfoProfile>(SNTPServer.GetDefaultList());
        
        // Add IP Scanner custom commands
        foreach (var customCommand in IPScannerCustomCommand.GetDefaultList())
        {
            var customCommandFound = Current.IPScanner_CustomCommands.FirstOrDefault(x => x.Name == customCommand.Name);

            if (customCommandFound == null)
            {
                _log.Info($"Add \"{customCommand.Name}\" to \"IPScanner_CustomCommands\"...");
                Current.IPScanner_CustomCommands.Add(customCommand);
            }
        }

        // Add or update Port Scanner port profiles
        foreach (var portProfile in PortProfile.GetDefaultList())
        {
            var portProfileFound = Current.PortScanner_PortProfiles.FirstOrDefault(x => x.Name == portProfile.Name);

            if (portProfileFound == null)
            {
                _log.Info($"Add \"{portProfile.Name}\" to \"PortScanner_PortProfiles\"...");
                Current.PortScanner_PortProfiles.Add(portProfile);
            }
            else if (!portProfile.Ports.Equals(portProfileFound.Ports))
            {
                _log.Info($"Update \"{portProfile.Name}\" in \"PortScanner_PortProfiles\"...");
                Current.PortScanner_PortProfiles.Remove(portProfileFound);
                Current.PortScanner_PortProfiles.Add(portProfile);
            }
        }

        // Add new DNS lookup profiles
        _log.Info("Init \"DNSLookup_DNSServers_v2\" with default DNS servers...");
        Current.DNSLookup_DNSServers_v2 = new ObservableCollection<DNSServerConnectionInfoProfile>(DNSServer.GetDefaultList());
    }

    /// <summary>
    /// Method to apply changes for version 2023.4.26.0.
    /// </summary>
    private static void UpgradeTo_2023_4_26_0()
    {
        _log.Info("Apply update to 2023.4.26.0...");

        // Add SNMP OID profiles
         _log.Info($"Add SNMP OID profiles...");
        Current.SNMP_OIDProfiles = new ObservableCollection<SNMPOIDProfileInfo>(SNMPOIDProfile.GetDefaultList());
    }

    /// <summary>
    /// Method to apply changes for the latest version.
    /// </summary>
    /// <param name="version">Latest version.</param>
    private static void UpgradeToLatest(Version version)
    {
        _log.Info($"Apply upgrade to {version}...");

       
    }
    #endregion
}
