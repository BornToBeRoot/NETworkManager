using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace NETworkManager.Models.Settings
{
    public static class PingProfileManager
    {
        public const string ProfilesFileName = "Ping.profiles";

        public static ObservableCollection<PingProfileInfo> Profiles { get; set; }
        public static bool ProfilesChanged { get; set; }

        public static string GetProfilesFilePath()
        {
            return Path.Combine(SettingsManager.GetSettingsLocation(), ProfilesFileName);
        }

        public static List<string> GetProfileGroups()
        {
            List<string> list = new List<string>();

            foreach (PingProfileInfo profile in Profiles)
            {
                if (!list.Contains(profile.Group))
                    list.Add(profile.Group);
            }

            return list;
        }

        public static void Load(bool deserialize = true)
        {
            Profiles = new ObservableCollection<PingProfileInfo>();

            if (deserialize)
                DeserializeFromFile();

            Profiles.CollectionChanged += Profiles_CollectionChanged; ;
        }

        public static void Import(bool overwrite)
        {
            if (overwrite)
                Profiles.Clear();

            DeserializeFromFile();
        }

        private static void DeserializeFromFile()
        {
            if (File.Exists(GetProfilesFilePath()))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PingProfileInfo>));

                using (FileStream fileStream = new FileStream(GetProfilesFilePath(), FileMode.Open))
                {
                    ((List<PingProfileInfo>)(xmlSerializer.Deserialize(fileStream))).ForEach(profile => AddProfile(profile));
                }
            }
        }

        private static void Profiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PingProfileInfo>));

            using (FileStream fileStream = new FileStream(GetProfilesFilePath(), FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, new List<PingProfileInfo>(Profiles));
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

        public static void AddProfile(PingProfileInfo profile)
        {
            Profiles.Add(profile);
        }

        public static void RemoveProfile(PingProfileInfo profile)
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
