using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Serialization;
using NETworkManager.Settings;

namespace NETworkManager.Profiles
{
    public static class ProfileManager
    {
        #region Variables
        /// <summary>
        /// Name of the profiles directory in the %appdata%\NETworkManager or in the portable location.
        /// </summary>
        private static string ProfilesFolderName => "Profiles";

        /// <summary>
        /// Default profile name
        /// </summary>
        private static string ProfilesDefaultFileName => "Default";

        /// <summary>
        /// Profile file extension
        /// </summary>
        public static string ProfilesFileExtension => ".xml";

        /// <summary>
        /// String to identify encrypted profile files
        /// </summary>
        public static string ProfilesEncryptionIdentifier => ".encrypted";

        public static string TagIdentifier => "tag=";

        public static ObservableCollection<ProfileFileInfo> ProfileFiles { get; set; } = new ObservableCollection<ProfileFileInfo>();

        private static ProfileFileInfo _profileFileInfo;

        public static ProfileFileInfo LoadedProfileFile
        {
            get => _profileFileInfo;
            set
            {
                if (value == _profileFileInfo)
                    return;

                _profileFileInfo = value;
            }
        }

        public static ObservableCollection<ProfileInfo> Profiles { get; set; } = new ObservableCollection<ProfileInfo>();
        
        public static bool ProfilesChanged { get; set; }

        public static event EventHandler<ProfileFileInfoArgs> OnProfileFileChangedEvent;

        private static void ProfileFileChanged(ProfileFileInfo profileFileInfo)
        {
            OnProfileFileChangedEvent?.Invoke(null, new ProfileFileInfoArgs(profileFileInfo));
        }
        #endregion

        static ProfileManager()
        {
            // Load files
            LoadProfileFiles();

            Profiles.CollectionChanged += Profiles_CollectionChanged;

            // Load profile
            Load(ProfileFiles[0]);
        }

