using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NETworkManager.Models.Settings
{
    public static class SettingsManager
    {
        private const string SettingsFolderName = "Settings";
        private const string SettingsFileName = "Settings";
        private const string SettingsFileExtension = "xml";
        private const string IsPortableFileName = "IsPortable";
        private const string IsPortableExtension = "settings";

        public static SettingsInfo Current { get; set; }

        public static bool ForceRestart { get; set; }
        public static bool HotKeysChanged { get; set; }

        private static string GetApplicationName()
        {
            return Assembly.GetEntryAssembly().GetName().Name;
        }

        private static string GetApplicationLocation()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string GetSettingsFileName()
        {
            return $"{SettingsFileName}.{SettingsFileExtension}";
        }

        public static string GetIsPortableFileName()
        {
            return $"{IsPortableFileName}.{IsPortableExtension}";
        }

        #region Settings locations (default, custom, portable)
        public static string GetDefaultSettingsLocation()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GetApplicationName(), SettingsFolderName);
        }

        public static string GetCustomSettingsLocation()
        {
            return Properties.Settings.Default.Settings_CustomSettingsLocation;
        }

        public static string GetPortableSettingsLocation()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException(), SettingsFolderName);
        }
        #endregion

        #region File paths
        private static string GetIsPortableFilePath()
        {
            return Path.Combine(GetApplicationLocation(), GetIsPortableFileName());
        }

        public static string GetSettingsFilePath()
        {
            return Path.Combine(GetSettingsLocation(), GetSettingsFileName());
        }
        #endregion

        #region IsPortable, SettingsLocation, SettingsLocationNotPortable
        public static bool GetIsPortable()
        {
            return File.Exists(GetIsPortableFilePath());
        }

        public static string GetSettingsLocation()
        {
            return GetIsPortable() ? GetPortableSettingsLocation() : GetSettingsLocationNotPortable();
        }

        public static string GetSettingsLocationNotPortable()
        {
            var settingsLocation = GetCustomSettingsLocation();

            if (!string.IsNullOrEmpty(settingsLocation) && Directory.Exists(settingsLocation))
                return settingsLocation;

            return GetDefaultSettingsLocation();
        }
        #endregion

        public static void Load()
        {
            if (File.Exists(GetSettingsFilePath()) && !CommandLineManager.Current.ResetSettings)
            {
                SettingsInfo settingsInfo;

                var xmlSerializer = new XmlSerializer(typeof(SettingsInfo));

                using (var fileStream = new FileStream(GetSettingsFilePath(), FileMode.Open))
                {
                    settingsInfo = (SettingsInfo)xmlSerializer.Deserialize(fileStream);
                }

                Current = settingsInfo;

                // Set the setting changed to false after loading them from a file...
                Current.SettingsChanged = false;
            }
            else
            {
                Current = new SettingsInfo();
            }
        }

        public static void Save()
        {
            // Create the directory if it does not exist
            Directory.CreateDirectory(GetSettingsLocation());

            var xmlSerializer = new XmlSerializer(typeof(SettingsInfo));

            using (var fileStream = new FileStream(Path.Combine(GetSettingsFilePath()), FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, Current);
            }

            // Set the setting changed to false after saving them as file...
            Current.SettingsChanged = false;
        }

        public static Task MoveSettingsAsync(string sourceLocation, string targedLocation, bool overwrite, string[] filesTargedLocation)
        {
            return Task.Run(() => MoveSettings(sourceLocation, targedLocation, overwrite, filesTargedLocation));
        }

        private static void MoveSettings(string sourceLocation, string targedLocation, bool overwrite, string[] filesTargedLocation = null)
        {
            var sourceFiles = Directory.GetFiles(sourceLocation);

            // Create the dircetory and copy the files to the new location
            Directory.CreateDirectory(targedLocation);

            foreach (var file in sourceFiles)
            {
                // Skip if file exists and user don't want to overwrite it
                if (!overwrite && (filesTargedLocation ?? throw new ArgumentNullException(nameof(filesTargedLocation))).Any(x => Path.GetFileName(x) == Path.GetFileName(file)))
                    continue;

                File.Copy(file, Path.Combine(targedLocation, Path.GetFileName(file)), overwrite);
            }

            // Delete the old files
            foreach (var file in sourceFiles)
                File.Delete(file);

            // Delete the folder, if it is not the default settings locations and does not contain any files or directories
            if (sourceLocation != GetDefaultSettingsLocation() && Directory.GetFiles(sourceLocation).Length == 0 && Directory.GetDirectories(sourceLocation).Length == 0)
                Directory.Delete(sourceLocation);
        }

        public static Task MakePortableAsync(bool isPortable, bool overwrite)
        {
            return Task.Run(() => MakePortable(isPortable, overwrite));
        }

        public static void MakePortable(bool isPortable, bool overwrite)
        {
            if (isPortable)
            {
                MoveSettings(GetSettingsLocationNotPortable(), GetPortableSettingsLocation(), overwrite);

                // After moving the files, set the indicator that the settings are now portable
                File.Create(GetIsPortableFilePath());
            }
            else
            {
                MoveSettings(GetPortableSettingsLocation(), GetSettingsLocationNotPortable(), overwrite);

                // Remove the indicator after moving the files...
                File.Delete(GetIsPortableFilePath());
            }
        }

        public static void InitDefault()
        {
            // Init new Settings with default data
            Current = new SettingsInfo
            {
                SettingsChanged = true
            };
        }

        public static void Reset()
        {
            InitDefault();

            ForceRestart = true;
        }

        public static void Update(Version programmVersion, Version settingsVersion)
        {
            // Version is 0.0.0.0 on first run or settings reset --> skip updates 
            if (settingsVersion > new Version("0.0.0.0"))
            {
                var reorderApplications = false;

                // Features added in 1.8.0.0
                if (settingsVersion < new Version("1.8.0.0"))
                {
                    Current.General_ApplicationList.Add(new ApplicationViewInfo(ApplicationViewManager.Name.TigerVNC));
                    Current.General_ApplicationList.Add(new ApplicationViewInfo(ApplicationViewManager.Name.Whois));

                    reorderApplications = true;
                }

                // Features added in 1.9.0.0
                if (settingsVersion < new Version("1.9.0.0"))
                {
                    Current.General_ApplicationList.Add(new ApplicationViewInfo(ApplicationViewManager.Name.PowerShell));
                    
                    reorderApplications = true;
                }

                // Features added in 1.10.0.0
                if (settingsVersion < new Version("1.10.0.0"))
                {
                    Current.General_ApplicationList.Add(new ApplicationViewInfo(ApplicationViewManager.Name.Dashboard));

                    reorderApplications = true;
                }

                // Reorder application view
                if (reorderApplications)
                    Current.General_ApplicationList = new ObservableCollection<ApplicationViewInfo>(Current.General_ApplicationList.OrderBy(info => info.Name));
            }

            // Update settings version
            Current.SettingsVersion = programmVersion.ToString();
        }
    }
}