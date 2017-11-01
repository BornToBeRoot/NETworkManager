using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace NETworkManager.Models.Settings
{
    public static class IPScannerProfileManager
    {
        public const string ProfilesFileName = "IPScanner.profiles";

        public static ObservableCollection<IPScannerProfileInfo> Profiles { get; set; }
        public static bool ProfilesChanged { get; set; }

        public static string GetProfilesFilePath()
        {
            return Path.Combine(SettingsManager.GetSettingsLocation(), ProfilesFileName);
        }

        public static List<string> GetProfileGroups()
        {
            List<string> list = new List<string>();

            foreach (IPScannerProfileInfo profile in Profiles)
            {
                if (!list.Contains(profile.Group))
                    list.Add(profile.Group);
            }

            return list;
        }

        public static void Load(bool deserialize = true)
        {
            Profiles = new ObservableCollection<IPScannerProfileInfo>();

            if (deserialize)
                Deserialize().ForEach(profile => AddProfile(profile));

            Profiles.CollectionChanged += Templates_CollectionChanged;
        }

        public static void Import(bool overwrite)
        {
            if (overwrite)
                Profiles.Clear();

            Deserialize().ForEach(profile => AddProfile(profile));
        }

        private static List<IPScannerProfileInfo> Deserialize()
        {
            List<IPScannerProfileInfo> list = new List<IPScannerProfileInfo>();

            if (File.Exists(GetProfilesFilePath()))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<IPScannerProfileInfo>));

                using (FileStream fileStream = new FileStream(GetProfilesFilePath(), FileMode.Open))
                {
                    ((List<IPScannerProfileInfo>)(xmlSerializer.Deserialize(fileStream))).ForEach(profile => list.Add(profile));
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
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<IPScannerProfileInfo>));

            using (FileStream fileStream = new FileStream(GetProfilesFilePath(), FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, new List<IPScannerProfileInfo>(Profiles));
            }
        }

        public static void Reset()
        {
            if (Profiles == null)
            {
                Load(false);
                ProfilesChanged = true;
            }
            else
            {
                Profiles.Clear();
            }
        }

        public static void AddProfile(IPScannerProfileInfo profile)
        {
            Profiles.Add(profile);
        }

        public static void RemoveProfile(IPScannerProfileInfo profile)
        {
            Profiles.Remove(profile);
        }
    }
}
