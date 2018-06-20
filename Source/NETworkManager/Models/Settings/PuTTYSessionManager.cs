using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace NETworkManager.Models.Settings
{
    public static class PuTTYProfileManager
    {
        public const string ProfilesFileName = "PuTTY.Profiles";

        public static ObservableCollection<PuTTYProfileInfo> Profiles { get; set; }
        public static bool ProfilesChanged { get; set; }

        public static string GetProfilesFilePath()
        {
            return Path.Combine(SettingsManager.GetSettingsLocation(), ProfilesFileName);
        }

        public static List<string> GetProfileGroups()
        {
            List<string> list = new List<string>();

            foreach (PuTTYProfileInfo Profile in Profiles)
            {
                if (!list.Contains(Profile.Group))
                    list.Add(Profile.Group);
            }

            return list;
        }

        public static void Load(bool deserialize = true)
        {
            Profiles = new ObservableCollection<PuTTYProfileInfo>();

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
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PuTTYProfileInfo>));

                using (FileStream fileStream = new FileStream(GetProfilesFilePath(), FileMode.Open))
                {
                    ((List<PuTTYProfileInfo>)(xmlSerializer.Deserialize(fileStream))).ForEach(Profile => AddProfile(Profile));
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
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PuTTYProfileInfo>));

            using (FileStream fileStream = new FileStream(GetProfilesFilePath(), FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, new List<PuTTYProfileInfo>(Profiles));
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

        public static void AddProfile(PuTTYProfileInfo Profile)
        {
            Profiles.Add(Profile);
        }

        public static void RemoveProfile(PuTTYProfileInfo Profile)
        {
            Profiles.Remove(Profile);
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
