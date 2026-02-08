using log4net;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace NETworkManager.Profiles;

public static class ProfileManager
{
    #region Variables    
    private static readonly ILog Log = LogManager.GetLogger(typeof(ProfileManager));

    /// <summary>
    ///     Profiles directory name.
    /// </summary>
    private const string ProfilesFolderName = "Profiles";

    /// <summary>
    ///     Profiles backups directory name.
    /// </summary>
    private static string BackupFolderName => "Backups";

    /// <summary>
    ///     Default profile name.
    /// </summary>
    private const string ProfilesDefaultFileName = "Default";

    /// <summary>
    ///     Profile file extension.
    /// </summary>
    private const string ProfileFileExtension = ".json";

    /// <summary>
    ///     Legacy XML profile file extension.
    /// </summary>
    [Obsolete("Legacy XML profiles are no longer used, but the extension is kept for migration purposes.")]
    private static string LegacyProfileFileExtension => ".xml";

    /// <summary>
    ///     Profile file extension for encrypted files.
    /// </summary>
    private const string ProfileFileExtensionEncrypted = ".encrypted";

    /// <summary>
    ///     JSON serializer options for consistent serialization/deserialization.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    ///     Maximum number of bytes to check for XML content detection.
    /// </summary>
    private const int XmlDetectionBufferSize = 200;

    /// <summary>
    ///     ObservableCollection of all profile files.
    /// </summary>
    public static ObservableCollection<ProfileFileInfo> ProfileFiles { get; set; } = [];

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
    ///     Currently loaded profile file data (wrapper containing groups and metadata).
    ///     This is updated during load/save operations.
    /// </summary>
    private static ProfileFileData _loadedProfileFileData = new();

    /// <summary>
    ///     Currently loaded profile file data (wrapper containing groups and metadata).
    ///     This is updated during load/save operations.
    /// </summary>
    public static ProfileFileData LoadedProfileFileData
    {
        get => _loadedProfileFileData;
        private set
        {
            if (Equals(value, _loadedProfileFileData))
                return;

            _loadedProfileFileData = value;
        }
    }

    #endregion

    #region Constructor

    /// <summary>
    ///     Static constructor. Load all profile files on startup.
    /// </summary>
    static ProfileManager()
    {
        LoadProfileFiles();
    }

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
    /// Occurs when the profile migration process begins.
    /// </summary>    
    [Obsolete("Will be removed after some time, as profile migration from legacy XML files is a one-time process.")]
    public static event EventHandler OnProfileMigrationStarted;

