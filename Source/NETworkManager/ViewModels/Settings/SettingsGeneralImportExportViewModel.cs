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

        #region Constructor, LoadSettings
        public SettingsGeneralImportExportViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            LoadSettings();
        }

        private void LoadSettings()
        {
            ApplicationSettingsExists = File.Exists(SettingsManager.GetSettingsFilePath());
            NetworkInterfaceProfilesExists = File.Exists(NetworkInterfaceProfileManager.GetProfilesFilePath());
            WakeOnLANClientsExists = File.Exists(WakeOnLANClientManager.GetClientsFilePath());
        }
        #endregion

        #region ICommands & Actions
        public ICommand ImportBrowseFileCommand
        {
            get { return new RelayCommand(p => ImportBrowseFileAction()); }
        }

        private void ImportBrowseFileAction()
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
                ImportWakeOnLANClientsExists = importOptions.Contains(ImportExportManager.ImportExportOptions.WakeOnLANClients);
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

            if (await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_AreYouSure"] as string, Application.Current.Resources["String_SelectedSettingsAreOverwritten"] as string, MessageDialogStyle.AffirmativeAndNegative, settings) == MessageDialogResult.Affirmative)
            {
                List<ImportExportManager.ImportExportOptions> importOptions = new List<ImportExportManager.ImportExportOptions>();

                if (ImportApplicationSettingsExists && (ImportEverything || ImportApplicationSettings))
                    importOptions.Add(ImportExportManager.ImportExportOptions.ApplicationSettings);

                if (ImportNetworkInterfaceProfilesExists && (ImportEverything || ImportNetworkInterfaceProfiles))
                    importOptions.Add(ImportExportManager.ImportExportOptions.NetworkInterfaceProfiles);

                if (ImportWakeOnLANClientsExists && (ImportEverything || ImportWakeOnLANClients))
                    importOptions.Add(ImportExportManager.ImportExportOptions.WakeOnLANClients);

                ImportExportManager.Import(ImportLocationSelectedPath, importOptions);

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

            if (WakeOnLANClientsExists && (ExportEverything || ExportWakeOnLANClients))
                exportOptions.Add(ImportExportManager.ImportExportOptions.WakeOnLANClients);

            // Save the settings before exporting them
            if (SettingsManager.Current.SettingsChanged)
                SettingsManager.Save();

            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = ImportExportFileExtensionFilter,
                FileName = string.Format("{0}_{1}_{2}{3}", Application.Current.Resources["String_ProductName"] as string, Application.Current.Resources["String_Backup"] as string,TimestampHelper.GetTimestamp(), ImportExportManager.ImportExportFileExtension)
            };

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImportExportManager.Export(exportOptions, saveFileDialog.FileName);

                MetroDialogSettings settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = Application.Current.Resources["String_Button_OK"] as string;

                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Success"] as string, string.Format("{0}\n\n{1}: {2}", Application.Current.Resources["String_SettingsSuccessfullyExported"] as string, Application.Current.Resources["String_Path"] as string, saveFileDialog.FileName), MessageDialogStyle.Affirmative, settings);
            }
        }
    }
    #endregion
}