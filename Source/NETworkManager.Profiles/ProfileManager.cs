using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Xml.Serialization;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.Profiles;

public static class ProfileManager
{
    #region Constructor

    /// <summary>
    ///     Static constructor. Load all profile files on startup.
    /// </summary>
    static ProfileManager()
    {
        LoadProfileFiles();
    }

    #endregion

    #region Variables

    /// <summary>
    ///     Profiles directory name.
    /// </summary>
    private const string ProfilesFolderName = "Profiles";

    /// <summary>
    ///     Default profile name.
    /// </summary>
    private const string ProfilesDefaultFileName = "Default";

    /// <summary>
    ///     Profile file extension.
    /// </summary>
    private const string ProfileFileExtension = ".xml";

    /// <summary>
    ///     Profile file extension for encrypted files.
    /// </summary>
    private const string ProfileFileExtensionEncrypted = ".encrypted";

    /// <summary>
    ///     ObservableCollection of all profile files.
    /// </summary>
    public static ObservableCollection<ProfileFileInfo> ProfileFiles { get; set; } = new();

    /// <summary>
    ///     Currently loaded profile file.
    /// </summary>
    private static ProfileFileInfo _loadedProfileFile;

    /// <summary>
    ///     Currently loaded profile file.
    /// </summary>
    public static ProfileFileInfo LoadedProfileFile
    {
        get => _loadedProfileFile;
        private set
        {
            if (Equals(value, _loadedProfileFile))
                return;

            _loadedProfileFile = value;
        }
    }

    /// <summary>
    ///     Currently loaded groups with profiles.
    /// </summary>
    public static List<GroupInfo> Groups { get; set; } = new();

    /// <summary>
    ///     Indicates if profiles have changed.
    /// </summary>
    public static bool ProfilesChanged { get; set; }

    #endregion

    #region Events

    /// <summary>
    ///     Event is fired if the currently loaded <see cref="ProfileFileInfo" /> is changed.
    ///     The <see cref="ProfileFileInfo" /> with the current loaded profile file is passed
    ///     as argument.
    /// </summary>
    public static event EventHandler<ProfileFileInfoArgs> OnLoadedProfileFileChangedEvent;

    /// <summary>
    ///     Method to fire the <see cref="OnLoadedProfileFileChangedEvent" />.
    /// </summary>
    /// <param name="profileFileInfo">Loaded <see cref="ProfileFileInfo" />.</param>
    /// <param name="profileFileUpdating">Indicates if the profile file is updating.</param>
    private static void LoadedProfileFileChanged(ProfileFileInfo profileFileInfo, bool profileFileUpdating = false)
    {
        OnLoadedProfileFileChangedEvent?.Invoke(null, new ProfileFileInfoArgs(profileFileInfo, profileFileUpdating));
    }

    /// <summary>
    ///     Event is fired if the profiles have changed.
    /// </summary>
    public static event EventHandler OnProfilesUpdated;

    /// <summary>
    ///     Method to fire the <see cref="OnProfilesUpdated" />.
    /// </summary>
    private static void ProfilesUpdated()
    {
        ProfilesChanged = true;

        OnProfilesUpdated?.Invoke(null, EventArgs.Empty);
    }

    #endregion

    #region Profiles locations, default paths and file names

