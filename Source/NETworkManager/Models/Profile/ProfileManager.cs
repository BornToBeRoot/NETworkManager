using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Enum;
using NETworkManager.Models.Settings;
using NETworkManager.ViewModels;
using NETworkManager.Views;

namespace NETworkManager.Models.Profile
{
    public static class ProfileManager
    {
        #region Variables
        private static string ProfilesFolderName => "Profiles";
        private static string ProfilesDefaultFileName => "Default";
        public static string ProfilesFileExtension => ".xml";

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

                if (value != null)
                    LoadedProfileFileChanged(value);

                _profileFileInfo = value;
            }
        }
        public static ObservableCollection<ProfileInfo> Profiles { get; set; } = new ObservableCollection<ProfileInfo>();
        public static bool ProfilesChanged { get; set; }

        public static event EventHandler<ProfileFileInfoArgs> LoadedProfileFilesChangedEvent;

        private static void LoadedProfileFileChanged(ProfileFileInfo profileFileInfo)
        {
            LoadedProfileFilesChangedEvent?.Invoke(null, new ProfileFileInfoArgs(profileFileInfo));
        }
        #endregion

        static ProfileManager()
        {
            // Load files
            LoadFiles();

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
            Debug.WriteLine(path);
            return path;
        }
        #endregion

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

        private static IEnumerable<string> GetProfileFiles(string location)
        {
            return Directory.GetFiles(location).Where(x => Path.GetExtension(x) == ProfilesFileExtension);
        }

        #region Load files, refresh files
        private static void LoadFiles()
        {
            var location = GetProfilesLocation();

            if (!Directory.Exists(location))
                return;

            ProfileFiles.Clear();

            foreach (var file in GetProfileFiles(location))
                ProfileFiles.Add(new ProfileFileInfo(Path.GetFileNameWithoutExtension(file), file));

            // Create default
            if (ProfileFiles.Count == 0)
                ProfileFiles.Add(new ProfileFileInfo(ProfilesDefaultFileName, GetProfilesDefaultFilePath()));
        }

        public static void RefreshFiles()
        {
            LoadFiles();

            if (LoadedProfileFile != null)
            {
                var name = LoadedProfileFile.Name;

                var profileFile = ProfileFiles.FirstOrDefault(x => x.Name == name);

                Load(profileFile != null ? profileFile : ProfileFiles[0]);
            }
        }
        #endregion

        public static void SwitchProfile(ProfileFileInfo info)
        {
            // Save
            if (LoadedProfileFile != null && ProfilesChanged)
                Save();

            // Load             
            Profiles.Clear();

            Load(info);
        }

        private static void Load(ProfileFileInfo info)
        {
            if (File.Exists(info.Path)) // Load if exists...
                DeserializeFromFile(info.Path);

            ProfilesChanged = false;

            LoadedProfileFile = info;
        }

        private static void DeserializeFromFile(string filePath)
        {
            var xmlSerializer = new XmlSerializer(typeof(List<ProfileInfo>));

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                ((List<ProfileInfo>)(xmlSerializer.Deserialize(fileStream))).ForEach(AddProfile);
            }
        }

        public static void Save()
        {
            var location = GetProfilesLocation();

            // Create the directory if it does not exist
            if (!Directory.Exists(location))
                Directory.CreateDirectory(location);

            SerializeToFile(LoadedProfileFile.Path);

            ProfilesChanged = false;
        }

        private static void SerializeToFile(string filePath)
        {
            var xmlSerializer = new XmlSerializer(typeof(List<ProfileInfo>));

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, new List<ProfileInfo>(Profiles));
            }
        }

        public static void Reset()
        {
            Profiles.Clear();
        }


        #region Move settings
        public static Task MoveProfilesAsync(string targedLocation)
        {
            return Task.Run(() => MoveProfiles(targedLocation));
        }

        private static void MoveProfiles(string targedLocation)
        {
            // Create the dircetory and copy the files to the new location
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
        }
        #endregion

        #region Methods --> Add profile, Remove profile, Rename group
        public static void AddProfile(ProfileInfo profile)
        {
            // Possible fix for appcrash --> when icollection view is refreshed...
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                lock (Profiles)
                    Profiles.Add(profile);
            }));
        }

        public static void AddProfile(ProfileViewModel instance)
        {
            AddProfile(new ProfileInfo
            {
                Name = instance.Name?.Trim(),
                Host = instance.Host?.Trim(),
                Group = instance.Group?.Trim(),
                Tags = instance.Tags?.Trim(),

                NetworkInterface_Enabled = instance.NetworkInterface_Enabled,
                NetworkInterface_EnableStaticIPAddress = instance.NetworkInterface_EnableStaticIPAddress,
                NetworkInterface_IPAddress = instance.NetworkInterface_IPAddress?.Trim(),
                NetworkInterface_Gateway = instance.NetworkInterface_Gateway?.Trim(),
                NetworkInterface_SubnetmaskOrCidr = instance.NetworkInterface_SubnetmaskOrCidr?.Trim(),
                NetworkInterface_EnableStaticDNS = instance.NetworkInterface_EnableStaticDNS,
                NetworkInterface_PrimaryDNSServer = instance.NetworkInterface_PrimaryDNSServer?.Trim(),
                NetworkInterface_SecondaryDNSServer = instance.NetworkInterface_SecondaryDNSServer?.Trim(),

                IPScanner_Enabled = instance.IPScanner_Enabled,
                IPScanner_InheritHost = instance.IPScanner_InheritHost,
                IPScanner_HostOrIPRange = instance.IPScanner_InheritHost ? instance.Host?.Trim() : instance.IPScanner_HostOrIPRange?.Trim(),

                PortScanner_Enabled = instance.PortScanner_Enabled,
                PortScanner_InheritHost = instance.PortScanner_InheritHost,
                PortScanner_Host = instance.PortScanner_InheritHost ? instance.Host?.Trim() : instance.PortScanner_Host?.Trim(),
                PortScanner_Ports = instance.PortScanner_Ports?.Trim(),

                Ping_Enabled = instance.Ping_Enabled,
                Ping_InheritHost = instance.Ping_InheritHost,
                Ping_Host = instance.Ping_InheritHost ? instance.Host?.Trim() : instance.Ping_Host?.Trim(),

                PingMonitor_Enabled = instance.PingMonitor_Enabled,
                PingMonitor_InheritHost = instance.PingMonitor_InheritHost,
                PingMonitor_Host = instance.PingMonitor_InheritHost ? instance.Host?.Trim() : instance.PingMonitor_Host?.Trim(),

                Traceroute_Enabled = instance.Traceroute_Enabled,
                Traceroute_InheritHost = instance.Traceroute_InheritHost,
                Traceroute_Host = instance.Traceroute_InheritHost ? instance.Host?.Trim() : instance.Traceroute_Host?.Trim(),

                DNSLookup_Enabled = instance.DNSLookup_Enabled,
                DNSLookup_InheritHost = instance.Traceroute_InheritHost,
                DNSLookup_Host = instance.DNSLookup_InheritHost ? instance.Host?.Trim() : instance.DNSLookup_Host?.Trim(),

                RemoteDesktop_Enabled = instance.RemoteDesktop_Enabled,
                RemoteDesktop_InheritHost = instance.RemoteDesktop_InheritHost,
                RemoteDesktop_Host = instance.RemoteDesktop_InheritHost ? instance.Host?.Trim() : instance.RemoteDesktop_Host?.Trim(),
                RemoteDesktop_OverrideDisplay = instance.RemoteDesktop_OverrideDisplay,
                RemoteDesktop_AdjustScreenAutomatically = instance.RemoteDesktop_AdjustScreenAutomatically,
                RemoteDesktop_UseCurrentViewSize = instance.RemoteDesktop_UseCurrentViewSize,
                RemoteDesktop_UseFixedScreenSize = instance.RemoteDesktop_UseFixedScreenSize,
                RemoteDesktop_ScreenWidth = instance.RemoteDesktop_ScreenWidth,
                RemoteDesktop_ScreenHeight = instance.RemoteDesktop_ScreenHeight,
                RemoteDesktop_UseCustomScreenSize = instance.RemoteDesktop_UseCustomScreenSize,
                RemoteDesktop_CustomScreenWidth = int.Parse(instance.RemoteDesktop_CustomScreenWidth),
                RemoteDesktop_CustomScreenHeight = int.Parse(instance.RemoteDesktop_CustomScreenHeight),
                RemoteDesktop_OverrideColorDepth = instance.RemoteDesktop_OverrideColorDepth,
                RemoteDesktop_ColorDepth = instance.RemoteDesktop_SelectedColorDepth,
                RemoteDesktop_OverridePort = instance.RemoteDesktop_OverridePort,
                RemoteDesktop_Port = instance.RemoteDesktop_Port,
                RemoteDesktop_OverrideCredSspSupport = instance.RemoteDesktop_OverrideCredSspSupport,
                RemoteDesktop_EnableCredSspSupport = instance.RemoteDesktop_EnableCredSspSupport,
                RemoteDesktop_OverrideAuthenticationLevel = instance.RemoteDesktop_OverrideAuthenticationLevel,
                RemoteDesktop_AuthenticationLevel = instance.RemoteDesktop_AuthenticationLevel,
                RemoteDesktop_OverrideAudioRedirectionMode = instance.RemoteDesktop_OverrideAudioRedirectionMode,
                RemoteDesktop_AudioRedirectionMode = instance.RemoteDesktop_AudioRedirectionMode,
                RemoteDesktop_OverrideAudioCaptureRedirectionMode = instance.RemoteDesktop_OverrideAudioCaptureRedirectionMode,
                RemoteDesktop_AudioCaptureRedirectionMode = instance.RemoteDesktop_AudioCaptureRedirectionMode,
                RemoteDesktop_OverrideApplyWindowsKeyCombinations = instance.RemoteDesktop_OverrideApplyWindowsKeyCombinations,
                RemoteDesktop_KeyboardHookMode = instance.RemoteDesktop_KeyboardHookMode,
                RemoteDesktop_OverrideRedirectClipboard = instance.RemoteDesktop_OverrideRedirectClipboard,
                RemoteDesktop_RedirectClipboard = instance.RemoteDesktop_RedirectClipboard,
                RemoteDesktop_OverrideRedirectDevices = instance.RemoteDesktop_OverrideRedirectDevices,
                RemoteDesktop_RedirectDevices = instance.RemoteDesktop_RedirectDevices,
                RemoteDesktop_OverrideRedirectDrives = instance.RemoteDesktop_OverrideRedirectDrives,
                RemoteDesktop_RedirectDrives = instance.RemoteDesktop_RedirectDrives,
                RemoteDesktop_OverrideRedirectPorts = instance.RemoteDesktop_OverrideRedirectPorts,
                RemoteDesktop_RedirectPorts = instance.RemoteDesktop_RedirectPorts,
                RemoteDesktop_OverrideRedirectSmartcards = instance.RemoteDesktop_OverrideRedirectSmartcards,
                RemoteDesktop_RedirectSmartCards = instance.RemoteDesktop_RedirectSmartCards,
                RemoteDesktop_OverrideRedirectPrinters = instance.RemoteDesktop_OverrideRedirectPrinters,
                RemoteDesktop_RedirectPrinters = instance.RemoteDesktop_RedirectPrinters,
                RemoteDesktop_OverridePersistentBitmapCaching = instance.RemoteDesktop_OverridePersistentBitmapCaching,
                RemoteDesktop_PersistentBitmapCaching = instance.RemoteDesktop_PersistentBitmapCaching,
                RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped = instance.RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped,
                RemoteDesktop_ReconnectIfTheConnectionIsDropped = instance.RemoteDesktop_ReconnectIfTheConnectionIsDropped,
                RemoteDesktop_OverrideNetworkConnectionType = instance.RemoteDesktop_OverrideNetworkConnectionType,
                RemoteDesktop_NetworkConnectionType = instance.RemoteDesktop_NetworkConnectionType,
                RemoteDesktop_DesktopBackground = instance.RemoteDesktop_DesktopBackground,
                RemoteDesktop_FontSmoothing = instance.RemoteDesktop_FontSmoothing,
                RemoteDesktop_DesktopComposition = instance.RemoteDesktop_DesktopComposition,
                RemoteDesktop_ShowWindowContentsWhileDragging = instance.RemoteDesktop_ShowWindowContentsWhileDragging,
                RemoteDesktop_MenuAndWindowAnimation = instance.RemoteDesktop_MenuAndWindowAnimation,
                RemoteDesktop_VisualStyles = instance.RemoteDesktop_VisualStyles,

                PowerShell_Enabled = instance.PowerShell_Enabled,
                PowerShell_EnableRemoteConsole = instance.PowerShell_EnableRemoteConsole,
                PowerShell_InheritHost = instance.PowerShell_InheritHost,
                PowerShell_Host = instance.PowerShell_InheritHost ? instance.Host?.Trim() : instance.PowerShell_Host?.Trim(),
                PowerShell_OverrideAdditionalCommandLine = instance.PowerShell_OverrideAdditionalCommandLine,
                PowerShell_AdditionalCommandLine = instance.PowerShell_AdditionalCommandLine,
                PowerShell_OverrideExecutionPolicy = instance.PowerShell_OverrideExecutionPolicy,
                PowerShell_ExecutionPolicy = instance.PowerShell_ExecutionPolicy,

                PuTTY_Enabled = instance.PuTTY_Enabled,
                PuTTY_ConnectionMode = instance.PuTTY_ConnectionMode,
                PuTTY_InheritHost = instance.PuTTY_InheritHost,
                PuTTY_HostOrSerialLine = instance.PuTTY_ConnectionMode == PuTTY.PuTTY.ConnectionMode.Serial ? instance.PuTTY_HostOrSerialLine?.Trim() : (instance.PuTTY_InheritHost ? instance.Host?.Trim() : instance.PuTTY_HostOrSerialLine?.Trim()),
                PuTTY_OverridePortOrBaud = instance.PuTTY_OverridePortOrBaud,
                PuTTY_PortOrBaud = instance.PuTTY_PortOrBaud,
                PuTTY_OverrideUsername = instance.PuTTY_OverrideUsername,
                PuTTY_Username = instance.PuTTY_Username?.Trim(),
                PuTTY_OverrideProfile = instance.PuTTY_OverrideProfile,
                PuTTY_Profile = instance.PuTTY_Profile?.Trim(),
                PuTTY_OverrideAdditionalCommandLine = instance.PuTTY_OverrideAdditionalCommandLine,
                PuTTY_AdditionalCommandLine = instance.PuTTY_AdditionalCommandLine?.Trim(),

                TigerVNC_Enabled = instance.TigerVNC_Enabled,
                TigerVNC_InheritHost = instance.TigerVNC_InheritHost,
                TigerVNC_Host = instance.TigerVNC_InheritHost ? instance.Host?.Trim() : instance.TigerVNC_Host?.Trim(),
                TigerVNC_OverridePort = instance.TigerVNC_OverridePort,
                TigerVNC_Port = instance.TigerVNC_Port,

                WakeOnLAN_Enabled = instance.WakeOnLAN_Enabled,
                WakeOnLAN_MACAddress = instance.WakeOnLAN_MACAddress?.Trim(),
                WakeOnLAN_Broadcast = instance.WakeOnLAN_Broadcast?.Trim(),
                WakeOnLAN_OverridePort = instance.WakeOnLAN_OverridePort,
                WakeOnLAN_Port = instance.WakeOnLAN_Port,

                HTTPHeaders_Enabled = instance.HTTPHeaders_Enabled,
                HTTPHeaders_Website = instance.HTTPHeaders_Website,

                Whois_Enabled = instance.Whois_Enabled,
                Whois_InheritHost = instance.Whois_InheritHost,
                Whois_Domain = instance.Whois_InheritHost ? instance.Host?.Trim() : instance.Whois_Domain?.Trim()
            });
        }

        public static void RemoveProfile(ProfileInfo profile)
        {
            // Possible fix for appcrash --> when icollection view is refreshed...
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
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

        #region Dialogs --> Add profile, Edit profile, CopyAs profile, Delete profile, Edit group
        public static async void ShowAddProfileDialog(IProfileManager viewModel, IDialogCoordinator dialogCoordinator)
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.AddProfile
            };

            var profileViewModel = new ProfileViewModel(async instance =>
            {
                await dialogCoordinator.HideMetroDialogAsync(viewModel, customDialog);
                viewModel.OnProfileDialogClose();

                AddProfile(instance);
            }, async instance =>
            {
                await dialogCoordinator.HideMetroDialogAsync(viewModel, customDialog);
                viewModel.OnProfileDialogClose();
            }, GetGroups());

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            viewModel.OnProfileDialogOpen();
            await dialogCoordinator.ShowMetroDialogAsync(viewModel, customDialog);
        }

        public static async void ShowEditProfileDialog(IProfileManager viewModel, IDialogCoordinator dialogCoordinator, ProfileInfo selectedProfile)
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.EditProfile
            };

            var profileViewModel = new ProfileViewModel(async instance =>
            {
                await dialogCoordinator.HideMetroDialogAsync(viewModel, customDialog);
                viewModel.OnProfileDialogClose();

                RemoveProfile(selectedProfile);

                AddProfile(instance);
            }, async instance =>
            {
                await dialogCoordinator.HideMetroDialogAsync(viewModel, customDialog);
                viewModel.OnProfileDialogClose();
            }, GetGroups(), ProfileEditMode.Edit, selectedProfile);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            viewModel.OnProfileDialogOpen();
            await dialogCoordinator.ShowMetroDialogAsync(viewModel, customDialog);
        }

        public static async void ShowCopyAsProfileDialog(IProfileManager viewModel, IDialogCoordinator dialogCoordinator, ProfileInfo selectedProfile)
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.CopyProfile
            };

            var profileViewModel = new ProfileViewModel(async instance =>
            {
                await dialogCoordinator.HideMetroDialogAsync(viewModel, customDialog);
                viewModel.OnProfileDialogClose();

                AddProfile(instance);
            }, async instance =>
            {
                await dialogCoordinator.HideMetroDialogAsync(viewModel, customDialog);
                viewModel.OnProfileDialogClose();
            }, GetGroups(), ProfileEditMode.Copy, selectedProfile);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            viewModel.OnProfileDialogOpen();
            await dialogCoordinator.ShowMetroDialogAsync(viewModel, customDialog);
        }

        public static async void ShowDeleteProfileDialog(IProfileManager viewModel, IDialogCoordinator dialogCoordinator, ProfileInfo selectedProfile)
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.DeleteProfile
            };

            var confirmRemoveViewModel = new ConfirmRemoveViewModel(async instance =>
            {
                await dialogCoordinator.HideMetroDialogAsync(viewModel, customDialog);
                viewModel.OnProfileDialogClose();

                RemoveProfile(selectedProfile);
            }, async instance =>
            {
                await dialogCoordinator.HideMetroDialogAsync(viewModel, customDialog);
                viewModel.OnProfileDialogClose();
            }, Resources.Localization.Strings.DeleteProfileMessage);

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            viewModel.OnProfileDialogOpen();
            await dialogCoordinator.ShowMetroDialogAsync(viewModel, customDialog);
        }

        public static async void ShowEditGroupDialog(IProfileManager viewModel, IDialogCoordinator dialogCoordinator, string group)
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.EditGroup
            };

            var editGroupViewModel = new GroupViewModel(async instance =>
            {
                await dialogCoordinator.HideMetroDialogAsync(viewModel, customDialog);
                viewModel.OnProfileDialogClose();

                RenameGroup(instance.OldGroup, instance.Group);

                viewModel.RefreshProfiles();
            }, async instance =>
            {
                await dialogCoordinator.HideMetroDialogAsync(viewModel, customDialog);
                viewModel.OnProfileDialogClose();
            }, group, GetGroups());

            customDialog.Content = new GroupDialog
            {
                DataContext = editGroupViewModel
            };

            viewModel.OnProfileDialogOpen();
            await dialogCoordinator.ShowMetroDialogAsync(viewModel, customDialog);
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
