using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;

namespace NETworkManager.Models.Settings
{
    public static class NetworkInterfaceProfileManager
    {
        public const string ProfilesFileName = "NetworkInterface.profiles";

        public static ObservableCollection<NetworkInterfaceProfileInfo> Profiles { get; set; }
        public static bool ProfilesChanged { get; set; }

        public static string GetProfilesFilePath()
        {
            return Path.Combine(SettingsManager.GetSettingsLocation(), ProfilesFileName);
        }

        public static List<string> GetProfileGroups()
        {
            List<string> list = new List<string>();

            foreach (NetworkInterfaceProfileInfo profile in Profiles)
            {
                if (!list.Contains(profile.Group))
                    list.Add(profile.Group);
            }

            return list;
        }

        public static void Load(bool deserialize = true)
        {
            Profiles = new ObservableCollection<NetworkInterfaceProfileInfo>();

            if (deserialize)
                Deserialize().ForEach(profile => AddProfile(profile));

            Profiles.CollectionChanged += Profiles_CollectionChanged;
        }

        public static void Import(bool overwrite)
        {
            if (overwrite)
                Profiles.Clear();

            Deserialize().ForEach(profile => AddProfile(profile));
        }

        private static List<NetworkInterfaceProfileInfo> Deserialize()
        {
            List<NetworkInterfaceProfileInfo> list = new List<NetworkInterfaceProfileInfo>();

            if (File.Exists(GetProfilesFilePath()))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<NetworkInterfaceProfileInfo>));

                using (FileStream fileStream = new FileStream(GetProfilesFilePath(), FileMode.Open))
                {
                    ((List<NetworkInterfaceProfileInfo>)(xmlSerializer.Deserialize(fileStream))).ForEach(profile => Profiles.Add(profile));
                }
            }

            return list;
        }

        private static void Profiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<NetworkInterfaceProfileInfo>));

            using (FileStream fileStream = new FileStream(GetProfilesFilePath(), FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, new List<NetworkInterfaceProfileInfo>(Profiles));
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

        public static void AddProfile(NetworkInterfaceProfileInfo profile)
        {
            Profiles.Add(profile);
        }

        public static void RemoveProfile(NetworkInterfaceProfileInfo profile)
        {
            Profiles.Remove(profile);
        }
    }
}