    /// <summary>
    /// Raises the event indicating that the profile migration process from legacy XML files has started.
    /// </summary>    
    [Obsolete("Will be removed after some time, as profile migration from legacy XML files is a one-time process.")]
    private static void ProfileMigrationStarted()
    {
        OnProfileMigrationStarted?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Occurs when the profile migration from legacy XML files has completed.
    /// </summary>    
    [Obsolete("Will be removed after some time, as profile migration from legacy XML files is a one-time process.")]
    public static event EventHandler OnProfileMigrationCompleted;

    /// <summary>
    /// Raises the event indicating that the profile migration from legacy XML files has completed.
    /// </summary>    
    [Obsolete("Will be removed after some time, as profile migration from legacy XML files is a one-time process.")]
    private static void ProfileMigrationCompleted()
    {
        OnProfileMigrationCompleted?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    ///     Event is fired if the profiles have changed.
    /// </summary>
    public static event EventHandler OnProfilesUpdated;

    /// <summary>
    ///     Method to fire the <see cref="OnProfilesUpdated" />.
    /// </summary>
    private static void ProfilesUpdated(bool profilesChanged = true)
    {
        LoadedProfileFileData?.ProfilesChanged = profilesChanged;

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
    ///     Method to get the path of the profiles backup folder.
    /// </summary>
    /// <returns>Path to the profiles backup folder.</returns>
    public static string GetProfilesBackupFolderLocation()
    {
        return Path.Combine(GetProfilesFolderLocation(), BackupFolderName);
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
            Path.GetExtension(x) == ProfileFileExtension ||
            Path.GetExtension(x) == ProfileFileExtensionEncrypted ||
            Path.GetExtension(x) == LegacyProfileFileExtension);
    }

    /// <summary>
    ///     Method to get the list of profile files from file system and detect if the file is encrypted.
    /// </summary>
    private static void LoadProfileFiles()
    {
        var location = GetProfilesFolderLocation();

        // Folder exists
        if (Directory.Exists(location))
        {
            foreach (var file in GetProfileFiles(location))
            {
                // Gets the filename, path and if the file is encrypted.
                ProfileFiles.Add(new ProfileFileInfo(Path.GetFileNameWithoutExtension(file), file,
                    Path.GetFileName(file).EndsWith(ProfileFileExtensionEncrypted)));
            }
        }

        // Create default profile if no profile file exists.
        if (ProfileFiles.Count == 0)
            ProfileFiles.Add(new ProfileFileInfo(ProfilesDefaultFileName, GetProfilesDefaultFilePath()));
    }

    #endregion

    #region Create, rename and delete profile file

    /// <summary>
    ///     Method to create a profile file.
    /// </summary>
    /// <param name="profileName">Name of the profile file to create.</param>
    /// <exception cref="ArgumentException">Thrown when profileName is null or empty.</exception>
    public static void CreateEmptyProfileFile(string profileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileName);

        ProfileFileInfo profileFileInfo = new(profileName,
            Path.Combine(GetProfilesFolderLocation(), $"{profileName}{ProfileFileExtension}"));

        Directory.CreateDirectory(GetProfilesFolderLocation());

        // Create and serialize empty ProfileFileData to new file (without loading it)
        var emptyProfileFileData = new ProfileFileData();
        var jsonString = JsonSerializer.Serialize(emptyProfileFileData, JsonOptions);
        File.WriteAllText(profileFileInfo.Path, jsonString);

        ProfileFiles.Add(profileFileInfo);
    }

    /// <summary>
    ///     Method to rename a profile file.
    /// </summary>
    /// <param name="profileFileInfo"><see cref="ProfileFileInfo" /> to rename.</param>
    /// <param name="newProfileName">New <see cref="ProfileFileInfo.Name" /> of the profile file.</param>
    /// <exception cref="ArgumentNullException">Thrown when profileFileInfo is null.</exception>
    /// <exception cref="ArgumentException">Thrown when newProfileName is null or empty.</exception>
    public static void RenameProfileFile(ProfileFileInfo profileFileInfo, string newProfileName)
    {
        ArgumentNullException.ThrowIfNull(profileFileInfo);
        ArgumentException.ThrowIfNullOrWhiteSpace(newProfileName);


        // Check if the profile is currently in use
        var switchProfile = false;

        if (LoadedProfileFile != null && LoadedProfileFile.Equals(profileFileInfo))
        {
            Save();
            switchProfile = true;
        }

        // Create backup
        Backup(profileFileInfo.Path,
               GetProfilesBackupFolderLocation(),
               TimestampHelper.GetTimestampFilename(Path.GetFileName(profileFileInfo.Path)));

        // Create new profile info with the new name
        ProfileFileInfo newProfileFileInfo = new(newProfileName,
            Path.Combine(GetProfilesFolderLocation(), $"{newProfileName}{Path.GetExtension(profileFileInfo.Path)}"),
            profileFileInfo.IsEncrypted)
        {
            Password = profileFileInfo.Password,
            IsPasswordValid = profileFileInfo.IsPasswordValid
        };

        // Copy the profile file to the new location
        File.Copy(profileFileInfo.Path, newProfileFileInfo.Path);
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

    /// <summary>
    ///     Method to delete a profile file.
    /// </summary>
    /// <param name="profileFileInfo"><see cref="ProfileFileInfo" /> to delete.</param>
    /// <exception cref="ArgumentNullException">Thrown when profileFileInfo is null.</exception>
    public static void DeleteProfileFile(ProfileFileInfo profileFileInfo)
    {
        ArgumentNullException.ThrowIfNull(profileFileInfo);


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
    /// <exception cref="ArgumentNullException">Thrown when profileFileInfo or password is null.</exception>
    public static void EnableEncryption(ProfileFileInfo profileFileInfo, SecureString password)
    {
        ArgumentNullException.ThrowIfNull(profileFileInfo);
        ArgumentNullException.ThrowIfNull(password);

        // Check if the profile is currently in use
        var switchProfile = false;

        if (LoadedProfileFile != null && LoadedProfileFile.Equals(profileFileInfo))
        {
            Save();
            switchProfile = true;
        }

        // Create backup
        Backup(profileFileInfo.Path,
               GetProfilesBackupFolderLocation(),
               TimestampHelper.GetTimestampFilename(Path.GetFileName(profileFileInfo.Path)));

        // Create a new profile info with the encryption infos
        var newProfileFileInfo = new ProfileFileInfo(profileFileInfo.Name,
            Path.ChangeExtension(profileFileInfo.Path, ProfileFileExtensionEncrypted), true)
        {
            Password = password,
            IsPasswordValid = true
        };

        // Save current state to prevent corruption
        var previousLoadedProfileFileData = LoadedProfileFileData;

        try
        {
            // Load the existing profile data (temporarily overwrites LoadedProfileFileData)
            if (Path.GetExtension(profileFileInfo.Path) == LegacyProfileFileExtension)
                DeserializeFromXmlFile(profileFileInfo.Path);
            else
                DeserializeFromFile(profileFileInfo.Path);

            // Save the encrypted file
            var decryptedBytes = SerializeToByteArray();
            var encryptedBytes = CryptoHelper.Encrypt(decryptedBytes,
                SecureStringHelper.ConvertToString(newProfileFileInfo.Password),
                GlobalStaticConfiguration.Profile_EncryptionKeySize,
                GlobalStaticConfiguration.Profile_EncryptionIterations);

            File.WriteAllBytes(newProfileFileInfo.Path, encryptedBytes);
        }
        finally
        {
            // Restore previous state if this wasn't the currently loaded profile
            if (!switchProfile)
                LoadedProfileFileData = previousLoadedProfileFileData;
        }

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
    /// <exception cref="ArgumentNullException">Thrown when profileFileInfo, password, or newPassword is null.</exception>
    public static void ChangeMasterPassword(ProfileFileInfo profileFileInfo, SecureString password,
        SecureString newPassword)
    {
        ArgumentNullException.ThrowIfNull(profileFileInfo);
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(newPassword);

        // Check if the profile is currently in use
        var switchProfile = false;

        if (LoadedProfileFile != null && LoadedProfileFile.Equals(profileFileInfo))
        {
            Save();
            switchProfile = true;
        }

        // Create backup
        Backup(profileFileInfo.Path,
               GetProfilesBackupFolderLocation(),
               TimestampHelper.GetTimestampFilename(Path.GetFileName(profileFileInfo.Path)));

        // Create new profile info with the encryption infos
        var newProfileFileInfo = new ProfileFileInfo(profileFileInfo.Name,
            Path.ChangeExtension(profileFileInfo.Path, ProfileFileExtensionEncrypted), true)
        {
            Password = newPassword,
            IsPasswordValid = true
        };

        // Save current state to prevent corruption
        var previousLoadedProfileFileData = LoadedProfileFileData;

        try
        {
            // Load and decrypt the profiles from the profile file (temporarily overwrites LoadedProfileFileData)
            var encryptedBytes = File.ReadAllBytes(profileFileInfo.Path);
            var decryptedBytes = CryptoHelper.Decrypt(encryptedBytes, SecureStringHelper.ConvertToString(password),
                GlobalStaticConfiguration.Profile_EncryptionKeySize,
                GlobalStaticConfiguration.Profile_EncryptionIterations);

            if (IsXmlContent(decryptedBytes))
                DeserializeFromXmlByteArray(decryptedBytes);
            else
                DeserializeFromByteArray(decryptedBytes);

            // Save the encrypted file with new password
            decryptedBytes = SerializeToByteArray();
            encryptedBytes = CryptoHelper.Encrypt(decryptedBytes,
                SecureStringHelper.ConvertToString(newProfileFileInfo.Password),
                GlobalStaticConfiguration.Profile_EncryptionKeySize,
                GlobalStaticConfiguration.Profile_EncryptionIterations);

            File.WriteAllBytes(newProfileFileInfo.Path, encryptedBytes);
        }
        finally
        {
            // Restore previous state if this wasn't the currently loaded profile
            if (!switchProfile)
                LoadedProfileFileData = previousLoadedProfileFileData;
        }

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
    /// <exception cref="ArgumentNullException">Thrown when profileFileInfo or password is null.</exception>
    public static void DisableEncryption(ProfileFileInfo profileFileInfo, SecureString password)
    {
        ArgumentNullException.ThrowIfNull(profileFileInfo);
        ArgumentNullException.ThrowIfNull(password);

        // Check if the profile is currently in use
        var switchProfile = false;

        if (LoadedProfileFile != null && LoadedProfileFile.Equals(profileFileInfo))
        {
            Save();
            switchProfile = true;
        }

        // Create backup
        Backup(profileFileInfo.Path,
            GetProfilesBackupFolderLocation(),
            TimestampHelper.GetTimestampFilename(Path.GetFileName(profileFileInfo.Path)));

        // Create new profile info
        var newProfileFileInfo = new ProfileFileInfo(profileFileInfo.Name,
            Path.ChangeExtension(profileFileInfo.Path, ProfileFileExtension));

        // Save current state to prevent corruption
        var previousLoadedProfileFileData = LoadedProfileFileData;

        try
        {
            // Load and decrypt the profiles from the profile file (temporarily overwrites LoadedProfileFileData)
            var encryptedBytes = File.ReadAllBytes(profileFileInfo.Path);
            var decryptedBytes = CryptoHelper.Decrypt(encryptedBytes, SecureStringHelper.ConvertToString(password),
                GlobalStaticConfiguration.Profile_EncryptionKeySize,
                GlobalStaticConfiguration.Profile_EncryptionIterations);

            if (IsXmlContent(decryptedBytes))
                DeserializeFromXmlByteArray(decryptedBytes);
            else
                DeserializeFromByteArray(decryptedBytes);

            // Save the decrypted profiles to the profile file
            SerializeToFile(newProfileFileInfo.Path);
        }
        finally
        {
            // Restore previous state if this wasn't the currently loaded profile
            if (!switchProfile)
                LoadedProfileFileData = previousLoadedProfileFileData;
        }

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

        Log.Info($"Load profile file: {profileFileInfo.Path}");

        if (File.Exists(profileFileInfo.Path))
        {
            // Encrypted profile file
            if (profileFileInfo.IsEncrypted)
            {
                var encryptedBytes = File.ReadAllBytes(profileFileInfo.Path);
                var decryptedBytes = CryptoHelper.Decrypt(encryptedBytes,
                    SecureStringHelper.ConvertToString(profileFileInfo.Password),
                    GlobalStaticConfiguration.Profile_EncryptionKeySize,
                    GlobalStaticConfiguration.Profile_EncryptionIterations);

                if (IsXmlContent(decryptedBytes))
                {
                    //
                    // MIGRATION FROM LEGACY XML PROFILE FILE
                    //

                    Log.Info($"Legacy XML profile file detected inside encrypted profile: {profileFileInfo.Path}. Migration in progress...");

                    // Load from legacy XML byte array
                    DeserializeFromXmlByteArray(decryptedBytes);

                    // Create a backup of the legacy XML file
                    Backup(profileFileInfo.Path,
                        GetProfilesBackupFolderLocation(),
                        TimestampHelper.GetTimestampFilename(Path.GetFileName(profileFileInfo.Path)));

                    // Save encrypted profile file with new JSON format
                    var newDecryptedBytes = SerializeToByteArray();
                    var newEncryptedBytes = CryptoHelper.Encrypt(newDecryptedBytes,
                        SecureStringHelper.ConvertToString(profileFileInfo.Password),
                        GlobalStaticConfiguration.Profile_EncryptionKeySize,
                        GlobalStaticConfiguration.Profile_EncryptionIterations);

                    File.WriteAllBytes(profileFileInfo.Path, newEncryptedBytes);

                    Log.Info($"Legacy XML profile file migration completed inside encrypted profile: {profileFileInfo.Path}.");
                }
                else
                {
                    DeserializeFromByteArray(decryptedBytes);
                }

                // Password is valid
                ProfileFiles.FirstOrDefault(x => x.Equals(profileFileInfo))!.IsPasswordValid = true;
                profileFileInfo.IsPasswordValid = true;
                loadedProfileUpdated = true;
            }
            // Unencrypted profile file
            else
            {
                if (Path.GetExtension(profileFileInfo.Path) == LegacyProfileFileExtension)
                {
                    //
                    // MIGRATION FROM LEGACY XML PROFILE FILE
                    //
                    Log.Info($"Legacy XML profile file detected: {profileFileInfo.Path}. Migration in progress...");

                    // Load from legacy XML file
                    DeserializeFromXmlFile(profileFileInfo.Path);

                    LoadedProfileFile = profileFileInfo;

                    // Create a backup of the legacy XML file and delete the original
                    Backup(profileFileInfo.Path,
                        GetProfilesBackupFolderLocation(),
                        TimestampHelper.GetTimestampFilename(Path.GetFileName(profileFileInfo.Path)));

                    // Create new profile file info with JSON extension
                    var newProfileFileInfo = new ProfileFileInfo(profileFileInfo.Name,
                        Path.ChangeExtension(profileFileInfo.Path, ProfileFileExtension));

                    // Save new JSON file
                    SerializeToFile(newProfileFileInfo.Path);

                    // Notify migration started
                    ProfileMigrationStarted();

                    // Add the new profile
                    ProfileFiles.Add(newProfileFileInfo);

                    // Switch profile
                    Log.Info($"Switching to migrated profile file: {newProfileFileInfo.Path}.");
                    Switch(newProfileFileInfo, false);
                    LoadedProfileFileChanged(LoadedProfileFile, true);

                    // Remove the old profile file
                    File.Delete(profileFileInfo.Path);
                    ProfileFiles.Remove(profileFileInfo);

                    // Notify migration completed
                    ProfileMigrationCompleted();

                    Log.Info($"Legacy XML profile file migration completed: {profileFileInfo.Path}.");
                    return;
                }
                else
                {
                    DeserializeFromFile(profileFileInfo.Path);
                }
            }
        }
        else
        {
            // Don't throw an error if it's the default file.                
            if (profileFileInfo.Path != GetProfilesDefaultFilePath())
                throw new FileNotFoundException($"{profileFileInfo.Path} could not be found!");
        }

        LoadedProfileFile = profileFileInfo;

        if (loadedProfileUpdated)
            LoadedProfileFileChanged(LoadedProfileFile, true);

        // Notify subscribers that profiles have been loaded/updated
        ProfilesUpdated(false);
    }

    /// <summary>
    ///     Method to save the currently loaded profiles based on the infos provided in the <see cref="ProfileFileInfo" />.
    /// </summary>
    public static void Save()
    {
        if (LoadedProfileFile == null)
        {
            Log.Warn("Cannot save profiles because no profile file is loaded or the profile file is encrypted and not yet unlocked.");

            return;
        }

        // Ensure the profiles directory exists.
        Directory.CreateDirectory(GetProfilesFolderLocation());

        // Create backup before modifying
        CreateDailyBackupIfNeeded();

        // Write profiles to the profile file (JSON, optionally encrypted).
        if (LoadedProfileFile.IsEncrypted)
        {
            // Only if the password provided earlier was valid...
            if (LoadedProfileFile.IsPasswordValid)
            {
                var decryptedBytes = SerializeToByteArray();
                var encryptedBytes = CryptoHelper.Encrypt(decryptedBytes,
                    SecureStringHelper.ConvertToString(LoadedProfileFile.Password),
                    GlobalStaticConfiguration.Profile_EncryptionKeySize,
                    GlobalStaticConfiguration.Profile_EncryptionIterations);

                File.WriteAllBytes(LoadedProfileFile.Path, encryptedBytes);
            }
        }
        else
        {
            SerializeToFile(LoadedProfileFile.Path);
        }

        LoadedProfileFileData?.ProfilesChanged = false;
    }

    /// <summary>
    ///     Method to unload the currently loaded profile file.
    /// </summary>
    /// <param name="saveLoadedProfiles">Save loaded profile file (default is true)</param>
    public static void Unload(bool saveLoadedProfiles = true)
    {
        if (saveLoadedProfiles && LoadedProfileFile != null && LoadedProfileFileData?.ProfilesChanged == true)
            Save();

        LoadedProfileFile = null;
        LoadedProfileFileData = new ProfileFileData();

        // Don't mark as changed since we just unloaded
        ProfilesUpdated(false);
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
    ///     Method to serialize profile data to a JSON file.
    /// </summary>
    /// <param name="filePath">Path to a JSON file.</param>
    private static void SerializeToFile(string filePath)
    {
        // Ensure LoadedProfileFileData exists
        LoadedProfileFileData ??= new ProfileFileData();

        var jsonString = JsonSerializer.Serialize(LoadedProfileFileData, JsonOptions);

        File.WriteAllText(filePath, jsonString);
    }

    /// <summary>
    ///     Method to serialize profile data to a byte array.
    /// </summary>
    /// <returns>Serialized profile data as byte array.</returns>
    private static byte[] SerializeToByteArray()
    {
        // Ensure LoadedProfileFileData exists
        LoadedProfileFileData ??= new ProfileFileData();

        var jsonString = JsonSerializer.Serialize(LoadedProfileFileData, JsonOptions);

        return Encoding.UTF8.GetBytes(jsonString);
    }

    /// <summary>
    ///     Method to deserialize profile data from a JSON file.
    /// </summary>
    /// <param name="filePath">Path to a JSON file.</param>
    private static void DeserializeFromFile(string filePath)
    {
        var jsonString = File.ReadAllText(filePath);

        DeserializeFromJson(jsonString);
    }

    /// <summary>
    ///     Method to deserialize a list of groups as <see cref="GroupInfo" /> from a legacy XML file.
    /// </summary>
    /// <param name="filePath">Path to an XML file.</param>
    [Obsolete("Legacy XML profile files are no longer used, but the method is kept for migration purposes.")]
    private static void DeserializeFromXmlFile(string filePath)
    {
        using FileStream fileStream = new(filePath, FileMode.Open);

        DeserializeFromXmlStream(fileStream);
    }

    /// <summary>
    ///     Method to deserialize profile data from a byte array.
    /// </summary>
    /// <param name="data">Serialized profile data as byte array.</param>
    private static void DeserializeFromByteArray(byte[] data)
    {
        var jsonString = Encoding.UTF8.GetString(data);

        DeserializeFromJson(jsonString);
    }

    /// <summary>
    ///     Method to deserialize a list of groups as <see cref="GroupInfo" /> from a legacy XML byte array.
    /// </summary>
    /// <param name="xml">Serialized list of groups as <see cref="GroupInfo" /> as XML byte array.</param>
    [Obsolete("Legacy XML profile files are no longer used, but the method is kept for migration purposes.")]
    private static void DeserializeFromXmlByteArray(byte[] xml)
    {
        using MemoryStream memoryStream = new(xml);

        DeserializeFromXmlStream(memoryStream);
    }

    /// <summary>
    ///     Method to deserialize profile data from JSON string.
    /// </summary>
    /// <param name="jsonString">JSON string to deserialize.</param>
    private static void DeserializeFromJson(string jsonString)
    {
        try
        {
            var profileFileData = JsonSerializer.Deserialize<ProfileFileData>(jsonString, JsonOptions);

            if (profileFileData != null)
            {
                LoadedProfileFileData = profileFileData;
                return;
            }
        }
        catch (JsonException)
        {
            Log.Info("Failed to deserialize as ProfileFileData, trying legacy format (direct Groups array)...");
        }

        // Fallback: Try to deserialize as legacy format (direct array of GroupInfoSerializable)
        var groupsSerializable = JsonSerializer.Deserialize<List<GroupInfoSerializable>>(jsonString, JsonOptions);

        if (groupsSerializable == null)
            throw new InvalidOperationException("Failed to deserialize JSON profile file.");

        // Create ProfileFileData wrapper for legacy format
        LoadedProfileFileData = new ProfileFileData
        {
            GroupsSerializable = groupsSerializable
        };

        Log.Info("Successfully loaded profile file in legacy format. It will be migrated to new format on next save.");
    }

    /// <summary>
    ///     Method to deserialize a list of groups as <see cref="GroupInfo" /> from an XML stream.
    /// </summary>
    /// <param name="stream">Stream to deserialize.</param>
    [Obsolete("Legacy XML profile files are no longer used, but the method is kept for migration purposes.")]
    private static void DeserializeFromXmlStream(Stream stream)
    {
        XmlSerializer xmlSerializer = new(typeof(List<GroupInfoSerializable>));

        var groupsSerializable = xmlSerializer.Deserialize(stream) as List<GroupInfoSerializable>;

        if (groupsSerializable == null)
            throw new InvalidOperationException("Failed to deserialize XML profile file.");

        LoadedProfileFileData = new ProfileFileData
        {
            GroupsSerializable = groupsSerializable
        };
    }

    /// <summary>
    ///     Method to check if the byte array content is XML.
    /// </summary>
    /// <param name="data">Byte array to check.</param>
    /// <returns>True if the content is XML.</returns>
    [Obsolete("Legacy XML profile files are no longer used, but the method is kept for migration purposes.")]
    private static bool IsXmlContent(byte[] data)
    {
        if (data == null || data.Length == 0)
            return false;

        try
        {
            // Only check the first few bytes for performance
            var bytesToCheck = Math.Min(XmlDetectionBufferSize, data.Length);
            var text = Encoding.UTF8.GetString(data, 0, bytesToCheck).TrimStart();
            // Check for XML declaration or root element that matches profile structure
            return text.StartsWith("<?xml") || text.StartsWith("<ArrayOfGroupInfoSerializable");
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Add, remove, replace group(s) and more.

    /// <summary>
    ///     Method to add a list of <see cref="GroupInfo" /> to the loaded profile data.
    /// </summary>
    /// <param name="groups">List of groups as <see cref="GroupInfo" /> to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when groups collection is null.</exception>
    private static void AddGroups(List<GroupInfo> groups, bool profilesChanged = true)
    {
        ArgumentNullException.ThrowIfNull(groups);

        var skippedCount = 0;
        foreach (var group in groups)
        {
            if (group is null)
            {
                skippedCount++;
                continue;
            }

            LoadedProfileFileData.Groups.Add(group);
        }

        if (skippedCount > 0)
            Log.Warn($"AddGroups skipped {skippedCount} null group(s) in collection.");

        ProfilesUpdated(profilesChanged);
    }

    /// <summary>
    ///     Method to add a <see cref="GroupInfo" /> to the loaded profile data.
    /// </summary>
    /// <param name="group">Group as <see cref="GroupInfo" /> to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when group is null.</exception>
    public static void AddGroup(GroupInfo group, bool profilesChanged = true)
    {
        ArgumentNullException.ThrowIfNull(group);

        LoadedProfileFileData.Groups.Add(group);

        ProfilesUpdated(profilesChanged);
    }

    /// <summary>
    ///     Method to get a group by name.
    /// </summary>
    /// <param name="name">Name of the group.</param>
    /// <returns>Group as <see cref="GroupInfo" />.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when group with specified name is not found.</exception>
    public static GroupInfo GetGroupByName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);


        var group = LoadedProfileFileData.Groups.FirstOrDefault(x => x.Name.Equals(name));

        if (group == null)
            throw new InvalidOperationException($"Group '{name}' not found.");

        return group;
    }

    /// <summary>
    ///     Method to replace a group.
    /// </summary>
    /// <param name="oldGroup">Old group as <see cref="GroupInfo" />.</param>
    /// <param name="newGroup">New group as <see cref="GroupInfo" />.</param>
    /// <exception cref="ArgumentNullException">Thrown when oldGroup or newGroup is null.</exception>
    public static void ReplaceGroup(GroupInfo oldGroup, GroupInfo newGroup)
    {
        ArgumentNullException.ThrowIfNull(oldGroup);
        ArgumentNullException.ThrowIfNull(newGroup);

        LoadedProfileFileData.Groups.Remove(oldGroup);
        LoadedProfileFileData.Groups.Add(newGroup);

        ProfilesUpdated();
    }

    /// <summary>
    ///     Method to remove a group.
    /// </summary>
    /// <param name="group">Group as <see cref="GroupInfo" /> to remove</param>
    /// <exception cref="ArgumentNullException">Thrown when group is null.</exception>
    public static void RemoveGroup(GroupInfo group)
    {
        ArgumentNullException.ThrowIfNull(group);

        LoadedProfileFileData.Groups.Remove(group);

        ProfilesUpdated();
    }

    /// <summary>
    ///     Method to get a list of all group names.
    /// </summary>
    /// <returns>List of group names.</returns>
    public static IReadOnlyCollection<string> GetGroupNames()
    {
        return (from groups in LoadedProfileFileData.Groups where !groups.IsDynamic select groups.Name).ToList();
    }

    /// <summary>
    ///     Method to check if a profile exists.
    /// </summary>
    /// <param name="name">Name of the group.</param>
    /// <returns>True if the profile exists.</returns>
    public static bool GroupExists(string name)
    {
        return LoadedProfileFileData.Groups.Any(group => group.Name == name);
    }

    /// <summary>
    ///     Method checks if a group has profiles.
    /// </summary>
    /// <param name="name">Name of the group</param>
    /// <returns>True if the group has no profiles.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when group with specified name is not found.</exception>
    public static bool IsGroupEmpty(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var group = LoadedProfileFileData.Groups.FirstOrDefault(x => x.Name == name);

        if (group == null)
            throw new InvalidOperationException($"Group '{name}' not found.");

        return group.Profiles.Count == 0;
    }

    #endregion

    #region Add, replace and remove profile(s)

    /// <summary>
    ///     Method to add a profile to a group.
    /// </summary>
    /// <param name="profile">Profile as <see cref="ProfileInfo" /> to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when profile is null.</exception>
    /// <exception cref="ArgumentException">Thrown when profile.Group is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when profile's group is not found after creation attempt.</exception>
    public static void AddProfile(ProfileInfo profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentException.ThrowIfNullOrWhiteSpace(profile.Group, nameof(profile));

        if (!GroupExists(profile.Group))
            AddGroup(new GroupInfo(profile.Group));

        var group = LoadedProfileFileData.Groups.FirstOrDefault(x => x.Name.Equals(profile.Group));

        if (group == null)
            throw new InvalidOperationException($"Group '{profile.Group}' not found for profile after creation attempt.");

        group.Profiles.Add(profile);

        ProfilesUpdated();
    }

    /// <summary>
    ///     Method to replace a profile in a group.
    /// </summary>
    /// <param name="oldProfile">Old profile as <see cref="ProfileInfo" />.</param>
    /// <param name="newProfile">New profile as <see cref="ProfileInfo" />.</param>
    /// <exception cref="ArgumentNullException">Thrown when oldProfile or newProfile is null.</exception>
    /// <exception cref="ArgumentException">Thrown when profile groups are null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when old profile's group is not found.</exception>
    public static void ReplaceProfile(ProfileInfo oldProfile, ProfileInfo newProfile)
    {
        ArgumentNullException.ThrowIfNull(oldProfile);
        ArgumentNullException.ThrowIfNull(newProfile);
        ArgumentException.ThrowIfNullOrWhiteSpace(oldProfile.Group, nameof(oldProfile));
        ArgumentException.ThrowIfNullOrWhiteSpace(newProfile.Group, nameof(newProfile));

        // Remove from old group
        var oldGroup = LoadedProfileFileData.Groups.FirstOrDefault(x => x.Name.Equals(oldProfile.Group));

        if (oldGroup == null)
            throw new InvalidOperationException($"Group '{oldProfile.Group}' not found for old profile.");

        oldGroup.Profiles.Remove(oldProfile);

        // Add to new group (create if doesn't exist)
        if (!GroupExists(newProfile.Group))
            AddGroup(new GroupInfo(newProfile.Group));

        var newGroup = LoadedProfileFileData.Groups.FirstOrDefault(x => x.Name.Equals(newProfile.Group));

        if (newGroup == null)
            throw new InvalidOperationException($"Group '{newProfile.Group}' not found for new profile after creation attempt.");

        newGroup.Profiles.Add(newProfile);

        ProfilesUpdated();
    }

    /// <summary>
    ///     Method to remove a profile from a group.
    /// </summary>
    /// <param name="profile">Profile as <see cref="ProfileInfo" /> to remove.</param>
    /// <exception cref="ArgumentNullException">Thrown when profile is null.</exception>
    /// <exception cref="ArgumentException">Thrown when profile.Group is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when profile's group is not found.</exception>
    public static void RemoveProfile(ProfileInfo profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentException.ThrowIfNullOrWhiteSpace(profile.Group, nameof(profile));

        var group = LoadedProfileFileData.Groups.FirstOrDefault(x => x.Name.Equals(profile.Group));

        if (group == null)
            throw new InvalidOperationException($"Group '{profile.Group}' not found.");

        group.Profiles.Remove(profile);

        ProfilesUpdated();
    }

    /// <summary>
    ///     Method to remove a list of profiles from a group.
    /// </summary>
    /// <param name="profiles">List of profiles as <see cref="ProfileInfo" /> to remove.</param>
    /// <exception cref="ArgumentNullException">Thrown when profiles collection is null.</exception>
    public static void RemoveProfiles(IEnumerable<ProfileInfo> profiles)
    {
        ArgumentNullException.ThrowIfNull(profiles);

        var skippedCount = 0;
        foreach (var profile in profiles)
        {
            if (profile is null || string.IsNullOrWhiteSpace(profile.Group))
            {
                skippedCount++;
                continue;
            }

            var group = LoadedProfileFileData.Groups.FirstOrDefault(x => x.Name.Equals(profile.Group));

            if (group == null)
            {
                Log.Warn($"RemoveProfiles: Group '{profile.Group}' not found for profile '{profile.Name ?? "<unnamed>"}'.");
                skippedCount++;
                continue;
            }

            group.Profiles.Remove(profile);
        }

        if (skippedCount > 0)
            Log.Warn($"RemoveProfiles skipped {skippedCount} null or invalid profile(s) in collection.");

        ProfilesUpdated();
    }

    #endregion

    #region Backup

    /// <summary>
    ///     Creates a backup of the currently loaded profile file if a backup has not already been created for the current day.
    /// </summary>
    private static void CreateDailyBackupIfNeeded()
    {
        // Skip if daily backups are disabled
        if (!SettingsManager.Current.Profiles_IsDailyBackupEnabled)
        {
            Log.Info("Daily profile backups are disabled. Skipping backup creation...");
            return;
        }

        // Skip if no profile is loaded
        if (LoadedProfileFile == null || LoadedProfileFileData == null)
        {
            Log.Info("No profile file is currently loaded. Skipping backup creation...");
            return;
        }

        // Skip if the profile file doesn't exist yet
        if (!File.Exists(LoadedProfileFile.Path))
        {
            Log.Warn($"Profile file does not exist yet: {LoadedProfileFile.Path}. Skipping backup creation...");
            return;
        }

        // Create backup if needed        
        var currentDate = DateTime.Now.Date;
        var lastBackupDate = LoadedProfileFileData.LastBackup?.Date ?? DateTime.MinValue;
        var profileFileName = Path.GetFileName(LoadedProfileFile.Path);

        if (lastBackupDate < currentDate)
        {
            Log.Info($"Creating daily backup for profile: {profileFileName}");

            // Create backup
            Backup(LoadedProfileFile.Path,
                GetProfilesBackupFolderLocation(),
                TimestampHelper.GetTimestampFilename(profileFileName));

            // Cleanup old backups
            CleanupBackups(GetProfilesBackupFolderLocation(),
                profileFileName,
                SettingsManager.Current.Profiles_MaximumNumberOfBackups);

            LoadedProfileFileData.LastBackup = currentDate;
        }
    }

    /// <summary>
    ///     Deletes older backup files in the specified folder to ensure that only the most recent backups, up to the
    ///     specified maximum, are retained.
    /// </summary>
    /// <param name="backupFolderPath">The full path to the directory containing the backup files to be managed.</param>
    /// <param name="profileFileName">The profile file name pattern used to identify backup files for cleanup.</param>
    /// <param name="maxBackupFiles">The maximum number of backup files to retain. Must be greater than zero.</param>
    private static void CleanupBackups(string backupFolderPath, string profileFileName, int maxBackupFiles)
    {
        // Skip if backup directory doesn't exist
        if (!Directory.Exists(backupFolderPath))
        {
            Log.Error($"Backup directory does not exist: {backupFolderPath}. Cannot cleanup old backups.");
            return;
        }

        // Extract profile name without extension to match all backup files regardless of extension
        // (e.g., "Default" matches "2025-01-19_Default.json", "2025-01-19_Default.encrypted", etc.)
        var profileNameWithoutExtension = Path.GetFileNameWithoutExtension(profileFileName);

        // Get all backup files for this specific profile (any extension) sorted by timestamp (newest first)
        var backupFiles = Directory.GetFiles(backupFolderPath)
            .Where(f =>
            {
                var fileName = Path.GetFileName(f);

                // Check if it's a timestamped backup and contains the profile name
                return TimestampHelper.IsTimestampedFilename(fileName) &&
                       fileName.Contains($"_{profileNameWithoutExtension}.");
            })
            .OrderByDescending(f => TimestampHelper.ExtractTimestampFromFilename(Path.GetFileName(f)))
            .ToList();

        if (backupFiles.Count > maxBackupFiles)
            Log.Info($"Cleaning up old backup files for {profileNameWithoutExtension}... Found {backupFiles.Count} backups, keeping the most recent {maxBackupFiles}.");

        // Delete oldest backups until the maximum number is reached
        while (backupFiles.Count > maxBackupFiles)
        {
            var fileToDelete = backupFiles.Last();

            File.Delete(fileToDelete);

            backupFiles.RemoveAt(backupFiles.Count - 1);

            Log.Info($"Backup deleted: {fileToDelete}");
        }
    }

    /// <summary>
    /// Creates a backup of the specified profile file in the given backup folder with the provided backup file name.
    /// </summary>
    /// <param name="filePath">The full path to the profile file to back up. Cannot be null or empty.</param>
    /// <param name="backupFolderPath">The directory path where the backup file will be stored. If the directory does not exist, it will be created.</param>
    /// <param name="backupFileName">The name to use for the backup file within the backup folder. Cannot be null or empty.</param>
    private static void Backup(string filePath, string backupFolderPath, string backupFileName)
    {
        // Create the backup directory if it does not exist
        Directory.CreateDirectory(backupFolderPath);

        // Create the backup file path
        var backupFilePath = Path.Combine(backupFolderPath, backupFileName);

        // Copy the current profile file to the backup location
        File.Copy(filePath, backupFilePath, true);

        Log.Info($"Backup created: {backupFilePath}");
    }

    #endregion
}
