using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace NETworkManager.Models.Settings
{
    public static class PortScannerProfileManager
    {
        public const string TemplatesFileName = "PortScanner.profiles";

        public static ObservableCollection<PortScannerProfileInfo> Profiles { get; set; }
        public static bool ProfilesChanged { get; set; }

        public static string GetClientsFilePath()
        {
            return Path.Combine(SettingsManager.GetSettingsLocation(), TemplatesFileName);
        }

        public static void Load(bool deserialize = true)
        {
            Profiles = new ObservableCollection<PortScannerProfileInfo>();

            if (deserialize)
                Deserialize().ForEach(profile => AddProfile(profile));

            Profiles.CollectionChanged += Templates_CollectionChanged; ;
        }

        public static void Import(bool overwrite)
        {
            if (overwrite)
                Profiles.Clear();

            Deserialize().ForEach(profile => AddProfile(profile));
        }

        private static List<PortScannerProfileInfo> Deserialize()
        {
            List<PortScannerProfileInfo> list = new List<PortScannerProfileInfo>();

            if (File.Exists(GetClientsFilePath()))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PortScannerProfileInfo>));

                using (FileStream fileStream = new FileStream(GetClientsFilePath(), FileMode.Open))
                {
                    ((List<PortScannerProfileInfo>)(xmlSerializer.Deserialize(fileStream))).ForEach(profile => list.Add(profile));
                }
            }

            return list;
        }

        private static void Templates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ProfilesChanged = true;
        }

        public static void Save()
        {
            Serialize();

            ProfilesChanged = false;
        }

        private static void Serialize()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PortScannerProfileInfo>));

            using (FileStream fileStream = new FileStream(GetClientsFilePath(), FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, new List<PortScannerProfileInfo>(Profiles));
            }
        }

        public static void Reset()
        {
            if (Profiles == null)
                Profiles = new ObservableCollection<PortScannerProfileInfo>();
            else
                Profiles.Clear();
        }

        public static void AddProfile(PortScannerProfileInfo profile)
        {
            Profiles.Add(profile);
        }

        public static void RemoveProfile(PortScannerProfileInfo profile)
        {
            Profiles.Remove(profile);
        }
    }
}
