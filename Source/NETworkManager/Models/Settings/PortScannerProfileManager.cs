using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
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

        public static List<string> GetProfileGroups()
        {
            List<string> list = new List<string>();

            foreach (PortScannerProfileInfo profile in Profiles)
            {
                if (!list.Contains(profile.Group))
                    list.Add(profile.Group);
            }

            return list;
        }

        public static void Load(bool deserialize = true)
        {
            Profiles = new ObservableCollection<PortScannerProfileInfo>();

            if (deserialize)
            {
                DeserializeFromFile().ForEach(profile => AddProfile(profile));

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
                new PortScannerProfileInfo("1-1023 (well known)",string.Empty, "1-1023", Application.Current.Resources["String_Default"] as string),
                new PortScannerProfileInfo("FTP",string.Empty,"20; 21", Application.Current.Resources["String_Default"] as string),
                new PortScannerProfileInfo("LDAP/LDAPS", string.Empty,"389; 636", Application.Current.Resources["String_Default"] as string),
                new PortScannerProfileInfo("RDP", string.Empty,"3389", Application.Current.Resources["String_Default"] as string),
                new PortScannerProfileInfo("SSH",string.Empty, "22", Application.Current.Resources["String_Default"] as string),
                new PortScannerProfileInfo("Webserver", string.Empty, "80; 443", Application.Current.Resources["String_Default"] as string),
            };
        }

        public static void Import(bool overwrite)
        {
            if (overwrite)
                Profiles.Clear();

            DeserializeFromFile().ForEach(profile => AddProfile(profile));
        }

        private static List<PortScannerProfileInfo> DeserializeFromFile()
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
            SerializeToFile();

            ProfilesChanged = false;
        }

        private static void SerializeToFile()
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
            {
                Load(false);
                ProfilesChanged = true;
            }
            else
            {
                Profiles.Clear();
            }

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

        public static void RenameGroup(string oldGroup, string group)
        {
            // Go through all groups
            for (int i = 0; i < Profiles.Count; i++)
            {
                // Find specific group
                if (Profiles[i].Group == oldGroup)
                {
                    // Rename the group
                    Profiles[i].Group = group;

                    ProfilesChanged = true;
                }
            }
        }
    }
}
