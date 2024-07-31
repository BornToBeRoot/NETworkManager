using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using log4net;
using NETworkManager.Controls;
using NETworkManager.Models;
using NETworkManager.Models.Network;
using NETworkManager.Models.PowerShell;

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
    ///     Settings file name.
    /// </summary>
    private static string SettingsFileName => "Settings";

    /// <summary>
    ///     Settings file extension.
    /// </summary>
    private static string SettingsFileExtension => ".xml";

    /// <summary>
    ///     Settings that are currently loaded.
    /// </summary>
    public static SettingsInfo Current { get; private set; }

    /// <summary>
    ///     Indicates if the HotKeys have changed. May need to be reworked if we add more HotKeys.
    /// </summary>
    public static bool HotKeysChanged { get; set; }

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
    ///     Method to get the settings file name.
    /// </summary>
    /// <returns>Settings file name.</returns>
    public static string GetSettingsFileName()
    {
        return $"{SettingsFileName}{SettingsFileExtension}";
    }

    /// <summary>
    ///     Method to get the settings file path.
    /// </summary>
    /// <returns>Settings file path.</returns>
    public static string GetSettingsFilePath()
    {
        return Path.Combine(GetSettingsFolderLocation(), GetSettingsFileName());
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

        if (File.Exists(filePath))
        {
            Current = DeserializeFromFile(filePath);

            Current.SettingsChanged = false;

            return;
        }

        // Initialize the default settings if there is no settings file.
        Initialize();
    }

    /// <summary>
    ///     Method to deserialize the settings from a file.
    /// </summary>
    /// <param name="filePath">Path to the settings file.</param>
    /// <returns>Settings as <see cref="SettingsInfo" />.</returns>
    private static SettingsInfo DeserializeFromFile(string filePath)
    {
        var xmlSerializer = new XmlSerializer(typeof(SettingsInfo));

        using var fileStream = new FileStream(filePath, FileMode.Open);

        var settingsInfo = (SettingsInfo)xmlSerializer.Deserialize(fileStream);

        return settingsInfo;
    }

    /// <summary>
    ///     Method to save the currently loaded settings (to a file).
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
    ///     Method to serialize the settings to a file.
    /// </summary>
    /// <param name="filePath">Path to the settings file.</param>
    private static void SerializeToFile(string filePath)
    {
        var xmlSerializer = new XmlSerializer(typeof(SettingsInfo));

        using var fileStream = new FileStream(filePath, FileMode.Create);

        xmlSerializer.Serialize(fileStream, Current);
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

        // 2022.12.20.0
        if (fromVersion < new Version(2022, 12, 20, 0))
            UpgradeTo_2022_12_20_0();

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

        // Latest
        if (fromVersion < toVersion)
            UpgradeToLatest(toVersion);

        // Update to the latest version and save
        Current.Version = toVersion.ToString();
        Save();

        Log.Info("Settings upgrade finished!");
    }

    /// <summary>
    ///     Method to apply changes for version 2022.12.20.0.
    /// </summary>
    private static void UpgradeTo_2022_12_20_0()
    {
        Log.Info("Apply update to 2022.12.20.0...");

        // Add AWS Session Manager application
        Log.Info("Add new app \"AWSSessionManager\"...");
        Current.General_ApplicationList.Add(ApplicationManager.GetDefaultList()
            .First(x => x.Name == ApplicationName.AWSSessionManager));

        var powerShellPath = "";
        foreach (var file in PowerShell.GetDefaultInstallationPaths.Where(File.Exists))
        {
            powerShellPath = file;
            break;
        }

        Log.Info($"Set \"AWSSessionManager_ApplicationFilePath\" to \"{powerShellPath}\"...");
        Current.AWSSessionManager_ApplicationFilePath = powerShellPath;

        // Add Bit Calculator application
        Log.Info("Add new app \"BitCalculator\"...");
        Current.General_ApplicationList.Add(ApplicationManager.GetDefaultList()
            .First(x => x.Name == ApplicationName.BitCalculator));
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
            new ObservableCollection<ServerConnectionInfoProfile>(SNTPServer.GetDefaultList());

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
            new ObservableCollection<DNSServerConnectionInfoProfile>(DNSServer.GetDefaultList());
    }

    /// <summary>
    ///     Method to apply changes for version 2023.4.26.0.
    /// </summary>
    private static void UpgradeTo_2023_4_26_0()
    {
        Log.Info("Apply update to 2023.4.26.0...");

        // Add SNMP OID profiles
        Log.Info("Add SNMP OID profiles...");
        Current.SNMP_OidProfiles = new ObservableCollection<SNMPOIDProfileInfo>(SNMPOIDProfile.GetDefaultList());
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
            new ObservableCollection<DNSServerConnectionInfoProfile>(DNSServer.GetDefaultList());
    }

    /// <summary>
    ///     Method to apply changes for the latest version.
    /// </summary>
    /// <param name="version">Latest version.</param>
    private static void UpgradeToLatest(Version version)
    {
        Log.Info($"Apply upgrade to {version}...");

        Log.Info("Reset ApplicationList to default...");
        Current.General_ApplicationList =
            new ObservableSetCollection<ApplicationInfo>(ApplicationManager.GetDefaultList());
    }

    #endregion
}