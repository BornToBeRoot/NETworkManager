using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace NETworkManager.Models.Settings
{
    public static class TemplateManager
    {
        private const string TemplateFileExtension = ".templates";
        public static string NetworkInterfaceConfigTemplatesFileName = "NetworkInterface" + TemplateFileExtension;
        public static string WakeOnLanTemplatesFileName = "WakeOnLan" + TemplateFileExtension;

        public static ObservableCollection<TemplateWakeOnLanInfo> WakeOnLanTemplates;
        public static ObservableCollection<TemplateNetworkInterfaceConfig> NetworkInterfaceConfigTemplates;

        #region TemplatesLocation
        // Templates are stored in the settings folder
        private static string TemplatesLocation
        {
            get { return SettingsManager.SettingsLocation; }
        }
        #endregion

        #region File paths
        public static string NetworkInterfaceConfigTemplatesFilePath
        {
            get { return Path.Combine(TemplatesLocation, NetworkInterfaceConfigTemplatesFileName); }
        }

        public static string WakeOnLanTemplatesFilePath
        {
            get { return Path.Combine(TemplatesLocation, WakeOnLanTemplatesFileName); }
        }
        #endregion

        public static bool WakeOnLanTemplatesChanged { get; set; }
        public static bool NetworkInterfaceConfigTemplatesChanged { get; set; }

        #region XmlSerializer (save and load) 
        public static void LoadNetworkInterfaceConfigTemplates()
        {
            NetworkInterfaceConfigTemplates = new ObservableCollection<TemplateNetworkInterfaceConfig>();

            if (File.Exists(NetworkInterfaceConfigTemplatesFilePath))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<TemplateNetworkInterfaceConfig>));

                using (FileStream fileStream = new FileStream(NetworkInterfaceConfigTemplatesFilePath, FileMode.Open))
                {
                    ((List<TemplateNetworkInterfaceConfig>)(xmlSerializer.Deserialize(fileStream))).ForEach(template => NetworkInterfaceConfigTemplates.Add(template));
                }
            }

            NetworkInterfaceConfigTemplates.CollectionChanged += NetworkInterfaceConfigTemplates_CollectionChanged;
        }

        private static void NetworkInterfaceConfigTemplates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NetworkInterfaceConfigTemplatesChanged = true;
        }

        public static void SaveNetworkInterfaceConfigTemplates()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<TemplateNetworkInterfaceConfig>));

            using (FileStream fileStream = new FileStream(NetworkInterfaceConfigTemplatesFilePath, FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, new List<TemplateNetworkInterfaceConfig>(NetworkInterfaceConfigTemplates));
            }

            NetworkInterfaceConfigTemplatesChanged = false;
        }

        public static void LoadWakeOnLanTemplates()
        {
            WakeOnLanTemplates = new ObservableCollection<TemplateWakeOnLanInfo>();

            if (File.Exists(WakeOnLanTemplatesFilePath))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<TemplateWakeOnLanInfo>));

                using (FileStream fileStream = new FileStream(WakeOnLanTemplatesFilePath, FileMode.Open))
                {
                    ((List<TemplateWakeOnLanInfo>)(xmlSerializer.Deserialize(fileStream))).ForEach(template => WakeOnLanTemplates.Add(template));
                }
            }

            WakeOnLanTemplates.CollectionChanged += WakeOnLanTemplates_CollectionChanged;
        }

        private static void WakeOnLanTemplates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            WakeOnLanTemplatesChanged = true;
        }

        public static void SaveWakeOnLanTemplates()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<TemplateWakeOnLanInfo>));

            using (FileStream fileStream = new FileStream(WakeOnLanTemplatesFilePath, FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, new List<TemplateWakeOnLanInfo>(WakeOnLanTemplates));
            }

            WakeOnLanTemplatesChanged = false;
        }
        #endregion

        #region Reset
        public static void ResetNetworkInterfaceConfigTemplates()
        {
            if (NetworkInterfaceConfigTemplates == null)
                NetworkInterfaceConfigTemplates = new ObservableCollection<TemplateNetworkInterfaceConfig>();
            else
                NetworkInterfaceConfigTemplates.Clear();
        }

        public static void ResetWakeOnLanTemplates()
        {
            if (WakeOnLanTemplates == null)
                WakeOnLanTemplates = new ObservableCollection<TemplateWakeOnLanInfo>();
            else
                WakeOnLanTemplates.Clear();
        }
        #endregion
    }
}
