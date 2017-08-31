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

        public static string ProfilesFilePath
        {
            get { return Path.Combine(SettingsManager.SettingsLocation, ProfilesFileName); }
        }

        public static void Load()
        {
            Profiles = new ObservableCollection<NetworkInterfaceProfileInfo>();

            if (File.Exists(ProfilesFilePath))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<NetworkInterfaceProfileInfo>));

                using (FileStream fileStream = new FileStream(ProfilesFilePath, FileMode.Open))
                {
                    ((List<NetworkInterfaceProfileInfo>)(xmlSerializer.Deserialize(fileStream))).ForEach(template => Profiles.Add(template));
                }
            }

            Profiles.CollectionChanged += Profiles_CollectionChanged;
        }

        private static void Profiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ProfilesChanged = true;
        }

        public static void Save()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<NetworkInterfaceProfileInfo>));

            using (FileStream fileStream = new FileStream(ProfilesFilePath, FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, new List<NetworkInterfaceProfileInfo>(Profiles));
            }

            ProfilesChanged = false;
        }

        public static void Reset()
        {
            if (Profiles == null)
                Profiles = new ObservableCollection<NetworkInterfaceProfileInfo>();
            else
                Profiles.Clear();
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
