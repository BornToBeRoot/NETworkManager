using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace NETworkManager.Models.Settings
{
    public static class PortScannerProfileManager
    {
        public const string ProfilesFileName = "PortScanner.profiles";

        public static ObservableCollection<PortScannerProfileInfo> Profiles { get; set; }
        public static bool ProfilesChanged { get; set; }

        public static string GetProfilesFilePath()
        {
            return Path.Combine(SettingsManager.GetSettingsLocation(), ProfilesFileName);
        }

        public static void Load(bool deserialize = true)
        {
            Profiles = new ObservableCollection<PortScannerProfileInfo>();

            if (deserialize)
            {
                Deserialize().ForEach(profile => AddProfile(profile));

                // Add default profiles
                if (Profiles.Count == 0)
                    GetDefaultProfiles().ForEach(profile => AddProfile(profile));
            }

            Profiles.CollectionChanged += Templates_CollectionChanged; ;
        }

        private static List<PortScannerProfileInfo> GetDefaultProfiles()
        {
            return new List<PortScannerProfileInfo>
            {
                new PortScannerProfileInfo("Webserver", "80; 443; 8080; 8443"),
                new PortScannerProfileInfo("FTP","20; 21")
            };
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

            if (File.Exists(GetProfilesFilePath()))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PortScannerProfileInfo>));

                using (FileStream fileStream = new FileStream(GetProfilesFilePath(), FileMode.Open))
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

            using (FileStream fileStream = new FileStream(GetProfilesFilePath(), FileMode.Create))
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

            // Add default profiles
            if (Profiles.Count == 0)
                GetDefaultProfiles().ForEach(profile => AddProfile(profile));
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
