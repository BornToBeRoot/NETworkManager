using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NETworkManager.Models.Settings
{
    public static class SettingsManager
    {
        private const string SettingsFolderName = "Settings";
        private const string SettingsFileExtension = ".settings";
        private const string IsPortableFileName = "IsPortable" + SettingsFileExtension;

        public static SettingsInfo Current { get; set; }

        public static bool ForceRestart { get; set; }
        public static bool HotKeysChanged { get; set; }

        private static string ApplicationName
        {
            get { return Assembly.GetEntryAssembly().GetName().Name; }
        }

        private static string ApplicationLocation
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        public static string SettingsFileName
        {
            get { return string.Format("{0}{1}", ApplicationName, SettingsFileExtension); }
        }

        #region Settings locations (default, custom, portable)
        // %AppData%\PRODUCTNAME\Settings
        public static string DefaultSettingsLocation
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationName, SettingsFolderName); }
        }

        // Custom location
        public static string CustomSettingsLocation
        {
            get { return Properties.Settings.Default.Settings_CustomSettingsLocation; }
        }

        // STARTUPPATH\Settings
        public static string PortableSettingsLocation
        {
            get { return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), SettingsFolderName); }
        }
        #endregion

        #region File paths
        // This is the file (stored in the application folder) which tells the application that it is portable
        private static string IsPortableFilePath
        {
            get { return Path.Combine(ApplicationLocation, IsPortableFileName); }
        }

        public static string SettingsFilePath
        {
            get { return Path.Combine(SettingsLocation, SettingsFileName); }
        }

        #endregion

        #region IsPortable, SettingsLocation, SettingsLocationNotPortable
        public static bool IsPortable
        {
            get { return File.Exists(IsPortableFilePath); }
        }

        public static string SettingsLocation
        {
            get
            {
                if (IsPortable)
                    return PortableSettingsLocation;

                string settingsLocation = CustomSettingsLocation;

                if (!string.IsNullOrEmpty(settingsLocation) && Directory.Exists(settingsLocation))
                    return settingsLocation;

                return DefaultSettingsLocation;
            }
        }

        public static string SettingsLocationNotPortable
        {
            get
            {
                string settingsLocation = CustomSettingsLocation;

                if (!string.IsNullOrEmpty(settingsLocation) && Directory.Exists(settingsLocation))
                    return settingsLocation;

                return DefaultSettingsLocation;
            }
        }
        #endregion

        #region XmlSerializer (save and load) 
        public static void Load()
        {
            if (File.Exists(SettingsFilePath) && !CommandLineManager.Current.ResetSettings)
            {
                SettingsInfo settingsInfo = new SettingsInfo();

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(SettingsInfo));

                using (FileStream fileStream = new FileStream(SettingsFilePath, FileMode.Open))
                {
                    settingsInfo = (SettingsInfo)(xmlSerializer.Deserialize(fileStream));
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
            Directory.CreateDirectory(SettingsLocation);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SettingsInfo));

            using (FileStream fileStream = new FileStream(Path.Combine(SettingsFilePath), FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, Current);
            }

            // Set the setting changed to false after saving them as file...
            Current.SettingsChanged = false;
        }
        #endregion

        #region Methods       
        public static Task MoveSettingsAsync(string sourceLocation, string targedLocation)
        {
            return Task.Run(() => MoveSettings(sourceLocation, targedLocation));
        }

        private static void MoveSettings(string sourceLocation, string targedLocation)
        {
            string[] sourceFiles = Directory.GetFiles(sourceLocation);

            // Create the dircetory and copy the files to the new location
            Directory.CreateDirectory(targedLocation);

            foreach (string file in sourceFiles)
                File.Copy(file, Path.Combine(targedLocation, Path.GetFileName(file)), true);

            // Delete the old files
            foreach (string file in sourceFiles)
                File.Delete(file);

            // Delete the folder, if it is not the default settings locations and does not contain any files or directories
            if (sourceLocation != DefaultSettingsLocation && Directory.GetFiles(sourceLocation).Length == 0 && Directory.GetDirectories(sourceLocation).Length == 0)
                Directory.Delete(sourceLocation);
        }

        public static Task MakePortableAsync(bool isPortable)
        {
            return Task.Run(() => MakePortable(isPortable));
        }

        public static void MakePortable(bool isPortable)
        {
            if (isPortable)
            {
                MoveSettings(SettingsLocationNotPortable, PortableSettingsLocation);

                // After moving the files, set the indicator that the settings are now portable
                File.Create(IsPortableFilePath);
            }
            else
            {
                MoveSettings(PortableSettingsLocation, SettingsLocationNotPortable);

                // Remove the indicator after moving the files...
                File.Delete(IsPortableFilePath);
            }
        }

        public static void Reset()
        {
            // Init new Settings with default data
            Current = new SettingsInfo()
            {
                SettingsChanged = true
            };

            ForceRestart = true;
        }
        #endregion                
    }
}