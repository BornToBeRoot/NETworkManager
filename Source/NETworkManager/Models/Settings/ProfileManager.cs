using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using NETworkManager.ViewModels;

namespace NETworkManager.Models.Settings
{
    public static class ProfileManager
    {
        public const string ProfilesFileName = "Profiles.xml";

        public static ObservableCollection<ProfileInfo> Profiles { get; set; }
        public static bool ProfilesChanged { get; set; }

        public static string GetProfilesFilePath()
        {
            return Path.Combine(SettingsManager.GetSettingsLocation(), ProfilesFileName);
        }

        public static List<string> GetGroups()
        {
            var list = new List<string>();

            foreach (var profile in Profiles)
            {
                if (!list.Contains(profile.Group))
                    list.Add(profile.Group);
            }

            return list;
        }

        public static void Load(bool deserialize = true)
        {
            Profiles = new ObservableCollection<ProfileInfo>();

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
            if (!File.Exists(GetProfilesFilePath()))
                return;

            var xmlSerializer = new XmlSerializer(typeof(List<ProfileInfo>));

            using (var fileStream = new FileStream(GetProfilesFilePath(), FileMode.Open))
            {
                ((List<ProfileInfo>)(xmlSerializer.Deserialize(fileStream))).ForEach(AddProfile);
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
            var xmlSerializer = new XmlSerializer(typeof(List<ProfileInfo>));

            using (var fileStream = new FileStream(GetProfilesFilePath(), FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, new List<ProfileInfo>(Profiles));
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

        public static void AddProfile(ProfileInfo profile)
        {
            Profiles.Add(profile);
        }

        public static void AddProfile(ProfileViewModel instance)
        {
            AddProfile(new ProfileInfo()
            {
                Name = instance.Name,
                Host = instance.Host,
                CredentialID = instance.CredentialID,
                Group = instance.Group,
                Tags = instance.Tags,

                NetworkInterface_Enabled = instance.NetworkInterface_Enabled,
                NetworkInterface_EnableStaticIPAddress = instance.NetworkInterface_EnableStaticIPAddress,
                NetworkInterface_IPAddress = instance.NetworkInterface_IPAddress,
                NetworkInterface_Gateway = instance.NetworkInterface_Gateway,
                NetworkInterface_SubnetmaskOrCidr = instance.NetworkInterface_SubnetmaskOrCidr,
                NetworkInterface_EnableStaticDNS = instance.NetworkInterface_EnableStaticDNS,
                NetworkInterface_PrimaryDNSServer = instance.NetworkInterface_PrimaryDNSServer,
                NetworkInterface_SecondaryDNSServer = instance.NetworkInterface_SecondaryDNSServer,

                IPScanner_Enabled = instance.IPScanner_Enabled,
                IPScanner_InheritHost = instance.IPScanner_InheritHost,
                IPScanner_IPRange = instance.IPScanner_InheritHost ? instance.Host : instance.IPScanner_IPRange,

                PortScanner_Enabled = instance.PortScanner_Enabled,
                PortScanner_InheritHost = instance.PortScanner_InheritHost,
                PortScanner_Host = instance.PortScanner_InheritHost ? instance.Host : instance.PortScanner_Host,
                PortScanner_Ports = instance.PortScanner_Ports,

                Ping_Enabled = instance.Ping_Enabled,
                Ping_InheritHost = instance.Ping_InheritHost,
                Ping_Host = instance.Ping_InheritHost ? instance.Host : instance.Ping_Host,

                Traceroute_Enabled = instance.Traceroute_Enabled,
                Traceroute_InheritHost = instance.Traceroute_InheritHost,
                Traceroute_Host = instance.Traceroute_InheritHost ? instance.Host : instance.Traceroute_Host,

                RemoteDesktop_Enabled = instance.RemoteDesktop_Enabled,
                RemoteDesktop_InheritHost = instance.RemoteDesktop_InheritHost,
                RemoteDesktop_Host = instance.RemoteDesktop_InheritHost ? instance.Host : instance.RemoteDesktop_Host,

                PuTTY_Enabled = instance.PuTTY_Enabled,
                PuTTY_ConnectionMode = instance.PuTTY_ConnectionMode,
                PuTTY_InheritHost = instance.PuTTY_InheritHost,
                PuTTY_HostOrSerialLine = instance.PuTTY_ConnectionMode == PuTTY.PuTTY.ConnectionMode.Serial ? instance.PuTTY_SerialLine : (instance.PuTTY_InheritHost ? instance.Host : instance.PuTTY_Host),
                PuTTY_PortOrBaud = instance.PuTTY_ConnectionMode == PuTTY.PuTTY.ConnectionMode.Serial ? instance.PuTTY_Baud : instance.PuTTY_Port,
                PuTTY_Username = instance.PuTTY_Username,
                PuTTY_Profile = instance.PuTTY_Profile,
                PuTTY_AdditionalCommandLine = instance.PuTTY_AdditionalCommandLine,

                WakeOnLAN_Enabled = instance.WakeOnLAN_Enabled,
                WakeOnLAN_MACAddress = instance.WakeOnLAN_MACAddress,
                WakeOnLAN_Broadcast = instance.WakeOnLAN_Broadcast,
                WakeOnLAN_Port = instance.WakeOnLAN_Port
            });
        }

        public static void RemoveProfile(ProfileInfo profile)
        {
            Profiles.Remove(profile);
        }

        public static void RenameGroup(string oldGroup, string group)
        {
            // Go through all groups
            foreach (var profile in Profiles)
            {
                // Find specific group
                if (profile.Group != oldGroup)
                    continue;

                // Rename the group
                profile.Group = @group;

                ProfilesChanged = true;
            }
        }
    }
}
