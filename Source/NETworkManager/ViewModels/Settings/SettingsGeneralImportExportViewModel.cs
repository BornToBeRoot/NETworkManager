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

        MetroDialogSettings dialogSettings = new MetroDialogSettings();

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

        private bool _importNetworkInterfaceConfigTemplatesExists;
        public bool ImportNetworkInterfaceConfigTemplatesExists
        {
            get { return _importNetworkInterfaceConfigTemplatesExists; }
            set
            {
                if (value == _importNetworkInterfaceConfigTemplatesExists)
                    return;

                _importNetworkInterfaceConfigTemplatesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _importNetworkInterfaceConfigTemplates;
        public bool ImportNetworkInterfaceConfigTemplates
        {
            get { return _importNetworkInterfaceConfigTemplates; }
            set
            {
                if (value == _importNetworkInterfaceConfigTemplates)
                    return;

                _importNetworkInterfaceConfigTemplates = value;
                OnPropertyChanged();
            }
        }

        private bool _importWakeOnLANTemplatesExists;
        public bool ImportWakeOnLANTemplatesExists
        {
            get { return _importWakeOnLANTemplatesExists; }
            set
            {
                if (value == _importWakeOnLANTemplatesExists)
                    return;

                _importWakeOnLANTemplatesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _importWakeOnLANTemplates;
        public bool ImportWakeOnLANTemplates
        {
            get { return _importWakeOnLANTemplates; }
            set
            {
                if (value == _importWakeOnLANTemplates)
                    return;

                _importWakeOnLANTemplates = value;
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

        private bool _networkInterfaceConfigTemplatesExists;
        public bool NetworkInterfaceConfigTemplatesExists
        {
            get { return _networkInterfaceConfigTemplatesExists; }
            set
            {
                if (value == _networkInterfaceConfigTemplatesExists)
                    return;

                _networkInterfaceConfigTemplatesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _exportNetworkInterfaceConfigTemplates;
        public bool ExportNetworkInterfaceConfigTemplates
        {
            get { return _exportNetworkInterfaceConfigTemplates; }
            set
            {
                if (value == _exportNetworkInterfaceConfigTemplates)
                    return;

                _exportNetworkInterfaceConfigTemplates = value;
                OnPropertyChanged();
            }
        }

        private bool _wakeOnLanTemplatesExists;
        public bool WakeOnLANTemplatesExists
        {
            get { return _wakeOnLanTemplatesExists; }
            set
            {
                if (value == _wakeOnLanTemplatesExists)
                    return;

                _wakeOnLanTemplatesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _exportWakeOnLANTemplates;
        public bool ExportWakeOnLANTemplates
        {
            get { return _exportWakeOnLANTemplates; }
            set
            {
                if (value == _exportWakeOnLANTemplates)
                    return;

                _exportWakeOnLANTemplates = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor, LoadSettings
        public SettingsGeneralImportExportViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            dialogSettings.CustomResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
            };

            LoadSettings();
        }

        private void LoadSettings()
        {
            ApplicationSettingsExists = File.Exists(SettingsManager.SettingsFilePath);
            NetworkInterfaceConfigTemplatesExists = File.Exists(TemplateManager.NetworkInterfaceConfigTemplatesFilePath);
            WakeOnLANTemplatesExists = File.Exists(TemplateManager.WakeOnLANTemplatesFilePath);
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
                ImportNetworkInterfaceConfigTemplatesExists = importOptions.Contains(ImportExportManager.ImportExportOptions.NetworkInterfaceConfigTemplates);
                ImportWakeOnLANTemplatesExists = importOptions.Contains(ImportExportManager.ImportExportOptions.WakeOnLANTemplates);
            }
            catch (ImportFileNotValidException)
            {
                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_ValidationFailed"] as string, Application.Current.Resources["String_NoValidFileFoundToImport"] as string, MessageDialogStyle.Affirmative, dialogSettings);
            }
        }

        public ICommand ImportSettingsCommand
        {
            get { return new RelayCommand(p => ImportSettingsAction()); }
        }

        private async void ImportSettingsAction()
        {
            MetroDialogSettings settings = dialogSettings;

            settings.AffirmativeButtonText = Application.Current.Resources["String_Button_Continue"] as string;
            settings.NegativeButtonText = Application.Current.Resources["String_Button_Cancel"] as string;

            settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            if (await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_AreYouSure"] as string, Application.Current.Resources["String_SelectedSettingsAreOverwritten"] as string, MessageDialogStyle.AffirmativeAndNegative, settings) == MessageDialogResult.Affirmative)
            {
                List<ImportExportManager.ImportExportOptions> importOptions = new List<ImportExportManager.ImportExportOptions>();

                if (ImportApplicationSettingsExists && (ImportEverything || ImportApplicationSettings))
                    importOptions.Add(ImportExportManager.ImportExportOptions.ApplicationSettings);

                if (ImportNetworkInterfaceConfigTemplatesExists && (ImportEverything || ImportNetworkInterfaceConfigTemplates))
                    importOptions.Add(ImportExportManager.ImportExportOptions.NetworkInterfaceConfigTemplates);

                if (ImportWakeOnLANTemplatesExists && (ImportEverything || ImportWakeOnLANTemplates))
                    importOptions.Add(ImportExportManager.ImportExportOptions.WakeOnLANTemplates);

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

            if (NetworkInterfaceConfigTemplatesExists && (ExportEverything || ExportNetworkInterfaceConfigTemplates))
                exportOptions.Add(ImportExportManager.ImportExportOptions.NetworkInterfaceConfigTemplates);

            if (WakeOnLANTemplatesExists && (ExportEverything || ExportWakeOnLANTemplates))
                exportOptions.Add(ImportExportManager.ImportExportOptions.WakeOnLANTemplates);

            // Save the settings before exporting them
            if (SettingsManager.Current.SettingsChanged)
                SettingsManager.Save();

            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = ImportExportFileExtensionFilter,
                FileName = string.Format("{0}_{1}{2}", Application.Current.Resources["String_ProductName"] as string, TimestampHelper.GetTimestamp(), ImportExportManager.ImportExportFileExtension)
            };

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImportExportManager.Export(exportOptions, saveFileDialog.FileName);

                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Success"] as string, string.Format("{0}\n\n{1}: {2}", Application.Current.Resources["String_SettingsSuccessfullyExported"] as string, Application.Current.Resources["String_Path"] as string, saveFileDialog.FileName), MessageDialogStyle.Affirmative, dialogSettings);
            }
        }
    }
    #endregion
}