using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsImportExportViewModel : ViewModelBase
    {
        private IDialogCoordinator dialogCoordinator;

        public Action CloseAction { get; set; }

        private const string ImportExportFileExtensionFilter = "ZIP Archive (*.zip)|*.zip";

        #region Variables
        #region Import
        private string _importFilePath;
        public string ImportFilePath
        {
            get { return _importFilePath; }
            set
            {
                if (value == _importFilePath)
                    return;

                ImportFileIsValid = false;

                _importFilePath = value;
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

        private bool _importPingProfilesExists;
        public bool ImportPingProfilesExists
        {
            get { return _importPingProfilesExists; }
            set
            {
                if (value == _importPingProfilesExists)
                    return;

                _importPingProfilesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _importPingProfiles;
        public bool ImportPingProfiles
        {
            get { return _importPingProfiles; }
            set
            {
                if (value == _importPingProfiles)
                    return;

                _importPingProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _importOverridePingProfiles = true;
        public bool ImportOverridePingProfiles
        {
            get { return _importOverridePingProfiles; }
            set
            {
                if (value == _importOverridePingProfiles)
                    return;

                _importOverridePingProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _importTracerouteProfilesExists;
        public bool ImportTracerouteProfilesExists
        {
            get { return _importTracerouteProfilesExists; }
            set
            {
                if (value == _importTracerouteProfilesExists)
                    return;

                _importTracerouteProfilesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _importTracerouteProfiles;
        public bool ImportTracerouteProfiles
        {
            get { return _importTracerouteProfiles; }
            set
            {
                if (value == _importTracerouteProfiles)
                    return;

                _importTracerouteProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _importOverrideTracerouteProfiles = true;
        public bool ImportOverrideTracerouteProfiles
        {
            get { return _importOverrideTracerouteProfiles; }
            set
            {
                if (value == _importOverrideTracerouteProfiles)
                    return;

                _importOverrideTracerouteProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _importRemoteDesktopProfilesExists;
        public bool ImportRemoteDesktopProfilesExists
        {
            get { return _importRemoteDesktopProfilesExists; }
            set
            {
                if (value == _importRemoteDesktopProfilesExists)
                    return;

                _importRemoteDesktopProfilesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _importRemoteDesktopProfiles;
        public bool ImportRemoteDesktopProfiles
        {
            get { return _importRemoteDesktopProfiles; }
            set
            {
                if (value == _importRemoteDesktopProfiles)
                    return;

                _importRemoteDesktopProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _importOverrideRemoteDesktopProfiles = true;
        public bool ImportOverrideRemoteDesktopProfiles
        {
            get { return _importOverrideRemoteDesktopProfiles; }
            set
            {
                if (value == _importOverrideRemoteDesktopProfiles)
                    return;

                _importOverrideRemoteDesktopProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _importPuTTYProfilesExists;
        public bool ImportPuTTYProfilesExists
        {
            get { return _importPuTTYProfilesExists; }
            set
            {
                if (value == _importPuTTYProfilesExists)
                    return;

                _importPuTTYProfilesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _importPuTTYProfiles;
        public bool ImportPuTTYProfiles
        {
            get { return _importPuTTYProfiles; }
            set
            {
                if (value == _importPuTTYProfiles)
                    return;

                _importPuTTYProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _importOverridePuTTYProfiles = true;
        public bool ImportOverridePuTTYProfiles
        {
            get { return _importOverridePuTTYProfiles; }
            set
            {
                if (value == _importOverridePuTTYProfiles)
                    return;

                _importOverridePuTTYProfiles = value;
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

        private bool _pingProfilesExists;
        public bool PingProfilesExists
        {
            get { return _pingProfilesExists; }
            set
            {
                if (value == _pingProfilesExists)
                    return;

                _pingProfilesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _exportPingProfiles;
        public bool ExportPingProfiles
        {
            get { return _exportPingProfiles; }
            set
            {
                if (value == _exportPingProfiles)
                    return;

                _exportPingProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _tracerouteProfilesExists;
        public bool TracerouteProfilesExists
        {
            get { return _tracerouteProfilesExists; }
            set
            {
                if (value == _tracerouteProfilesExists)
                    return;

                _tracerouteProfilesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _exportTracerouteProfiles;
        public bool ExportTracerouteProfiles
        {
            get { return _exportTracerouteProfiles; }
            set
            {
                if (value == _exportTracerouteProfiles)
                    return;

                _exportTracerouteProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktopSesionsExists;
        public bool RemoteDesktopProfilesExists
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

        private bool _exportRemoteDesktopProfiles;
        public bool ExportRemoteDesktopProfiles
        {
            get { return _exportRemoteDesktopProfiles; }
            set
            {
                if (value == _exportRemoteDesktopProfiles)
                    return;

                _exportRemoteDesktopProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _puTTYSesionsExists;
        public bool PuTTYProfilesExists
        {
            get { return _puTTYSesionsExists; }
            set
            {
                if (value == _puTTYSesionsExists)
                    return;

                _puTTYSesionsExists = value;
                OnPropertyChanged();
            }
        }

        private bool _exportPuTTYProfiles;
        public bool ExportPuTTYProfiles
        {
            get { return _exportPuTTYProfiles; }
            set
            {
                if (value == _exportPuTTYProfiles)
                    return;

                _exportPuTTYProfiles = value;
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
        #endregion
        #endregion

        #region Constructor
        public SettingsImportExportViewModel(IDialogCoordinator instance)
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
                ImportFilePath = openFileDialog.FileName;
        }

        public ICommand ValidateImportSettingsCommand
        {
            get { return new RelayCommand(p => ValidateImportSettingsAction()); }
        }

        private async void ValidateImportSettingsAction()
        {
            try
            {
                List<ImportExportManager.ImportExportOptions> importOptions = ImportExportManager.ValidateImportFile(ImportFilePath);

                ImportFileIsValid = true;
                ImportApplicationSettingsExists = importOptions.Contains(ImportExportManager.ImportExportOptions.ApplicationSettings);
                ImportNetworkInterfaceProfilesExists = importOptions.Contains(ImportExportManager.ImportExportOptions.NetworkInterfaceProfiles);
                ImportIPScannerProfilesExists = importOptions.Contains(ImportExportManager.ImportExportOptions.IPScannerProfiles);
                ImportPortScannerProfilesExists = importOptions.Contains(ImportExportManager.ImportExportOptions.PortScannerProfiles);
                ImportPingProfilesExists = importOptions.Contains(ImportExportManager.ImportExportOptions.PingProfiles);
                ImportTracerouteProfilesExists = importOptions.Contains(ImportExportManager.ImportExportOptions.TracerouteProfiles);
                ImportRemoteDesktopProfilesExists = importOptions.Contains(ImportExportManager.ImportExportOptions.RemoteDesktopProfiles);
                ImportPuTTYProfilesExists = importOptions.Contains(ImportExportManager.ImportExportOptions.PuTTYProfiles);
                ImportWakeOnLANClientsExists = importOptions.Contains(ImportExportManager.ImportExportOptions.WakeOnLANClients);
            }
            catch (ImportFileNotValidException)
            {
                await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_ValidationFailed"), LocalizationManager.GetStringByKey("String_NoValidFileFoundToImport"), MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
            }
        }

        public ICommand ImportSettingsCommand
        {
            get { return new RelayCommand(p => ImportSettingsAction()); }
        }

        private async void ImportSettingsAction()
        {
            MetroDialogSettings settings = AppearanceManager.MetroDialog;

            settings.AffirmativeButtonText = LocalizationManager.GetStringByKey("String_Button_Continue");
            settings.NegativeButtonText = LocalizationManager.GetStringByKey("String_Button_Cancel");

            settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            string message = LocalizationManager.GetStringByKey("String_SelectedSettingsAreOverwritten");

            if (ImportApplicationSettingsExists && (ImportEverything || ImportApplicationSettings))
                message += Environment.NewLine + Environment.NewLine + string.Format("* {0}", LocalizationManager.GetStringByKey("String_ApplicationIsRestartedAfterwards"));

            if (await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_AreYouSure"), message, MessageDialogStyle.AffirmativeAndNegative, settings) == MessageDialogResult.Affirmative)
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

                // Import (copy) files from zip archive
                ImportExportManager.Import(ImportFilePath, importOptions);

                // Do the import (replace or add)
                if (importOptions.Contains(ImportExportManager.ImportExportOptions.NetworkInterfaceProfiles))
                    NetworkInterfaceProfileManager.Import(ImportEverything || ImportOverrideNetworkInterfaceProfiles);

                if (importOptions.Contains(ImportExportManager.ImportExportOptions.PuTTYProfiles))
                    PuTTYProfileManager.Import(ImportEverything || ImportOverridePuTTYProfiles);

                // Show the user a message what happened
                if (!ImportExportManager.ForceRestart)
                {
                    settings.AffirmativeButtonText = LocalizationManager.GetStringByKey("String_Button_OK");

                    message = LocalizationManager.GetStringByKey("String_SettingsSuccessfullyImported") + Environment.NewLine;

                    if (importOptions.Contains(ImportExportManager.ImportExportOptions.NetworkInterfaceProfiles))
                        message += Environment.NewLine + string.Format("* {0}", LocalizationManager.GetStringByKey("String_NetworkInterfaceProfilesReloaded"));

                    if (importOptions.Contains(ImportExportManager.ImportExportOptions.IPScannerProfiles))
                        message += Environment.NewLine + string.Format("* {0}", LocalizationManager.GetStringByKey("String_IPScannerProfilesReloaded"));

                    if (importOptions.Contains(ImportExportManager.ImportExportOptions.PortScannerProfiles))
                        message += Environment.NewLine + string.Format("* {0}", LocalizationManager.GetStringByKey("String_PortScannerProfilesReloaded"));

                    if (importOptions.Contains(ImportExportManager.ImportExportOptions.PingProfiles))
                        message += Environment.NewLine + string.Format("* {0}", LocalizationManager.GetStringByKey("String_PingProfilesReloaded"));

                    if (importOptions.Contains(ImportExportManager.ImportExportOptions.TracerouteProfiles))
                        message += Environment.NewLine + string.Format("* {0}", LocalizationManager.GetStringByKey("String_TracerouteProfilesReloaded"));

                    if (importOptions.Contains(ImportExportManager.ImportExportOptions.RemoteDesktopProfiles))
                        message += Environment.NewLine + string.Format("* {0}", LocalizationManager.GetStringByKey("String_RemoteDesktopProfilesReloaded"));

                    if (importOptions.Contains(ImportExportManager.ImportExportOptions.PuTTYProfiles))
                        message += Environment.NewLine + string.Format("* {0}", LocalizationManager.GetStringByKey("String_PuTTYProfilesReloaded"));

                    if (importOptions.Contains(ImportExportManager.ImportExportOptions.WakeOnLANClients))
                        message += Environment.NewLine + string.Format("* {0}", LocalizationManager.GetStringByKey("String_WakeOnLANClientsReloaded"));

                    await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_Success"), message, MessageDialogStyle.Affirmative, settings);

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
                        
            if (PortScannerProfilesExists && (ExportEverything || ExportPortScannerProfiles))
                exportOptions.Add(ImportExportManager.ImportExportOptions.PortScannerProfiles);

            if (PingProfilesExists && (ExportEverything || ExportPingProfiles))
                exportOptions.Add(ImportExportManager.ImportExportOptions.PingProfiles);

            if (TracerouteProfilesExists && (ExportEverything || ExportTracerouteProfiles))
                exportOptions.Add(ImportExportManager.ImportExportOptions.TracerouteProfiles);

            if (RemoteDesktopProfilesExists && (ExportEverything || ExportRemoteDesktopProfiles))
                exportOptions.Add(ImportExportManager.ImportExportOptions.RemoteDesktopProfiles);

            if (PuTTYProfilesExists && (ExportEverything || ExportPuTTYProfiles))
                exportOptions.Add(ImportExportManager.ImportExportOptions.PuTTYProfiles);

            if (WakeOnLANClientsExists && (ExportEverything || ExportWakeOnLANClients))
                exportOptions.Add(ImportExportManager.ImportExportOptions.WakeOnLANClients);

            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = ImportExportFileExtensionFilter,
                FileName = string.Format("{0}_{1}_{2}{3}", LocalizationManager.GetStringByKey("String_ProductName"), LocalizationManager.GetStringByKey("String_Backup"), TimestampHelper.GetTimestamp(), ImportExportManager.ImportExportFileExtension)
            };

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImportExportManager.Export(exportOptions, saveFileDialog.FileName);

                MetroDialogSettings settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = LocalizationManager.GetStringByKey("String_Button_OK");

                await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_Success"), string.Format("{0}\n\n{1}: {2}", LocalizationManager.GetStringByKey("String_SettingsSuccessfullyExported"), LocalizationManager.GetStringByKey("String_Path"), saveFileDialog.FileName), MessageDialogStyle.Affirmative, settings);
            }
        }
        #endregion

        #region Methods
        public void SaveAndCheckSettings()
        {
            // Save everything
            if (SettingsManager.Current.SettingsChanged)
                SettingsManager.Save();

            if (ProfileManager.ProfilesChanged)
                ProfileManager.Save();

            if (NetworkInterfaceProfileManager.ProfilesChanged)
                NetworkInterfaceProfileManager.Save();

            if (PuTTYProfileManager.ProfilesChanged)
                PuTTYProfileManager.Save();

            // Check if files exist
            ApplicationSettingsExists = File.Exists(SettingsManager.GetSettingsFilePath());
            NetworkInterfaceProfilesExists = File.Exists(NetworkInterfaceProfileManager.GetProfilesFilePath());
            PuTTYProfilesExists = File.Exists(PuTTYProfileManager.GetProfilesFilePath());
        }

        public void SetImportLocationFilePathFromDragDrop(string filePath)
        {
            ImportFilePath = filePath;
                        
            OnPropertyChanged(nameof(ImportFilePath));
        }
        #endregion
    }
}