    /// <summary>
    ///     Method to get the path of the profiles folder.
    /// </summary>
    /// <returns>Path to the profiles folder.</returns>
    public static string GetProfilesFolderLocation()
    {
        return ConfigurationManager.Current.IsPortable
            ? Path.Combine(AssemblyManager.Current.Location, ProfilesFolderName)
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                AssemblyManager.Current.Name, ProfilesFolderName);
    }

    /// <summary>
    ///     Method to get the default profile file name.
    /// </summary>
    /// <returns>Default profile file name.</returns>
    private static string GetProfilesDefaultFileName()
    {
        return $"{ProfilesDefaultFileName}{ProfileFileExtension}";
    }

    /// <summary>
    ///     Method to get the default profile file path.
    /// </summary>
    /// <returns>Default profile file path.</returns>
    private static string GetProfilesDefaultFilePath()
    {
        return Path.Combine(GetProfilesFolderLocation(), GetProfilesDefaultFileName());
    }

    #endregion

    #region Get and load profile files

    /// <summary>
    ///     Get all files in the folder with the extension <see cref="ProfileFileExtension" /> or
    ///     <see cref="ProfileFileExtensionEncrypted" />.
    /// </summary>
    /// <param name="location">Path of the profile folder.</param>
    /// <returns>List of profile files.</returns>
    private static IEnumerable<string> GetProfileFiles(string location)
    {
        return Directory.GetFiles(location).Where(x =>
            Path.GetExtension(x) == ProfileFileExtension || Path.GetExtension(x) == ProfileFileExtensionEncrypted);
    }

    /// <summary>
    ///     Method to get the list of profile files from file system and detect if the file is encrypted.
    /// </summary>
    private static void LoadProfileFiles()
    {
        var location = GetProfilesFolderLocation();

        // Folder exists
        if (Directory.Exists(location))
            foreach (var file in GetProfileFiles(location))
                // Gets the filename, path and if the file is encrypted.
                ProfileFiles.Add(new ProfileFileInfo(Path.GetFileNameWithoutExtension(file), file,
                    Path.GetFileName(file).EndsWith(ProfileFileExtensionEncrypted)));

        // Create default profile if no profile file exists.
        if (ProfileFiles.Count == 0)
            ProfileFiles.Add(new ProfileFileInfo(ProfilesDefaultFileName, GetProfilesDefaultFilePath()));
    }

    #endregion

    #region Create, rename and delete profile file

    /// <summary>
    ///     Method to create a profile file.
    /// </summary>
    /// <param name="profileName"></param>
    public static void CreateEmptyProfileFile(string profileName)
    {
        ProfileFileInfo profileFileInfo = new(profileName,
            Path.Combine(GetProfilesFolderLocation(), $"{profileName}{ProfileFileExtension}"));

        Directory.CreateDirectory(GetProfilesFolderLocation());

        SerializeToFile(profileFileInfo.Path, new List<GroupInfo>());

        ProfileFiles.Add(profileFileInfo);
    }

    /// <summary>
    ///     Method to rename a profile file.
    /// </summary>
    /// <param name="profileFileInfo"><see cref="ProfileFileInfo" /> to rename.</param>
    /// <param name="newProfileName">New <see cref="ProfileFileInfo.Name" /> of the profile file.</param>
    public static void RenameProfileFile(ProfileFileInfo profileFileInfo, string newProfileName)
    {
        var switchProfile = false;

        if (LoadedProfileFile != null && LoadedProfileFile.Equals(profileFileInfo))
        {
            Save();

            switchProfile = true;
        }

        ProfileFileInfo newProfileFileInfo = new(newProfileName,
            Path.Combine(GetProfilesFolderLocation(), $"{newProfileName}{Path.GetExtension(profileFileInfo.Path)}"),
            profileFileInfo.IsEncrypted)
        {
            Password = profileFileInfo.Password,
            IsPasswordValid = profileFileInfo.IsPasswordValid
        };

        File.Copy(profileFileInfo.Path, newProfileFileInfo.Path);
        ProfileFiles.Add(newProfileFileInfo);

        if (switchProfile)
        {
            Switch(newProfileFileInfo, false);
            LoadedProfileFileChanged(LoadedProfileFile, true);
        }

        File.Delete(profileFileInfo.Path);
        ProfileFiles.Remove(profileFileInfo);
    }

    /// <summary>
    ///     Method to delete a profile file.
    /// </summary>
    /// <param name="profileFileInfo"><see cref="ProfileFileInfo" /> to delete.</param>
    public static void DeleteProfileFile(ProfileFileInfo profileFileInfo)
    {
        // Trigger switch via UI (to get the password if the file is encrypted), if the selected profile file is deleted
        if (LoadedProfileFile != null && LoadedProfileFile.Equals(profileFileInfo))
            LoadedProfileFileChanged(ProfileFiles.FirstOrDefault(x => !x.Equals(profileFileInfo)));

        File.Delete(profileFileInfo.Path);
        ProfileFiles.Remove(profileFileInfo);
    }

    #endregion

    #region Enable, disable encryption and change master password

    /// <summary>
    ///     Method to enable encryption for a profile file.
    /// </summary>
    /// <param name="profileFileInfo"><see cref="ProfileFileInfo" /> which should be encrypted.</param>
    /// <param name="password">Password to encrypt the profile file.</param>
    public static void EnableEncryption(ProfileFileInfo profileFileInfo, SecureString password)
    {
        // Check if the profile is currently in use
        var switchProfile = false;

        if (LoadedProfileFile != null && LoadedProfileFile.Equals(profileFileInfo))
        {
            Save();
            switchProfile = true;
        }

        // Create a new profile info with the encryption infos
        var newProfileFileInfo = new ProfileFileInfo(profileFileInfo.Name,
            Path.ChangeExtension(profileFileInfo.Path, ProfileFileExtensionEncrypted), true)
        {
            Password = password,
            IsPasswordValid = true
        };

        // Load the profiles from the profile file
        var profiles = DeserializeFromFile(profileFileInfo.Path);

        // Save the encrypted file
        var decryptedBytes = SerializeToByteArray(profiles);
        var encryptedBytes = CryptoHelper.Encrypt(decryptedBytes,
            SecureStringHelper.ConvertToString(newProfileFileInfo.Password),
            GlobalStaticConfiguration.Profile_EncryptionKeySize,
            GlobalStaticConfiguration.Profile_EncryptionIterations);

        File.WriteAllBytes(newProfileFileInfo.Path, encryptedBytes);

        // Add the new profile
        ProfileFiles.Add(newProfileFileInfo);

        // Switch profile, if it was previously loaded
        if (switchProfile)
        {
            Switch(newProfileFileInfo, false);
            LoadedProfileFileChanged(LoadedProfileFile, true);
        }

        // Remove the old profile file
        if (profileFileInfo.Path != null)
            File.Delete(profileFileInfo.Path);

        ProfileFiles.Remove(profileFileInfo);
    }

    /// <summary>
    ///     Method to change the master password of an encrypted profile file.
    /// </summary>
    /// <param name="profileFileInfo"><see cref="ProfileFileInfo" /> which should be changed.</param>
    /// <param name="password">Password to decrypt the profile file.</param>
    /// <param name="newPassword">Password to encrypt the profile file.</param>
    public static void ChangeMasterPassword(ProfileFileInfo profileFileInfo, SecureString password,
        SecureString newPassword)
    {
        // Check if the profile is currently in use
        var switchProfile = false;

        if (LoadedProfileFile != null && LoadedProfileFile.Equals(profileFileInfo))
        {
            Save();
            switchProfile = true;
        }

        // Create a new profile info with the encryption infos
        var newProfileFileInfo = new ProfileFileInfo(profileFileInfo.Name,
            Path.ChangeExtension(profileFileInfo.Path, ProfileFileExtensionEncrypted), true)
        {
            Password = newPassword,
            IsPasswordValid = true
        };

        // Load and decrypt the profiles from the profile file
        var encryptedBytes = File.ReadAllBytes(profileFileInfo.Path);
        var decryptedBytes = CryptoHelper.Decrypt(encryptedBytes, SecureStringHelper.ConvertToString(password),
            GlobalStaticConfiguration.Profile_EncryptionKeySize,
            GlobalStaticConfiguration.Profile_EncryptionIterations);
        var profiles = DeserializeFromByteArray(decryptedBytes);

        // Save the encrypted file
        decryptedBytes = SerializeToByteArray(profiles);
        encryptedBytes = CryptoHelper.Encrypt(decryptedBytes,
            SecureStringHelper.ConvertToString(newProfileFileInfo.Password),
            GlobalStaticConfiguration.Profile_EncryptionKeySize,
            GlobalStaticConfiguration.Profile_EncryptionIterations);

        File.WriteAllBytes(newProfileFileInfo.Path, encryptedBytes);

        // Add the new profile
        ProfileFiles.Add(newProfileFileInfo);


        // Switch profile, if it was previously loaded
        if (switchProfile)
        {
            Switch(newProfileFileInfo, false);
            LoadedProfileFileChanged(LoadedProfileFile, true);
        }

        // Remove the old profile file
        ProfileFiles.Remove(profileFileInfo);
    }

    /// <summary>
    ///     Method to disable encryption for a profile file.
    /// </summary>
    /// <param name="profileFileInfo"><see cref="ProfileFileInfo" /> which should be decrypted.</param>
    /// <param name="password">Password to decrypt the profile file.</param>
    public static void DisableEncryption(ProfileFileInfo profileFileInfo, SecureString password)
    {
        // Check if the profile is currently in use
        var switchProfile = false;

        if (LoadedProfileFile != null && LoadedProfileFile.Equals(profileFileInfo))
        {
            Save();
            switchProfile = true;
        }

        // Create a new profile info
        var newProfileFileInfo = new ProfileFileInfo(profileFileInfo.Name,
            Path.ChangeExtension(profileFileInfo.Path, ProfileFileExtension));

        // Load and decrypt the profiles from the profile file
        var encryptedBytes = File.ReadAllBytes(profileFileInfo.Path);
        var decryptedBytes = CryptoHelper.Decrypt(encryptedBytes, SecureStringHelper.ConvertToString(password),
            GlobalStaticConfiguration.Profile_EncryptionKeySize,
            GlobalStaticConfiguration.Profile_EncryptionIterations);
        var profiles = DeserializeFromByteArray(decryptedBytes);

        // Save the decrypted profiles to the profile file
        SerializeToFile(newProfileFileInfo.Path, profiles);

        // Add the new profile
        ProfileFiles.Add(newProfileFileInfo);

        // Switch profile, if it was previously loaded
        if (switchProfile)
        {
            Switch(newProfileFileInfo, false);
            LoadedProfileFileChanged(LoadedProfileFile, true);
        }

        // Remove the old profile file
        File.Delete(profileFileInfo.Path);
        ProfileFiles.Remove(profileFileInfo);
    }

    #endregion

    #region Load, save and switch profile

    /// <summary>
    ///     Method to load profiles based on the infos provided in the <see cref="ProfileFileInfo" />.
    /// </summary>
    /// <param name="profileFileInfo"><see cref="ProfileFileInfo" /> to be loaded.</param>
    private static void Load(ProfileFileInfo profileFileInfo)
    {
        var loadedProfileUpdated = false;

        if (File.Exists(profileFileInfo.Path))
        {
            if (profileFileInfo.IsEncrypted)
            {
                var encryptedBytes = File.ReadAllBytes(profileFileInfo.Path);
                var decryptedBytes = CryptoHelper.Decrypt(encryptedBytes,
                    SecureStringHelper.ConvertToString(profileFileInfo.Password),
                    GlobalStaticConfiguration.Profile_EncryptionKeySize,
                    GlobalStaticConfiguration.Profile_EncryptionIterations);

                AddGroups(DeserializeFromByteArray(decryptedBytes));

                // Password is valid
                ProfileFiles.FirstOrDefault(x => x.Equals(profileFileInfo))!.IsPasswordValid = true;
                profileFileInfo.IsPasswordValid = true;
                loadedProfileUpdated = true;
            }
            else
            {
                AddGroups(DeserializeFromFile(profileFileInfo.Path));
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
            LoadedProfileFileChanged(LoadedProfileFile, true);
    }

    /// <summary>
    ///     Method to save the currently loaded profiles based on the infos provided in the <see cref="ProfileFileInfo" />.
    /// </summary>
    public static void Save()
    {
        if (LoadedProfileFile == null)
            return;

        Directory.CreateDirectory(GetProfilesFolderLocation());

        // Write to an xml file.
        if (LoadedProfileFile.IsEncrypted)
        {
            // Only if the password provided earlier was valid...
            if (LoadedProfileFile.IsPasswordValid)
            {
                var decryptedBytes = SerializeToByteArray([..Groups]);
                var encryptedBytes = CryptoHelper.Encrypt(decryptedBytes,
                    SecureStringHelper.ConvertToString(LoadedProfileFile.Password),
                    GlobalStaticConfiguration.Profile_EncryptionKeySize,
                    GlobalStaticConfiguration.Profile_EncryptionIterations);

                File.WriteAllBytes(LoadedProfileFile.Path, encryptedBytes);
            }
        }
        else
        {
            SerializeToFile(LoadedProfileFile.Path, [..Groups]);
        }

        ProfilesChanged = false;
    }

    /// <summary>
    ///     Method to unload the currently loaded profile file.
    /// </summary>
    /// <param name="saveLoadedProfiles">Save loaded profile file (default is true)</param>
    public static void Unload(bool saveLoadedProfiles = true)
    {
        if (saveLoadedProfiles && LoadedProfileFile != null && ProfilesChanged)
            Save();

        LoadedProfileFile = null;

        Groups.Clear();

        ProfilesUpdated();
    }

    /// <summary>
    ///     Method to switch to another profile file.
    /// </summary>
    /// <param name="info">New <see cref="ProfileFileInfo" /> to load.</param>
    /// <param name="saveLoadedProfiles">Save loaded profile file (default is true)</param>
    public static void Switch(ProfileFileInfo info, bool saveLoadedProfiles = true)
    {
        Unload(saveLoadedProfiles);

        Load(info);
    }

    #endregion

    #region Serialize and deserialize

    /// <summary>
    ///     Method to serialize a list of groups as <see cref="GroupInfo" /> to an xml file.
    /// </summary>
    /// <param name="filePath">Path to an xml file.</param>
    /// <param name="groups">List of the groups as <see cref="GroupInfo" /> to serialize.</param>
    private static void SerializeToFile(string filePath, List<GroupInfo> groups)
    {
        var xmlSerializer = new XmlSerializer(typeof(List<GroupInfoSerializable>));

        using var fileStream = new FileStream(filePath, FileMode.Create);

        xmlSerializer.Serialize(fileStream, SerializeGroup(groups));
    }

    /// <summary>
    ///     Method to serialize a list of groups as <see cref="GroupInfo" /> to a byte array.
    /// </summary>
    /// <param name="groups">List of the groups as <see cref="GroupInfo" /> to serialize.</param>
    /// <returns>Serialized list of groups as <see cref="GroupInfo" /> as byte array.</returns>
    private static byte[] SerializeToByteArray(List<GroupInfo> groups)
    {
        var xmlSerializer = new XmlSerializer(typeof(List<GroupInfoSerializable>));

        using var memoryStream = new MemoryStream();

        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);

        xmlSerializer.Serialize(streamWriter, SerializeGroup(groups));

        return memoryStream.ToArray();
    }

    /// <summary>
    ///     Method to serialize a list of groups as <see cref="GroupInfo" />.
    /// </summary>
    /// <param name="groups">List of the groups as <see cref="GroupInfo" /> to serialize.</param>
    /// <returns>Serialized list of groups as <see cref="GroupInfoSerializable" />.</returns>
    private static List<GroupInfoSerializable> SerializeGroup(List<GroupInfo> groups)
    {
        List<GroupInfoSerializable> groupsSerializable = new();

        foreach (var group in groups)
        {
            // Don't save temp groups
            if (group.IsDynamic)
                continue;

            var profilesSerializable = (from profile in @group.Profiles
                where !profile.IsDynamic
                select new ProfileInfoSerializable(profile)
                {
                    RemoteDesktop_Password = profile.RemoteDesktop_Password != null
                        ? SecureStringHelper.ConvertToString(profile.RemoteDesktop_Password)
                        : string.Empty,
                    RemoteDesktop_GatewayServerPassword = profile.RemoteDesktop_GatewayServerPassword != null
                        ? SecureStringHelper.ConvertToString(profile.RemoteDesktop_GatewayServerPassword)
                        : string.Empty,
                    SNMP_Community = profile.SNMP_Community != null
                        ? SecureStringHelper.ConvertToString(profile.SNMP_Community)
                        : string.Empty,
                    SNMP_Auth = profile.SNMP_Auth != null
                        ? SecureStringHelper.ConvertToString(profile.SNMP_Auth)
                        : string.Empty,
                    SNMP_Priv = profile.SNMP_Priv != null
                        ? SecureStringHelper.ConvertToString(profile.SNMP_Priv)
                        : string.Empty
                }).ToList();

            groupsSerializable.Add(new GroupInfoSerializable(group)
            {
                Profiles = profilesSerializable,
                RemoteDesktop_Password = group.RemoteDesktop_Password != null
                    ? SecureStringHelper.ConvertToString(group.RemoteDesktop_Password)
                    : string.Empty,
                RemoteDesktop_GatewayServerPassword = group.RemoteDesktop_GatewayServerPassword != null
                    ? SecureStringHelper.ConvertToString(group.RemoteDesktop_GatewayServerPassword)
                    : string.Empty,
                SNMP_Community = group.SNMP_Community != null
                    ? SecureStringHelper.ConvertToString(group.SNMP_Community)
                    : string.Empty,
                SNMP_Auth =
                    group.SNMP_Auth != null ? SecureStringHelper.ConvertToString(group.SNMP_Auth) : string.Empty,
                SNMP_Priv =
                    group.SNMP_Priv != null ? SecureStringHelper.ConvertToString(group.SNMP_Priv) : string.Empty
            });
        }

        return groupsSerializable;
    }

    /// <summary>
    ///     Method to deserialize a list of groups as <see cref="GroupInfo" /> from an xml file.
    /// </summary>
    /// <param name="filePath">Path to an xml file.</param>
    /// <returns>List of groups as <see cref="GroupInfo" />.</returns>
    private static List<GroupInfo> DeserializeFromFile(string filePath)
    {
        using FileStream fileStream = new(filePath, FileMode.Open);

        return DeserializeGroup(fileStream);
    }

    /// <summary>
    ///     Method to deserialize a list of groups as <see cref="GroupInfo" /> from a byte array.
    /// </summary>
    /// <param name="xml">Serialized list of groups as <see cref="GroupInfo" /> as byte array.</param>
    /// <returns>List of groups as <see cref="GroupInfo" />.</returns>
    private static List<GroupInfo> DeserializeFromByteArray(byte[] xml)
    {
        using MemoryStream memoryStream = new(xml);

        return DeserializeGroup(memoryStream);
    }

    /// <summary>
    ///     Method to deserialize a list of groups as <see cref="GroupInfo" />.
    /// </summary>
    /// <param name="stream">Stream to deserialize.</param>
    /// <returns>List of groups as <see cref="GroupInfo" />.</returns>
    private static List<GroupInfo> DeserializeGroup(Stream stream)
    {
        XmlSerializer xmlSerializer = new(typeof(List<GroupInfoSerializable>));

        return (from groupSerializable in ((List<GroupInfoSerializable>)xmlSerializer.Deserialize(stream))!
            let profiles = groupSerializable.Profiles.Select(profileSerializable => new ProfileInfo(profileSerializable)
                {
                    // Migrate old data
                    NetworkInterface_Subnetmask =
                        string.IsNullOrEmpty(profileSerializable.NetworkInterface_Subnetmask) &&
                        !string.IsNullOrEmpty(profileSerializable.NetworkInterface_SubnetmaskOrCidr)
                            ? profileSerializable.NetworkInterface_SubnetmaskOrCidr
                            : profileSerializable.NetworkInterface_Subnetmask,

                    // Convert passwords to secure strings
                    RemoteDesktop_Password = !string.IsNullOrEmpty(profileSerializable.RemoteDesktop_Password)
                        ? SecureStringHelper.ConvertToSecureString(profileSerializable.RemoteDesktop_Password)
                        : null,
                    RemoteDesktop_GatewayServerPassword =
                        !string.IsNullOrEmpty(profileSerializable.RemoteDesktop_GatewayServerPassword)
                            ? SecureStringHelper.ConvertToSecureString(profileSerializable
                                .RemoteDesktop_GatewayServerPassword)
                            : null,
                    SNMP_Community = !string.IsNullOrEmpty(profileSerializable.SNMP_Community)
                        ? SecureStringHelper.ConvertToSecureString(profileSerializable.SNMP_Community)
                        : null,
                    SNMP_Auth = !string.IsNullOrEmpty(profileSerializable.SNMP_Auth)
                        ? SecureStringHelper.ConvertToSecureString(profileSerializable.SNMP_Auth)
                        : null,
                    SNMP_Priv = !string.IsNullOrEmpty(profileSerializable.SNMP_Priv)
                        ? SecureStringHelper.ConvertToSecureString(profileSerializable.SNMP_Priv)
                        : null
                })
                .ToList()
            select new GroupInfo(groupSerializable)
            {
                Profiles = profiles,

                // Convert passwords to secure strings
                RemoteDesktop_Password = !string.IsNullOrEmpty(groupSerializable.RemoteDesktop_Password)
                    ? SecureStringHelper.ConvertToSecureString(groupSerializable.RemoteDesktop_Password)
                    : null,
                RemoteDesktop_GatewayServerPassword =
                    !string.IsNullOrEmpty(groupSerializable.RemoteDesktop_GatewayServerPassword)
                        ? SecureStringHelper.ConvertToSecureString(
                            groupSerializable.RemoteDesktop_GatewayServerPassword)
                        : null,
                SNMP_Community = !string.IsNullOrEmpty(groupSerializable.SNMP_Community)
                    ? SecureStringHelper.ConvertToSecureString(groupSerializable.SNMP_Community)
                    : null,
                SNMP_Auth = !string.IsNullOrEmpty(groupSerializable.SNMP_Auth)
                    ? SecureStringHelper.ConvertToSecureString(groupSerializable.SNMP_Auth)
                    : null,
                SNMP_Priv = !string.IsNullOrEmpty(groupSerializable.SNMP_Priv)
                    ? SecureStringHelper.ConvertToSecureString(groupSerializable.SNMP_Priv)
                    : null
            }).ToList();
    }

    #endregion

    #region Add, remove, replace group(s) and more.

    /// <summary>
    ///     Method to add a list of <see cref="GroupInfo" /> to the <see cref="Groups" /> list.
    /// </summary>
    /// <param name="groups">List of groups as <see cref="GroupInfo" /> to add.</param>
    private static void AddGroups(List<GroupInfo> groups)
    {
        foreach (var group in groups)
            Groups.Add(group);

        ProfilesUpdated();
    }

    /// <summary>
    ///     Method to add a <see cref="GroupInfo" /> to the <see cref="Groups" /> list.
    /// </summary>
    /// <param name="group">Group as <see cref="GroupInfo" /> to add.</param>
    public static void AddGroup(GroupInfo group)
    {
        Groups.Add(group);

        ProfilesUpdated();
    }

    /// <summary>
    ///     Method to get a group by name.
    /// </summary>
    /// <param name="name">Name of the group.</param>
    /// <returns>Group as <see cref="GroupInfo" />.</returns>
    public static GroupInfo GetGroup(string name)
    {
        return Groups.First(x => x.Name.Equals(name));
    }

    /// <summary>
    ///     Method to replace a group.
    /// </summary>
    /// <param name="oldGroup">Old group as <see cref="GroupInfo" />.</param>
    /// <param name="newGroup">New group as <see cref="GroupInfo" />.</param>
    public static void ReplaceGroup(GroupInfo oldGroup, GroupInfo newGroup)
    {
        Groups.Remove(oldGroup);
        Groups.Add(newGroup);

        ProfilesUpdated();
    }

    /// <summary>
    ///     Method to remove a group.
    /// </summary>
    /// <param name="group">Group as <see cref="GroupInfo" /> to remove</param>
    public static void RemoveGroup(GroupInfo group)
    {
        Groups.Remove(group);

        ProfilesUpdated();
    }

    /// <summary>
    ///     Method to get a list of all group names.
    /// </summary>
    /// <returns>List of group names.</returns>
    public static IReadOnlyCollection<string> GetGroupNames()
    {
        return (from groups in Groups where !groups.IsDynamic select groups.Name).ToList();
    }

    /// <summary>
    ///     Method to check if a profile exists.
    /// </summary>
    /// <param name="name">Name of the group.</param>
    /// <returns>True if the profile exists.</returns>
    public static bool GroupExists(string name)
    {
        return Groups.Any(group => group.Name == name);
    }

    /// <summary>
    ///     Method checks if a group has profiles.
    /// </summary>
    /// <param name="name">Name of the group</param>
    /// <returns>True if the group has no profiles.</returns>
    public static bool IsGroupEmpty(string name)
    {
        return Groups.FirstOrDefault(x => x.Name == name)!.Profiles.Count == 0;
    }

    #endregion

    #region Add, replace and remove profile(s)

    /// <summary>
    ///     Method to add a profile to a group.
    /// </summary>
    /// <param name="profile">Profile as <see cref="ProfileInfo" /> to add.</param>
    public static void AddProfile(ProfileInfo profile)
    {
        if (!GroupExists(profile.Group))
            AddGroup(new GroupInfo(profile.Group));

        Groups.First(x => x.Name.Equals(profile.Group)).Profiles.Add(profile);

        ProfilesUpdated();
    }

    /// <summary>
    ///     Method to replace a profile in a group.
    /// </summary>
    /// <param name="oldProfile">Old profile as <see cref="ProfileInfo" />.</param>
    /// <param name="newProfile">New profile as <see cref="ProfileInfo" />.</param>
    public static void ReplaceProfile(ProfileInfo oldProfile, ProfileInfo newProfile)
    {
        // Remove
        Groups.First(x => x.Name.Equals(oldProfile.Group)).Profiles.Remove(oldProfile);

        // Add
        if (!GroupExists(newProfile.Group))
            AddGroup(new GroupInfo(newProfile.Group));

        Groups.First(x => x.Name.Equals(newProfile.Group)).Profiles.Add(newProfile);

        ProfilesUpdated();
    }

    /// <summary>
    ///     Method to remove a profile from a group.
    /// </summary>
    /// <param name="profile">Profile as <see cref="ProfileInfo" /> to remove.</param>
    public static void RemoveProfile(ProfileInfo profile)
    {
        Groups.First(x => x.Name.Equals(profile.Group)).Profiles.Remove(profile);

        ProfilesUpdated();
    }

    /// <summary>
    ///     Method to remove a list of profiles from a group.
    /// </summary>
    /// <param name="profiles">List of profiles as <see cref="ProfileInfo" /> to remove.</param>
    public static void RemoveProfiles(IEnumerable<ProfileInfo> profiles)
    {
        foreach (var profile in profiles)
            Groups.First(x => x.Name.Equals(profile.Group)).Profiles.Remove(profile);

        ProfilesUpdated();
    }

    #endregion
}