        #region Profiles locations (default, custom, portable)
        public static string GetDefaultProfilesLocation()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AssemblyManager.Current.Name, ProfilesFolderName);
        }

        public static string GetCustomProfilesLocation()
        {
            return SettingsManager.Current.Profiles_CustomProfilesLocation;
        }

        public static string GetPortableProfilesLocation()
        {
            return Path.Combine(Path.GetDirectoryName(AssemblyManager.Current.Location) ?? throw new InvalidOperationException(), ProfilesFolderName);
        }

        public static string GetProfilesLocation()
        {
            return ConfigurationManager.Current.IsPortable ? GetPortableProfilesLocation() : GetProfilesLocationNotPortable();
        }

        public static string GetProfilesLocationNotPortable()
        {
            var location = GetCustomProfilesLocation();

            if (!string.IsNullOrEmpty(location) && Directory.Exists(location))
                return location;

            return GetDefaultProfilesLocation();
        }
        #endregion

        #region FileName, FilePath
        public static string GetProfilesDefaultFileName()
        {
            return $"{ProfilesDefaultFileName}{ProfilesFileExtension}";
        }

        public static string GetProfilesDefaultFilePath()
        {
            var path = Path.Combine(GetProfilesLocation(), GetProfilesDefaultFileName());
            
            return path;
        }
        #endregion

        #region Get profile files, load profile files, refresh profile files  
        private static IEnumerable<string> GetProfileFiles(string location)
        {
            return Directory.GetFiles(location).Where(x => Path.GetExtension(x) == ProfilesFileExtension);
        }

        private static void LoadProfileFiles()
        {
            var location = GetProfilesLocation();

            if (Directory.Exists(location))
            {
                foreach (var file in GetProfileFiles(location))
                {
                    var isEncryptionEnabled = Path.GetFileNameWithoutExtension(file).EndsWith(ProfilesEncryptionIdentifier);

                    var name = Path.GetFileNameWithoutExtension(file);

                    if (isEncryptionEnabled)
                        name = name.Substring(0, name.Length - ProfilesEncryptionIdentifier.Length);

                    ProfileFiles.Add(new ProfileFileInfo(name, file, isEncryptionEnabled));
                }
            }

            // Create default
            if (ProfileFiles.Count == 0)
                ProfileFiles.Add(new ProfileFileInfo(ProfilesDefaultFileName, GetProfilesDefaultFilePath()));
        }

        private static void RefreshProfileFiles()
        {
            ProfileFiles.Clear();

            LoadProfileFiles();
        }
        #endregion

        #region Add profile file, edit profile file, delete profile file
        public static void AddProfileFile(string profileName)
        {
            Save(new ProfileFileInfo(profileName, Path.Combine(GetDefaultProfilesLocation(), $"{profileName}{ProfilesFileExtension}")), new List<ProfileInfo>());

            RefreshProfileFiles();

            SwitchProfile(ProfileFiles.FirstOrDefault(x => x.Name == profileName));

            ProfileFileChanged(LoadedProfileFile);
        }

        public static void RenameProfileFile(ProfileFileInfo profileFileInfo, string newProfileName)
        {
            bool switchProfile = false;

            if (LoadedProfileFile.Equals(profileFileInfo))
            {
                Save();

                switchProfile = true;
            }

            ProfileFileInfo newProfileFileInfo = new ProfileFileInfo(newProfileName, Path.Combine(GetProfilesLocation(), profileFileInfo.IsEncryptionEnabled ? $"{newProfileName}{ProfilesEncryptionIdentifier}{ProfilesFileExtension}" : $"{newProfileName}{ProfilesFileExtension}"), profileFileInfo.IsEncryptionEnabled)
            {
                Password = profileFileInfo.Password,
            };

            File.Move(profileFileInfo.Path, newProfileFileInfo.Path);

            RefreshProfileFiles();

            if (switchProfile)
                SwitchProfile(newProfileFileInfo, false);

            ProfileFileChanged(LoadedProfileFile);
        }

        public static void DeleteProfileFile(ProfileFileInfo profileFileInfo)
        {
            if (LoadedProfileFile.Equals(profileFileInfo))
                SwitchProfile(ProfileFiles.FirstOrDefault(x => !x.Equals(profileFileInfo)));

            File.Delete(profileFileInfo.Path);

            RefreshProfileFiles();

            ProfileFileChanged(LoadedProfileFile);
        }
        #endregion

        #region Enable encryption, disable encryption, change master password

        #endregion

        #region Load profile, save profile
        private static void Load(ProfileFileInfo info)
        {
            if (File.Exists(info.Path))
                DeserializeFromFile(info.Path).ForEach(AddProfile);

            ProfilesChanged = false;

            LoadedProfileFile = info;
        }

        public static void Save(ProfileFileInfo profileFileInfo = null, List<ProfileInfo> profiles = null)
        {
            var location = GetProfilesLocation();

            if (!Directory.Exists(location))
                Directory.CreateDirectory(location);

            if (profileFileInfo == null)
                profileFileInfo = LoadedProfileFile;

            if (profiles == null)
                profiles = new List<ProfileInfo>(Profiles);

            SerializeToFile(profileFileInfo.Path, profiles);

            ProfilesChanged = false;
        }
        #endregion

        #region Deserialize, serialize to file
        private static List<ProfileInfo> DeserializeFromFile(string filePath)
        {
            var profiles = new List<ProfileInfo>();

            var xmlSerializer = new XmlSerializer(typeof(List<ProfileInfo>));

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                ((List<ProfileInfo>)xmlSerializer.Deserialize(fileStream)).ForEach(x => profiles.Add(x));
            }

            return profiles;
        }

        private static void SerializeToFile(string filePath, List<ProfileInfo> profiles)
        {
            var xmlSerializer = new XmlSerializer(typeof(List<ProfileInfo>));

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, profiles);
            }
        }
        #endregion

        #region Switch profile
        public static void SwitchProfile(ProfileFileInfo info, bool saveLoadedProfiles = true)
        {
            if (saveLoadedProfiles && LoadedProfileFile != null && ProfilesChanged)
                Save();

            Profiles.Clear();

            Load(info);
        }
        #endregion

        #region Move profiles
        public static Task MoveProfilesAsync(string targedLocation)
        {
            return Task.Run(() => MoveProfiles(targedLocation));
        }

        private static void MoveProfiles(string targedLocation)
        {
            Save();

            // Create the directory
            if (!Directory.Exists(targedLocation))
                Directory.CreateDirectory(targedLocation);

            var sourceFiles = GetProfileFiles(GetProfilesLocation());

            // Copy files
            foreach (var file in sourceFiles)
                File.Copy(file, Path.Combine(targedLocation, Path.GetFileName(file)), true);

            // Delete files
            foreach (var file in sourceFiles)
                File.Delete(file);

            // Delete folder, if it is empty not the default profiles location and does not contain any files or directories
            if (GetProfilesLocation() != GetDefaultProfilesLocation() && Directory.GetFiles(GetProfilesLocation()).Length == 0 && Directory.GetDirectories(GetProfilesLocation()).Length == 0)
                Directory.Delete(GetProfilesLocation());

            RefreshProfileFiles();

            SwitchProfile(ProfileFiles.FirstOrDefault(x => x.Name == LoadedProfileFile.Name), false);

            ProfileFileChanged(LoadedProfileFile);
        }
        #endregion

        #region Reset profiles
        public static void ResetProfiles()
        {
            Profiles.Clear();
        }
        #endregion

        #region Add profile, Remove profile, Rename group
        public static void AddProfile(ProfileInfo profile)
        {
            // Possible fix for appcrash --> when icollection view is refreshed...
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                lock (Profiles)
                    Profiles.Add(profile);
            }));
        }      

        public static void RemoveProfile(ProfileInfo profile)
        {
            // Possible fix for appcrash --> when icollection view is refreshed...
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                lock (Profiles)
                    Profiles.Remove(profile);
            }));
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
                profile.Group = @group?.Trim();

                ProfilesChanged = true;
            }
        }
        #endregion

        #region GetGroups
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
        #endregion

        #region Events
        private static void Profiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ProfilesChanged = true;
        }
        #endregion
    }
}
