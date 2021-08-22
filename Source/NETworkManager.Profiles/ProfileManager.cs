using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Serialization;
using NETworkManager.Settings;
using NETworkManager.Utilities;

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
        /// Default profile name.
        /// </summary>
        private static string ProfilesDefaultFileName => "Default";

        /// <summary>
        /// Profile file extension.
        /// </summary>
        public static string ProfileFileExtension => ".xml";

        /// <summary>
        /// Profile file extension for encrypted files.
        /// </summary>
        public static string ProfileFileExtensionEncrypted => ".encrypted";

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

        public static ObservableCollection<GroupInfo> Groups { get; set; } = new ObservableCollection<GroupInfo>();

        public static bool ProfilesChanged { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Event is fired if a loaded profile file is changed.
        /// </summary>
        public static event EventHandler<ProfileFileInfoArgs> OnLoadedProfileFileChangedEvent;

        private static void LoadedProfileFileChanged(ProfileFileInfo profileFileInfo)
        {
            OnLoadedProfileFileChangedEvent?.Invoke(null, new ProfileFileInfoArgs(profileFileInfo));
        }
        #endregion

        static ProfileManager()
        {
            // Load files
            LoadProfileFiles();
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
            return Path.Combine(AssemblyManager.Current.Location ?? throw new InvalidOperationException(), ProfilesFolderName);
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
            return $"{ProfilesDefaultFileName}{ProfileFileExtension}";
        }

        public static string GetProfilesDefaultFilePath()
        {
            var path = Path.Combine(GetProfilesLocation(), GetProfilesDefaultFileName());

            return path;
        }
        #endregion

        #region Get profile files, load profile files, refresh profile files  
        /// <summary>
        /// Get all files in the folder with the extension <see cref="ProfileFileExtension" /> or <see cref="ProfileFileExtensionEncrypted"/>.
        /// </summary>
        /// <param name="location">Path of the profile folder.</param>
        /// <returns>List of profile files.</returns>
        private static IEnumerable<string> GetProfileFiles(string location)
        {
            return Directory.GetFiles(location).Where(x => (Path.GetExtension(x) == ProfileFileExtension || Path.GetExtension(x) == ProfileFileExtensionEncrypted));
        }

        /// <summary>
        /// Method to get the list of profile files from file system and detect if the file is encrypted.
        /// </summary>
        private static void LoadProfileFiles()
        {
            var location = GetProfilesLocation();

            // Folder exists
            if (Directory.Exists(location))
            {
                foreach (var file in GetProfileFiles(location))
                {
                    // Gets the filename, path and if the file is encrypted.
                    ProfileFiles.Add(new ProfileFileInfo(Path.GetFileNameWithoutExtension(file), file, Path.GetFileName(file).EndsWith(ProfileFileExtensionEncrypted)));
                }
            }

            // Create default profile
            if (ProfileFiles.Count == 0)
                ProfileFiles.Add(new ProfileFileInfo(ProfilesDefaultFileName, GetProfilesDefaultFilePath()));
        }
        #endregion

        #region Create empty profile file, rename profile file, delete profile file
        /// <summary>
        /// Method to add a profile file.
        /// </summary>
        /// <param name="profileName"></param>
        public static void CreateEmptyProfileFile(string profileName)
        {
            ProfileFileInfo profileFileInfo = new ProfileFileInfo(profileName, Path.Combine(GetDefaultProfilesLocation(), $"{profileName}{ProfileFileExtension}"));

            CheckAndCreateDirectory();

            SerializeToFile(profileFileInfo.Path, new List<GroupInfo>());

            ProfileFiles.Add(profileFileInfo);
        }

        /// <summary>
        /// Method to rename a profile file.
        /// </summary>
        /// <param name="profileFileInfo"><see cref="ProfileFileInfo"/> to rename.</param>
        /// <param name="newProfileName">New <see cref="ProfileFileInfo.Name"/> of the profile file.</param>
        public static void RenameProfileFile(ProfileFileInfo profileFileInfo, string newProfileName)
        {
            bool switchProfile = false;

            if (LoadedProfileFile != null && LoadedProfileFile.Equals(profileFileInfo))
            {
                Save();

                switchProfile = true;
            }

            ProfileFileInfo newProfileFileInfo = new ProfileFileInfo(newProfileName, Path.Combine(GetProfilesLocation(), newProfileName, Path.GetExtension(profileFileInfo.Path)), profileFileInfo.IsEncrypted)
            {
                Password = profileFileInfo.Password,
                IsPasswordValid = profileFileInfo.IsPasswordValid
            };

            File.Copy(profileFileInfo.Path, newProfileFileInfo.Path);
            ProfileFiles.Add(newProfileFileInfo);

            if (switchProfile)
            {
                SwitchProfile(newProfileFileInfo, false);
                LoadedProfileFileChanged(LoadedProfileFile);
            }

            File.Delete(profileFileInfo.Path);
            ProfileFiles.Remove(profileFileInfo);
        }

        /// <summary>
        /// Method to delete a profile file.
        /// </summary>
        /// <param name="profileFileInfo"></param>
        public static void DeleteProfileFile(ProfileFileInfo profileFileInfo)
        {
            if (LoadedProfileFile != null && LoadedProfileFile.Equals(profileFileInfo))
            {
                SwitchProfile(ProfileFiles.FirstOrDefault(x => !x.Equals(profileFileInfo)));
                LoadedProfileFileChanged(LoadedProfileFile);
            }

            File.Delete(profileFileInfo.Path);
            ProfileFiles.Remove(profileFileInfo);
        }
        #endregion

        #region Enable encryption, disable encryption, change master password
        /// <summary>
        /// Method to enable encryption for a profile file.
        /// </summary>
        /// <param name="profileFileInfo"><see cref="ProfileFileInfo"/> which should be encrypted.</param>
        /// <param name="password">Password to encrypt the profile file.</param>
        public static void EnableEncryption(ProfileFileInfo profileFileInfo, SecureString password)
        {
            // Check if the profile is currently in use
            bool switchProfile = false;

            if (LoadedProfileFile != null && LoadedProfileFile.Equals(profileFileInfo))
            {
                Save();
                switchProfile = true;
            }

            // Create a new profile info with the encryption infos
            var newProfileFileInfo = new ProfileFileInfo(profileFileInfo.Name, Path.ChangeExtension(profileFileInfo.Path, ProfileFileExtensionEncrypted), true)
            {
                Password = password,
                IsPasswordValid = true
            };

            // Load the profiles from the profile file
            var profiles = DeserializeFromFile(profileFileInfo.Path);

            // Save the encrypted file
            byte[] decryptedBytes = SerializeToByteArray(profiles);
            byte[] encryptedBytes = CryptoHelper.Encrypt(decryptedBytes, SecureStringHelper.ConvertToString(newProfileFileInfo.Password), GlobalStaticConfiguration.Profile_EncryptionKeySize, GlobalStaticConfiguration.Profile_EncryptionBlockSize, GlobalStaticConfiguration.Profile_EncryptionIterations);

            File.WriteAllBytes(newProfileFileInfo.Path, encryptedBytes);

            // Add the new profile
            ProfileFiles.Add(newProfileFileInfo);

            // Switch profile, if it was previously loaded
            if (switchProfile)
            {
                SwitchProfile(newProfileFileInfo, false);
                LoadedProfileFileChanged(LoadedProfileFile);
            }

            // Remove the old profile file
            File.Delete(profileFileInfo.Path);
            ProfileFiles.Remove(profileFileInfo);
        }

        /// <summary>
        /// Method to change the master password of an encrypted profile file.
        /// </summary>
        /// <param name="profileFileInfo"><see cref="ProfileFileInfo"/> which should be changed.</param>
        /// <param name="password">Password to decrypt the profile file.</param>
        /// <param name="newPassword">Password to encrypt the profile file.</param>
        public static void ChangeMasterPassword(ProfileFileInfo profileFileInfo, SecureString password, SecureString newPassword)
        {
            // Check if the profile is currently in use
            bool switchProfile = false;

            if (LoadedProfileFile != null && LoadedProfileFile.Equals(profileFileInfo))
            {
                Save();
                switchProfile = true;
            }

            // Create a new profile info with the encryption infos
            var newProfileFileInfo = new ProfileFileInfo(profileFileInfo.Name, Path.ChangeExtension(profileFileInfo.Path, ProfileFileExtensionEncrypted), true)
            {
                Password = newPassword,
                IsPasswordValid = true
            };

            // Load and decrypt the profiles from the profile file
            var encryptedBytes = File.ReadAllBytes(profileFileInfo.Path);
            var decryptedBytes = CryptoHelper.Decrypt(encryptedBytes, SecureStringHelper.ConvertToString(password), GlobalStaticConfiguration.Profile_EncryptionKeySize, GlobalStaticConfiguration.Profile_EncryptionBlockSize, GlobalStaticConfiguration.Profile_EncryptionIterations);
            var profiles = DeserializeFromByteArray(decryptedBytes);

            // Save the encrypted file
            decryptedBytes = SerializeToByteArray(profiles);
            encryptedBytes = CryptoHelper.Encrypt(decryptedBytes, SecureStringHelper.ConvertToString(newProfileFileInfo.Password), GlobalStaticConfiguration.Profile_EncryptionKeySize, GlobalStaticConfiguration.Profile_EncryptionBlockSize, GlobalStaticConfiguration.Profile_EncryptionIterations);

            File.WriteAllBytes(newProfileFileInfo.Path, encryptedBytes);

            // Add the new profile
            ProfileFiles.Add(newProfileFileInfo);


            // Switch profile, if it was previously loaded
            if (switchProfile)
            {
                SwitchProfile(newProfileFileInfo, false);
                LoadedProfileFileChanged(LoadedProfileFile);
            }

            // Remove the old profile file
            ProfileFiles.Remove(profileFileInfo);
        }

        /// <summary>
        /// Method to disable encryption for a profile file.
        /// </summary>
        /// <param name="profileFileInfo"><see cref="ProfileFileInfo"/> which should be decrypted.</param>
        /// <param name="password">Password to decrypt the profile file.</param>
        public static void DisableEncryption(ProfileFileInfo profileFileInfo, SecureString password)
        {
            // Check if the profile is currently in use
            bool switchProfile = false;

            if (LoadedProfileFile != null && LoadedProfileFile.Equals(profileFileInfo))
            {
                Save();
                switchProfile = true;
            }

            // Create a new profile info
            var newProfileFileInfo = new ProfileFileInfo(profileFileInfo.Name, Path.ChangeExtension(profileFileInfo.Path, ProfileFileExtension));

            // Load and decrypt the profiles from the profile file
            var encryptedBytes = File.ReadAllBytes(profileFileInfo.Path);
            var decryptedBytes = CryptoHelper.Decrypt(encryptedBytes, SecureStringHelper.ConvertToString(password), GlobalStaticConfiguration.Profile_EncryptionKeySize, GlobalStaticConfiguration.Profile_EncryptionBlockSize, GlobalStaticConfiguration.Profile_EncryptionIterations);
            var profiles = DeserializeFromByteArray(decryptedBytes);

            // Save the decrypted profiles to the profile file
            SerializeToFile(newProfileFileInfo.Path, profiles);

            // Add the new profile
            ProfileFiles.Add(newProfileFileInfo);

            // Switch profile, if it was previously loaded
            if (switchProfile)
            {
                SwitchProfile(newProfileFileInfo, false);
                LoadedProfileFileChanged(LoadedProfileFile);
            }

            // Remove the old profile file
            File.Delete(profileFileInfo.Path);
            ProfileFiles.Remove(profileFileInfo);
        }
        #endregion

        #region Load, save and switch profile
        /// <summary>
        /// Method to load profiles based on the infos provided in the <see cref="ProfileFileInfo"/>.
        /// </summary>
        /// <param name="profileFileInfo"><see cref="ProfileFileInfo"/> to be loaded.</param>
        private static void Load(ProfileFileInfo profileFileInfo)
        {
            bool loadedProfileUpdated = false;

            if (File.Exists(profileFileInfo.Path))
            {
                if (profileFileInfo.IsEncrypted)
                {
                    var encryptedBytes = File.ReadAllBytes(profileFileInfo.Path);
                    var decryptedBytes = CryptoHelper.Decrypt(encryptedBytes, SecureStringHelper.ConvertToString(profileFileInfo.Password), GlobalStaticConfiguration.Profile_EncryptionKeySize, GlobalStaticConfiguration.Profile_EncryptionBlockSize, GlobalStaticConfiguration.Profile_EncryptionIterations);


                    DeserializeFromByteArray(decryptedBytes).ForEach(AddGroup);

                    // Password is valid
                    ProfileFiles.FirstOrDefault(x => x.Equals(profileFileInfo)).IsPasswordValid = true;
                    profileFileInfo.IsPasswordValid = true;
                    loadedProfileUpdated = true;
                }
                else
                {
                    DeserializeFromFile(profileFileInfo.Path).ForEach(AddGroup);
                }
            }
            else
            {
                // Don't throw an error if it's the default file.                
                if (profileFileInfo.Path != GetProfilesDefaultFilePath())
                    throw new FileNotFoundException($"{profileFileInfo.Path} could not be found!");
            }

            ProfilesChanged = false;

            LoadedProfileFile = profileFileInfo;

            if (loadedProfileUpdated)
                LoadedProfileFileChanged(LoadedProfileFile);
        }

        /// <summary>
        /// Method to save the currently loaded profiles based on the infos provided in the <see cref="ProfileFileInfo"/>.
        /// </summary>
        public static void Save()
        {
            if (LoadedProfileFile == null)
                return;

            CheckAndCreateDirectory();

            // Write to an xml file.
            if (LoadedProfileFile.IsEncrypted)
            {
                // Only if the password provided earlier was valid...
                if (LoadedProfileFile.IsPasswordValid)
                {
                    byte[] decryptedBytes = SerializeToByteArray(new List<GroupInfo>(Groups));
                    byte[] encryptedBytes = CryptoHelper.Encrypt(decryptedBytes, SecureStringHelper.ConvertToString(LoadedProfileFile.Password), GlobalStaticConfiguration.Profile_EncryptionKeySize, GlobalStaticConfiguration.Profile_EncryptionBlockSize, GlobalStaticConfiguration.Profile_EncryptionIterations);

                    File.WriteAllBytes(LoadedProfileFile.Path, encryptedBytes);
                }
            }
            else
            {
                SerializeToFile(LoadedProfileFile.Path, new List<GroupInfo>(Groups));
            }

            ProfilesChanged = false;
        }

        public static void SwitchProfile(ProfileFileInfo info, bool saveLoadedProfiles = true)
        {
            if (saveLoadedProfiles && LoadedProfileFile != null && ProfilesChanged)
                Save();

            Reset();

            Load(info);
        }

        private static void CheckAndCreateDirectory()
        {
            var location = GetProfilesLocation();

            if (!Directory.Exists(location))
                Directory.CreateDirectory(location);
        }

        #endregion

        #region Serialize
        private static void SerializeToFile(string filePath, List<GroupInfo> groups)
        {
            List<GroupInfoSerializable> groupsSerializable = new();

            string groupRemoteDesktopPassword = string.Empty;

            foreach (GroupInfo group in groups)
            {
                List<ProfileInfoSerializable> profilesSerializable = new();

                string profileRemoteDesktopPassword = string.Empty;

                foreach (ProfileInfo profile in group.Profiles)
                {
                    if (profile.RemoteDesktop_Password != null)
                        profileRemoteDesktopPassword = SecureStringHelper.ConvertToString(profile.RemoteDesktop_Password);

                    profilesSerializable.Add(new ProfileInfoSerializable(profile)
                    {
                        RemoteDesktop_Password = profileRemoteDesktopPassword
                    });
                }

                if (group.RemoteDesktop_Password != null)
                    groupRemoteDesktopPassword = SecureStringHelper.ConvertToString(group.RemoteDesktop_Password);

                groupsSerializable.Add(new GroupInfoSerializable(group)
                {
                    Profiles = profilesSerializable,
                    RemoteDesktop_Password = groupRemoteDesktopPassword
                });
            }

            var xmlSerializer = new XmlSerializer(typeof(List<GroupInfoSerializable>));

            using var fileStream = new FileStream(filePath, FileMode.Create);

            xmlSerializer.Serialize(fileStream, groupsSerializable);
        }

        private static byte[] SerializeToByteArray(List<GroupInfo> groups)
        {
            List<GroupInfoSerializable> groupsSerializable = new();

            string groupRemoteDesktopPassword = string.Empty;

            foreach (GroupInfo group in groups)
            {                
                List<ProfileInfoSerializable> profilesSerializable = new();

                string profileRemoteDesktopPassword = string.Empty;

                foreach (ProfileInfo profile in group.Profiles)
                {
                    if (profile.RemoteDesktop_Password != null)
                        profileRemoteDesktopPassword = SecureStringHelper.ConvertToString(profile.RemoteDesktop_Password);

                    profilesSerializable.Add(new ProfileInfoSerializable(profile)
                    {
                        RemoteDesktop_Password = profileRemoteDesktopPassword
                    });
                }

                if (group.RemoteDesktop_Password != null)
                    groupRemoteDesktopPassword = SecureStringHelper.ConvertToString(group.RemoteDesktop_Password);

                groupsSerializable.Add(new GroupInfoSerializable(group)
                {
                    Profiles = profilesSerializable,
                    RemoteDesktop_Password = groupRemoteDesktopPassword
                });
            }

            var xmlSerializer = new XmlSerializer(typeof(List<GroupInfoSerializable>));

            using var memoryStream = new MemoryStream();

            using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);

            xmlSerializer.Serialize(streamWriter, groupsSerializable);

            return memoryStream.ToArray();
        }
        #endregion

        #region Deserialize
        private static List<GroupInfo> DeserializeFromFile(string filePath)
        {
            List<GroupInfo> groups = new();

            XmlSerializer xmlSerializer = new(typeof(List<GroupInfoSerializable>));

            using (FileStream fileStream = new(filePath, FileMode.Open))
            {
                foreach (GroupInfoSerializable groupSerializable in (List<GroupInfoSerializable>)xmlSerializer.Deserialize(fileStream))
                {
                    ObservableCollection<ProfileInfo> profiles = new();

                    foreach (ProfileInfoSerializable profileSerializable in groupSerializable.Profiles)
                    {
                        ProfileInfo profile = new(profileSerializable)
                        {
                            RemoteDesktop_Password = !string.IsNullOrEmpty(profileSerializable.RemoteDesktop_Password) ? SecureStringHelper.ConvertToSecureString(profileSerializable.RemoteDesktop_Password) : null
                        };

                        profiles.Add(profile);
                    }

                    groups.Add(new(groupSerializable)
                    {
                        Profiles = profiles,

                        // Convert passwort to secure string
                        RemoteDesktop_Password = !string.IsNullOrEmpty(groupSerializable.RemoteDesktop_Password) ? SecureStringHelper.ConvertToSecureString(groupSerializable.RemoteDesktop_Password) : null
                    });
                }
            }

            return groups;
        }

        private static List<GroupInfo> DeserializeFromByteArray(byte[] xml)
        {
            List<GroupInfo> groups = new();

            XmlSerializer xmlSerializer = new(typeof(List<GroupInfoSerializable>));

            using var memoryStream = new MemoryStream(xml);

            foreach (GroupInfoSerializable groupSerializable in (List<GroupInfoSerializable>)xmlSerializer.Deserialize(memoryStream))
            {
                ObservableCollection<ProfileInfo> profiles = new();

                foreach (ProfileInfoSerializable profileSerializable in groupSerializable.Profiles)
                {
                    ProfileInfo profile = new(profileSerializable)
                    {
                        RemoteDesktop_Password = !string.IsNullOrEmpty(profileSerializable.RemoteDesktop_Password) ? SecureStringHelper.ConvertToSecureString(profileSerializable.RemoteDesktop_Password) : null
                    };

                    profiles.Add(profile);
                }

                groups.Add(new(groupSerializable)
                {
                    Profiles = profiles,

                    // Convert passwort to secure string
                    RemoteDesktop_Password = !string.IsNullOrEmpty(groupSerializable.RemoteDesktop_Password) ? SecureStringHelper.ConvertToSecureString(groupSerializable.RemoteDesktop_Password) : null
                });
            }

            return groups;
        }
        #endregion               

        #region Move profiles
        public static Task MoveProfilesAsync(string targedLocation, bool overwrite)
        {
            return Task.Run(() => MoveProfiles(targedLocation, overwrite));
        }

        private static void MoveProfiles(string targedLocation, bool overwrite)
        {
            // Save the current profile
            Save();

            // Create the directory
            if (!Directory.Exists(targedLocation))
                Directory.CreateDirectory(targedLocation);

            // Copy files
            foreach (var profileFile in ProfileFiles)
                File.Copy(profileFile.Path, Path.Combine(targedLocation, Path.GetFileName(profileFile.Path)), overwrite);

            // Remove old profile files
            foreach (var profileFile in ProfileFiles)
                File.Delete(profileFile.Path);

            // Delete folder, if it is empty
            var profileLocation = GetProfilesLocation();

            if (Directory.GetFiles(profileLocation).Length == 0 && Directory.GetDirectories(profileLocation).Length == 0)
                Directory.Delete(profileLocation);
        }
        #endregion

        #region Reset groups / profiles
        public static void Reset()
        {
            Groups.Clear();

            ProfilesHasChanged();
        }
        #endregion

        #region Add group, remove group, get groups
        public static void AddGroup(GroupInfo group)
        {
            // Possible fix for appcrash --> when icollection view is refreshed...
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                //lock (Profiles)
                Groups.Add(group);
            }));

            ProfilesHasChanged();
        }

        public static void RemoveGroup(GroupInfo group)
        {
            // Possible fix for appcrash --> when icollection view is refreshed...
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                //lock (Profiles)
                Groups.Remove(group);
            }));

            ProfilesHasChanged();
        }

        /// <summary>
        /// Method to get a list of all groups.
        /// </summary>
        /// <returns>List of groups.</returns>
        public static List<string> GetGroups()
        {
            var list = new List<string>();

            foreach (var groups in Groups)
                list.Add(groups.Name);

            return list;
        }

        public static bool GroupExists(string name)
        {
            foreach (GroupInfo group in Groups)
            {
                if (group.Name == name)
                    return true;
            }

            return false;
        }
        public static bool GroupProfilesIsEmpty(string name)
        {
            return Groups.FirstOrDefault(x => x.Name == name).Profiles.Count == 0;
        }

        #endregion

        #region Add profile, remove profile
        /// <summary>
        /// Add a profile to a group.
        /// </summary>
        /// <param name="profile"><see cref="ProfileInfo"/> to add.</param>
        public static void AddProfile(ProfileInfo profile)
        {
            if (!GroupExists(profile.Group))
                AddGroup(new GroupInfo(profile.Group));

            // Possible fix for appcrash --> when icollection view is refreshed...
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                //lock (Profiles)
                Groups.First(x => x.Name.Equals(profile.Group)).Profiles.Add(profile);
            }));

            ProfilesHasChanged();
        }

        /// <summary>
        /// Remove a profile from a group.
        /// </summary>
        /// <param name="profile"><see cref="ProfileInfo"/> to remove.</param>
        public static void RemoveProfile(ProfileInfo profile)
        {
            // Possible fix for appcrash --> when icollection view is refreshed...
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                //lock (Profiles)
                Groups.First(x => x.Name.Equals(profile.Group)).Profiles.Remove(profile);
            }));

            ProfilesHasChanged();
        }
        #endregion

        /// <summary>
        /// Method to rename a group.
        /// </summary>
        /// <param name="oldGroup">Old name of the group.</param>
        /// <param name="group">New name of the group.</param>
        /*
        public static void RenameGroup(string oldGroup, string group)
        {
            // Go through all groups
            foreach (var profile in Groups)
            {
                // Find specific group
                if (profile.Group != oldGroup)
                    continue;

                // Rename the group
                profile.Group = @group?.Trim();

                ProfilesChanged = true;
            }
        }
        */

        public static void ProfilesHasChanged()
        {
            ProfilesChanged = true;
        }
    }
}
