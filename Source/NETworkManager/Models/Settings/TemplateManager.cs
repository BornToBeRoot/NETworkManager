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
        public static string WakeOnLANTemplatesFileName = "WakeOnLAN" + TemplateFileExtension;

        public static ObservableCollection<TemplateWakeOnLANInfo> WakeOnLANTemplates;
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

        public static string WakeOnLANTemplatesFilePath
        {
            get { return Path.Combine(TemplatesLocation, WakeOnLANTemplatesFileName); }
        }
        #endregion

        public static bool WakeOnLANTemplatesChanged { get; set; }
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

        public static void LoadWakeOnLANTemplates()
        {
            WakeOnLANTemplates = new ObservableCollection<TemplateWakeOnLANInfo>();

            if (File.Exists(WakeOnLANTemplatesFilePath))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<TemplateWakeOnLANInfo>));

                using (FileStream fileStream = new FileStream(WakeOnLANTemplatesFilePath, FileMode.Open))
                {
                    ((List<TemplateWakeOnLANInfo>)(xmlSerializer.Deserialize(fileStream))).ForEach(template => WakeOnLANTemplates.Add(template));
                }
            }

            WakeOnLANTemplates.CollectionChanged += WakeOnLANTemplates_CollectionChanged;
        }

        private static void WakeOnLANTemplates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            WakeOnLANTemplatesChanged = true;
        }

        public static void SaveWakeOnLANTemplates()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<TemplateWakeOnLANInfo>));

            using (FileStream fileStream = new FileStream(WakeOnLANTemplatesFilePath, FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, new List<TemplateWakeOnLANInfo>(WakeOnLANTemplates));
            }

            WakeOnLANTemplatesChanged = false;
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

        public static void ResetWakeOnLANTemplates()
        {
            if (WakeOnLANTemplates == null)
                WakeOnLANTemplates = new ObservableCollection<TemplateWakeOnLANInfo>();
            else
                WakeOnLANTemplates.Clear();
        }
        #endregion
    }
}
