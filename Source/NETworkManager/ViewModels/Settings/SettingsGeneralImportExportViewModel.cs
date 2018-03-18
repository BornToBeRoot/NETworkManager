using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Helpers;
using NETworkManager.Models.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsGeneralImportExportViewModel : ViewModelBase
    {
        private IDialogCoordinator dialogCoordinator;

        public Action CloseAction { get; set; }

        private const string ImportExportFileExtensionFilter = "ZIP Archive (*.zip)|*.zip";

        #region Variables
        #region Import
        private string _importLocationSelectedPath;
        public string ImportLocationSelectedPath
        {
            get { return _importLocationSelectedPath; }
            set
            {
                if (value == _importLocationSelectedPath)
                    return;

                ImportFileIsValid = false;

                _importLocationSelectedPath = value;
                OnPropertyChanged();
            }
        }

        private bool _importFileIsValid;
        public bool ImportFileIsValid
        {
            get { return _importFileIsValid; }
            set
            {
                if (value == _importFileIsValid)
                    return;

                _importFileIsValid = value;
                OnPropertyChanged();
            }
        }

        public bool _importEverything = true;
        public bool ImportEverything
        {
            get { return _importEverything; }
            set
            {
                if (value == _importEverything)
                    return;

                _importEverything = value;
                OnPropertyChanged();
            }
        }

        private bool _importApplicationSettingsExists;
        public bool ImportApplicationSettingsExists
        {
            get { return _importApplicationSettingsExists; }
            set
            {
                if (value == _importApplicationSettingsExists)
                    return;

                _importApplicationSettingsExists = value;
                OnPropertyChanged();
            }
        }

        private bool _importApplicationSettings;
        public bool ImportApplicationSettings
        {
            get { return _importApplicationSettings; }
            set
            {
                if (value == _importApplicationSettings)
                    return;

                _importApplicationSettings = value;
                OnPropertyChanged();
            }
        }

        private bool _importNetworkInterfaceProfilesExists;
        public bool ImportNetworkInterfaceProfilesExists
        {
            get { return _importNetworkInterfaceProfilesExists; }
            set
            {
                if (value == _importNetworkInterfaceProfilesExists)
                    return;

                _importNetworkInterfaceProfilesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _importNetworkInterfaceProfiles;
        public bool ImportNetworkInterfaceProfiles
        {
            get { return _importNetworkInterfaceProfiles; }
            set
            {
                if (value == _importNetworkInterfaceProfiles)
                    return;

                _importNetworkInterfaceProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _importOverrideNetworkInterfaceProfiles = true;
        public bool ImportOverrideNetworkInterfaceProfiles
        {
            get { return _importOverrideNetworkInterfaceProfiles; }
            set
            {
                if (value == _importOverrideNetworkInterfaceProfiles)
                    return;

                _importOverrideNetworkInterfaceProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _importIPScannerProfilesExists;
        public bool ImportIPScannerProfilesExists
        {
            get { return _importIPScannerProfilesExists; }
            set
            {
                if (value == _importIPScannerProfilesExists)
                    return;

                _importIPScannerProfilesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _importIPScannerProfiles;
        public bool ImportIPScannerProfiles
        {
            get { return _importIPScannerProfiles; }
            set
            {
                if (value == _importIPScannerProfiles)
                    return;

                _importIPScannerProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _importOverrideIPScannerProfiles = true;
        public bool ImportOverrideIPScannerProfiles
        {
            get { return _importOverrideIPScannerProfiles; }
            set
            {
                if (value == _importOverrideIPScannerProfiles)
                    return;

                _importOverrideIPScannerProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _importWakeOnLANClientsExists;
        public bool ImportWakeOnLANClientsExists
        {
            get { return _importWakeOnLANClientsExists; }
            set
            {
                if (value == _importWakeOnLANClientsExists)
                    return;

                _importWakeOnLANClientsExists = value;
                OnPropertyChanged();
            }
        }

        private bool _importWakeOnLANClients;
        public bool ImportWakeOnLANClients
        {
            get { return _importWakeOnLANClients; }
            set
            {
                if (value == _importWakeOnLANClients)
                    return;

                _importWakeOnLANClients = value;
                OnPropertyChanged();
            }
        }

        private bool _importOverrideWakeOnLANClients = true;
        public bool ImportOverrideWakeOnLANClients
        {
            get { return _importOverrideWakeOnLANClients; }
            set
            {
                if (value == _importOverrideWakeOnLANClients)
                    return;

                _importOverrideWakeOnLANClients = value;
                OnPropertyChanged();
            }
        }

        private bool _importPortScannerProfilesExists;
        public bool ImportPortScannerProfilesExists
        {
            get { return _importPortScannerProfilesExists; }
            set
            {
                if (value == _importPortScannerProfilesExists)
                    return;

                _importPortScannerProfilesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _importPortScannerProfiles;
        public bool ImportPortScannerProfiles
        {
            get { return _importPortScannerProfiles; }
            set
            {
                if (value == _importPortScannerProfiles)
                    return;

                _importPortScannerProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _importOverridePortScannerProfiles = true;
        public bool ImportOverridePortScannerProfiles
        {
            get { return _importOverridePortScannerProfiles; }
            set
            {
                if (value == _importOverridePortScannerProfiles)
                    return;

                _importOverridePortScannerProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _importRemoteDesktopSessionsExists;
        public bool ImportRemoteDesktopSessionsExists
        {
            get { return _importRemoteDesktopSessionsExists; }
            set
            {
                if (value == _importRemoteDesktopSessionsExists)
                    return;

                _importRemoteDesktopSessionsExists = value;
                OnPropertyChanged();
            }
        }

        private bool _importRemoteDesktopSessions;
        public bool ImportRemoteDesktopSessions
        {
            get { return _importRemoteDesktopSessions; }
            set
            {
                if (value == _importRemoteDesktopSessions)
                    return;

                _importRemoteDesktopSessions = value;
                OnPropertyChanged();
            }
        }

        private bool _importOverrideRemoteDesktopSessions = true;
        public bool ImportOverrideRemoteDesktopSessions
        {
            get { return _importOverrideRemoteDesktopSessions; }
            set
            {
                if (value == _importOverrideRemoteDesktopSessions)
                    return;

                _importOverrideRemoteDesktopSessions = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Export
        private bool _exportEverything;
        public bool ExportEverything
        {
            get { return _exportEverything; }
            set
            {
                if (value == _exportEverything)
                    return;

                _exportEverything = value;
                OnPropertyChanged();
            }
        }

        private bool _applicationSettingsExists;
        public bool ApplicationSettingsExists
        {
            get { return _applicationSettingsExists; }
            set
            {
                if (value == _applicationSettingsExists)
                    return;

                _applicationSettingsExists = value;
                OnPropertyChanged();
            }
        }

        private bool _exportApplicationSettings;
        public bool ExportApplicationSettings
        {
            get { return _exportApplicationSettings; }
            set
            {
                if (value == _exportApplicationSettings)
                    return;

                _exportApplicationSettings = value;
                OnPropertyChanged();
            }
        }

        private bool _networkInterfaceProfilesExists;
        public bool NetworkInterfaceProfilesExists
        {
            get { return _networkInterfaceProfilesExists; }
            set
            {
                if (value == _networkInterfaceProfilesExists)
                    return;

                _networkInterfaceProfilesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _exportNetworkInterfaceProfiles;
        public bool ExportNetworkInterfaceProfiles
        {
            get { return _exportNetworkInterfaceProfiles; }
            set
            {
                if (value == _exportNetworkInterfaceProfiles)
                    return;

                _exportNetworkInterfaceProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _ipScannerProfilesExists;
        public bool IPScannerProfilesExists
        {
            get { return _ipScannerProfilesExists; }
            set
            {
                if (value == _ipScannerProfilesExists)
                    return;

                _ipScannerProfilesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _exportIPScannerProfiles;
        public bool ExportIPScannerProfiles
        {
            get { return _exportIPScannerProfiles; }
            set
            {
                if (value == _exportIPScannerProfiles)
                    return;

                _exportIPScannerProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _wakeOnLANClientsExists;
        public bool WakeOnLANClientsExists
        {
            get { return _wakeOnLANClientsExists; }
            set
            {
                if (value == _wakeOnLANClientsExists)
                    return;

                _wakeOnLANClientsExists = value;
                OnPropertyChanged();
            }
        }

        private bool _exportWakeOnLANClients;
        public bool ExportWakeOnLANClients
        {
            get { return _exportWakeOnLANClients; }
            set
            {
                if (value == _exportWakeOnLANClients)
                    return;

                _exportWakeOnLANClients = value;
                OnPropertyChanged();
            }
        }

        private bool _portScannerProfilesExists;
        public bool PortScannerProfilesExists
        {
            get { return _portScannerProfilesExists; }
            set
            {
                if (value == _portScannerProfilesExists)
                    return;

                _portScannerProfilesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _exportPortScannerProfiles;
        public bool ExportPortScannerProfiles
        {
            get { return _exportPortScannerProfiles; }
            set
            {
                if (value == _exportPortScannerProfiles)
                    return;

                _exportPortScannerProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktopSesionsExists;
        public bool RemoteDesktopSessionsExists
        {
            get { return _remoteDesktopSesionsExists; }
            set
            {
                if (value == _remoteDesktopSesionsExists)
                    return;

                _remoteDesktopSesionsExists = value;
                OnPropertyChanged();
            }
        }

        private bool _exportRemoteDesktopSessions;
        public bool ExportRemoteDesktopSessions
        {
            get { return _exportRemoteDesktopSessions; }
            set
            {
                if (value == _exportRemoteDesktopSessions)
                    return;

                _exportRemoteDesktopSessions = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor
        public SettingsGeneralImportExportViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;
        }
        #endregion

        #region ICommands & Actions
        public ICommand BrowseFileCommand
        {
            get { return new RelayCommand(p => BrowseFileAction()); }
        }

        private void BrowseFileAction()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = ImportExportFileExtensionFilter
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                ImportLocationSelectedPath = openFileDialog.FileName;
        }

        public ICommand ValidateImportSettingsCommand
        {
            get { return new RelayCommand(p => ValidateImportSettingsAction()); }
        }

        private async void ValidateImportSettingsAction()
        {
            try
            {
                List<ImportExportManager.ImportExportOptions> importOptions = ImportExportManager.ValidateImportFile(ImportLocationSelectedPath);

                ImportFileIsValid = true;
                ImportApplicationSettingsExists = importOptions.Contains(ImportExportManager.ImportExportOptions.ApplicationSettings);
                ImportNetworkInterfaceProfilesExists = importOptions.Contains(ImportExportManager.ImportExportOptions.NetworkInterfaceProfiles);
                ImportIPScannerProfilesExists = importOptions.Contains(ImportExportManager.ImportExportOptions.IPScannerProfiles);
                ImportWakeOnLANClientsExists = importOptions.Contains(ImportExportManager.ImportExportOptions.WakeOnLANClients);
                ImportPortScannerProfilesExists = importOptions.Contains(ImportExportManager.ImportExportOptions.PortScannerProfiles);
                ImportRemoteDesktopSessionsExists = importOptions.Contains(ImportExportManager.ImportExportOptions.RemoteDesktopSessions);
            }
            catch (ImportFileNotValidException)
            {
                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_ValidationFailed"] as string, Application.Current.Resources["String_NoValidFileFoundToImport"] as string, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
            }
        }

        public ICommand ImportSettingsCommand
        {
            get { return new RelayCommand(p => ImportSettingsAction()); }
        }

        private async void ImportSettingsAction()
        {
            MetroDialogSettings settings = AppearanceManager.MetroDialog;

            settings.AffirmativeButtonText = Application.Current.Resources["String_Button_Continue"] as string;
            settings.NegativeButtonText = Application.Current.Resources["String_Button_Cancel"] as string;

            settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            string message = Application.Current.Resources["String_SelectedSettingsAreOverwritten"] as string;

            if (ImportApplicationSettingsExists && (ImportEverything || ImportApplicationSettings))
                message += Environment.NewLine + Environment.NewLine + string.Format("* {0}", Application.Current.Resources["String_ApplicationIsRestartedAfterwards"] as string);

            if (await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_AreYouSure"] as string, message, MessageDialogStyle.AffirmativeAndNegative, settings) == MessageDialogResult.Affirmative)
            {
                List<ImportExportManager.ImportExportOptions> importOptions = new List<ImportExportManager.ImportExportOptions>();

                if (ImportApplicationSettingsExists && (ImportEverything || ImportApplicationSettings))
                    importOptions.Add(ImportExportManager.ImportExportOptions.ApplicationSettings);

                if (ImportNetworkInterfaceProfilesExists && (ImportEverything || ImportNetworkInterfaceProfiles))
                {
                    importOptions.Add(ImportExportManager.ImportExportOptions.NetworkInterfaceProfiles);

                    // Load network interface profile (option: add)
                    if (NetworkInterfaceProfileManager.Profiles == null)
                        NetworkInterfaceProfileManager.Load(!ImportOverrideNetworkInterfaceProfiles);
                }

                if (ImportIPScannerProfilesExists && (ImportEverything || ImportIPScannerProfiles))
                {
                    importOptions.Add(ImportExportManager.ImportExportOptions.IPScannerProfiles);

                    // Load ip scanner profiles (option: add)
                    if (IPScannerProfileManager.Profiles == null)
                        IPScannerProfileManager.Load(!ImportOverrideIPScannerProfiles);
                }

                if (ImportWakeOnLANClientsExists && (ImportEverything || ImportWakeOnLANClients))
                {
                    importOptions.Add(ImportExportManager.ImportExportOptions.WakeOnLANClients);

                    // Load WoL clients (option: add)
                    if (WakeOnLANClientManager.Clients == null)
                        WakeOnLANClientManager.Load(!ImportOverrideWakeOnLANClients);
                }

                if (ImportPortScannerProfilesExists && (ImportEverything || ImportPortScannerProfiles))
                {
                    importOptions.Add(ImportExportManager.ImportExportOptions.PortScannerProfiles);

                    // Load port scanner profiles (option: add)
                    if (PortScannerProfileManager.Profiles == null)
                        PortScannerProfileManager.Load(!ImportOverridePortScannerProfiles);
                }

                if (ImportRemoteDesktopSessionsExists && (ImportEverything || ImportRemoteDesktopSessions))
                {
                    importOptions.Add(ImportExportManager.ImportExportOptions.RemoteDesktopSessions);

                    // Load remote desktop sessions (option: add)
                    if (RemoteDesktopSessionManager.Sessions == null)
                        RemoteDesktopSessionManager.Load(!ImportOverrideRemoteDesktopSessions);
                }

                // Import (copy) files from zip archive
                ImportExportManager.Import(ImportLocationSelectedPath, importOptions);

                // Do the import (replace or add)
                if (importOptions.Contains(ImportExportManager.ImportExportOptions.NetworkInterfaceProfiles))
                    NetworkInterfaceProfileManager.Import(ImportEverything || ImportOverrideNetworkInterfaceProfiles);

                if (importOptions.Contains(ImportExportManager.ImportExportOptions.IPScannerProfiles))
                    IPScannerProfileManager.Import(ImportEverything || ImportOverrideIPScannerProfiles);

                if (importOptions.Contains(ImportExportManager.ImportExportOptions.WakeOnLANClients))
                    WakeOnLANClientManager.Import(ImportEverything || ImportOverrideWakeOnLANClients);

                if (importOptions.Contains(ImportExportManager.ImportExportOptions.PortScannerProfiles))
                    PortScannerProfileManager.Import(ImportEverything || ImportOverridePortScannerProfiles);

                if (importOptions.Contains(ImportExportManager.ImportExportOptions.RemoteDesktopSessions))
                    RemoteDesktopSessionManager.Import(ImportEverything || ImportOverrideRemoteDesktopSessions);

                // Show the user a message what happened
                if (!ImportExportManager.ForceRestart)
                {
                    settings.AffirmativeButtonText = Application.Current.Resources["String_Button_OK"] as string;

                    message = Application.Current.Resources["String_SettingsSuccessfullyImported"] as string + Environment.NewLine;

                    if (importOptions.Contains(ImportExportManager.ImportExportOptions.NetworkInterfaceProfiles))
                        message += Environment.NewLine + string.Format("* {0}", Application.Current.Resources["String_NetworkInterfaceProfilesReloaded"] as string);

                    if (importOptions.Contains(ImportExportManager.ImportExportOptions.IPScannerProfiles))
                        message += Environment.NewLine + string.Format("* {0}", Application.Current.Resources["String_IPScannerProfilesReloaded"] as string);

                    if (importOptions.Contains(ImportExportManager.ImportExportOptions.WakeOnLANClients))
                        message += Environment.NewLine + string.Format("* {0}", Application.Current.Resources["String_WakeOnLANClientsReloaded"] as string);

                    if (importOptions.Contains(ImportExportManager.ImportExportOptions.PortScannerProfiles))
                        message += Environment.NewLine + string.Format("* {0}", Application.Current.Resources["String_PortScannerProfilesReloaded"] as string);

                    if(importOptions.Contains(ImportExportManager.ImportExportOptions.RemoteDesktopSessions))
                        message += Environment.NewLine + string.Format("* {0}", Application.Current.Resources["String_RemoteDesktopSessionsReloaded"] as string);

                    await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Success"] as string, message, MessageDialogStyle.Affirmative, settings);

                    return;
                }

                // Close this view (this will restart the application)
                CloseAction();
            }
        }

        public ICommand ExportSettingsCommand
        {
            get { return new RelayCommand(p => ExportSettingsAction()); }
        }

        private async void ExportSettingsAction()
        {
            List<ImportExportManager.ImportExportOptions> exportOptions = new List<ImportExportManager.ImportExportOptions>();

            if (ApplicationSettingsExists && (ExportEverything || ExportApplicationSettings))
                exportOptions.Add(ImportExportManager.ImportExportOptions.ApplicationSettings);

            if (NetworkInterfaceProfilesExists && (ExportEverything || ExportNetworkInterfaceProfiles))
                exportOptions.Add(ImportExportManager.ImportExportOptions.NetworkInterfaceProfiles);

            if (IPScannerProfilesExists && (ExportEverything || ExportIPScannerProfiles))
                exportOptions.Add(ImportExportManager.ImportExportOptions.IPScannerProfiles);

            if (WakeOnLANClientsExists && (ExportEverything || ExportWakeOnLANClients))
                exportOptions.Add(ImportExportManager.ImportExportOptions.WakeOnLANClients);

            if (PortScannerProfilesExists && (ExportEverything || ExportPortScannerProfiles))
                exportOptions.Add(ImportExportManager.ImportExportOptions.PortScannerProfiles);

            if (RemoteDesktopSessionsExists && (ExportEverything || ExportRemoteDesktopSessions))
                exportOptions.Add(ImportExportManager.ImportExportOptions.RemoteDesktopSessions);

            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = ImportExportFileExtensionFilter,
                FileName = string.Format("{0}_{1}_{2}{3}", Application.Current.Resources["String_ProductName"] as string, Application.Current.Resources["String_Backup"] as string, TimestampHelper.GetTimestamp(), ImportExportManager.ImportExportFileExtension)
            };

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImportExportManager.Export(exportOptions, saveFileDialog.FileName);

                MetroDialogSettings settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = Application.Current.Resources["String_Button_OK"] as string;

                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Success"] as string, string.Format("{0}\n\n{1}: {2}", Application.Current.Resources["String_SettingsSuccessfullyExported"] as string, Application.Current.Resources["String_Path"] as string, saveFileDialog.FileName), MessageDialogStyle.Affirmative, settings);
            }
        }
        #endregion

        #region Methods
        public void SaveAndCheckSettings()
        {
            // Save everything
            if (SettingsManager.Current.SettingsChanged)
                SettingsManager.Save();

            if (NetworkInterfaceProfileManager.ProfilesChanged)
                NetworkInterfaceProfileManager.Save();

            if (IPScannerProfileManager.ProfilesChanged)
                IPScannerProfileManager.Save();

            if (WakeOnLANClientManager.ClientsChanged)
                WakeOnLANClientManager.Save();

            if (PortScannerProfileManager.ProfilesChanged)
                PortScannerProfileManager.Save();

            if (RemoteDesktopSessionManager.SessionsChanged)
                RemoteDesktopSessionManager.Save();

            // Check if files exist
            ApplicationSettingsExists = File.Exists(SettingsManager.GetSettingsFilePath());
            NetworkInterfaceProfilesExists = File.Exists(NetworkInterfaceProfileManager.GetProfilesFilePath());
            IPScannerProfilesExists = File.Exists(IPScannerProfileManager.GetProfilesFilePath());
            WakeOnLANClientsExists = File.Exists(WakeOnLANClientManager.GetClientsFilePath());
            PortScannerProfilesExists = File.Exists(PortScannerProfileManager.GetProfilesFilePath());
            RemoteDesktopSessionsExists = File.Exists(RemoteDesktopSessionManager.GetSessionsFilePath());
        }
        #endregion
    }
}