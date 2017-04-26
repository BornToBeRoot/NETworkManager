using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;
using NETworkManager.Settings.Templates;

namespace NETworkManager.Settings
{
    public static class SettingsManager
    {
        private const string SettingsFolderName = "Settings";
        private const string IsPortableFileName = "IsPortable.settings";

        public static bool SettingsChanged { get; set; }
        public static bool RestartRequired { get; set; }

        /// <summary>
        /// Name of the application
        /// </summary>
        private static string ApplicationName
        {
            get { return Assembly.GetEntryAssembly().GetName().Name; }
        }

        /// <summary>
        /// Default settings location ("%AppData%\PRODUCTNAME\Settings")
        /// </summary>
        public static string DefaultSettingsLocation
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationName, SettingsFolderName); }
        }

        /// <summary>     
        /// Custom settings location (wherever the use want to store the files)
        /// </summary>
        private static string CustomSettingsLocation
        {
            get { return Properties.Settings.Default.Settings_Location; }
        }

        /// <summary>
        /// Returns the custom (if it exists) or default settings location
        /// </summary>
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

        /// <summary>
        /// Returns the current used settings location (Portable or Custom or Default)
        /// </summary>
        public static string SettingsLocation
        {
            get
            {
                if (IsPortable)
                    return PortableSettingsLocation;

                return SettingsLocationNotPortable;
            }
        }

        /// <summary>
        /// Portable settings location (PROGRAMFOLDER\Settings)
        /// </summary>
        public static string PortableSettingsLocation
        {
            get { return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), SettingsFolderName); }
        }

        /// <summary>
        /// Path to the file which indicates that the application is portable
        /// </summary>
        private static string IsPortableFilePath
        {
            get { return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), IsPortableFileName); }
        }

        /// <summary>
        /// Indicates if the settings are portable
        /// </summary>
        public static bool IsPortable
        {
            get { return File.Exists(IsPortableFilePath); }
        }

        public static void SaveSettings()
        {
            Properties.Settings.Default.Save();
        }

        private static void MoveSettings(string sourceLocation, string targedLocation, bool overwriteExistingFiles)
        {
            if (!Directory.Exists(targedLocation))
                Directory.CreateDirectory(targedLocation);

            if (Directory.Exists(sourceLocation))
            {
                string[] files = Directory.GetFiles(sourceLocation);

                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string targedFile = Path.Combine(targedLocation, fileName);

                    // Delete file if it already exists
                    if (overwriteExistingFiles)
                        File.Delete(targedFile);

                    File.Move(file, targedFile);
                }
            }
        }

        public static void ChangeSettingsLocation(string targedLocation, bool overrideExistingFiles)
        {
            MoveSettings(SettingsLocationNotPortable, targedLocation, overrideExistingFiles);
        }

        public static void MakeSettingsPortable(bool isPortable, bool overrideExistingFiles)
        {
            string sourceLocation = string.Empty;
            string targedLocation = string.Empty;

            if (isPortable)
            {
                sourceLocation = SettingsLocationNotPortable;
                targedLocation = PortableSettingsLocation;

                // Create the file that indicates that the application is portable
                File.Create(IsPortableFilePath);
            }
            else
            {
                sourceLocation = PortableSettingsLocation;
                targedLocation = SettingsLocationNotPortable;

                // Delete the file that indicates that the application is portable
                File.Delete(IsPortableFilePath);
            }

            // Move all existing settings to the targed location
            MoveSettings(sourceLocation, targedLocation, overrideExistingFiles);
        }

        #region WakeOnLan
        private static List<WakeOnLanTemplate> DeserializeWakeOnLanTemplates(string filePath)
        {
            List<WakeOnLanTemplate> list = new List<WakeOnLanTemplate>();

            XmlSerializer serializer = new XmlSerializer(typeof(List<WakeOnLanTemplate>));

            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                List<WakeOnLanTemplate> _list = (List<WakeOnLanTemplate>)(serializer.Deserialize(stream));
                list.AddRange(_list);
            }

            return list;
        }

        public static List<WakeOnLanTemplate> GetWakeOnLanTemplates()
        {
            string filePath = Path.Combine(SettingsLocation, Properties.Resources.WakeOnLan_Templates_FileName);

            if (File.Exists(filePath))
                return DeserializeWakeOnLanTemplates(filePath);

            return new List<WakeOnLanTemplate>();
        }

        private static void SerializeWakeOnLanTemplates(List<WakeOnLanTemplate> list, string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<WakeOnLanTemplate>));

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(stream, list);
            }
        }

        public static void SaveWakeOnLanTemplates(List<WakeOnLanTemplate> list)
        {
            string filePath = Path.Combine(SettingsLocation, Properties.Resources.WakeOnLan_Templates_FileName);

            SerializeWakeOnLanTemplates(list, filePath);
        }
        #endregion

        #region NetworkInterfaceConfig
        private static List<NetworkInterfaceTemplate> DeserializeNetworkInterfaceConfigTemplates(string filePath)
        {
            List<NetworkInterfaceTemplate> list = new List<NetworkInterfaceTemplate>();

            XmlSerializer serializer = new XmlSerializer(typeof(List<NetworkInterfaceTemplate>));

            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                List<NetworkInterfaceTemplate> _list = (List<NetworkInterfaceTemplate>)(serializer.Deserialize(stream));
                list.AddRange(_list);
            }

            return list;
        }

        public static List<NetworkInterfaceTemplate> GetNetworkInterfaceConfigTemplates()
        {
            string filePath = Path.Combine(SettingsLocation, Properties.Resources.NetworkInterface_ConfigTemplates_FileName);

            if (File.Exists(filePath))
                return DeserializeNetworkInterfaceConfigTemplates(filePath);

            return new List<NetworkInterfaceTemplate>();
        }

        private static void SerializeNetworkInterfaceConfigTemplates(List<NetworkInterfaceTemplate> list, string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<NetworkInterfaceTemplate>));

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(stream, list);
            }
        }

        public static void SaveNetworkInterfaceConfigTemplates(List<NetworkInterfaceTemplate> list)
        {
            string filePath = Path.Combine(SettingsLocation, Properties.Resources.NetworkInterface_ConfigTemplates_FileName);

            SerializeNetworkInterfaceConfigTemplates(list, filePath);
        }
        #endregion
    }
}
