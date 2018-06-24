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

        private bool _importSettingsExists;
        public bool ImportSettingsExists
        {
            get { return _importSettingsExists; }
            set
            {
                if (value == _importSettingsExists)
                    return;

                _importSettingsExists = value;
                OnPropertyChanged();
            }
        }

        private bool _importSettings;
        public bool ImportSettings
        {
            get { return _importSettings; }
            set
            {
                if (value == _importSettings)
                    return;

                _importSettings = value;
                OnPropertyChanged();
            }
        }

        private bool _importProfilesExists;
        public bool ImportProfilesExists
        {
            get { return _importProfilesExists; }
            set
            {
                if (value == _importProfilesExists)
                    return;

                _importProfilesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _importProfiles;
        public bool ImportProfiles
        {
            get { return _importProfiles; }
            set
            {
                if (value == _importProfiles)
                    return;

                _importProfiles = value;
                OnPropertyChanged();
            }
        }

        private bool _importOverrideProfiles = true;
        public bool ImportOverrideProfiles
        {
            get { return _importOverrideProfiles; }
            set
            {
                if (value == _importOverrideProfiles)
                    return;

                _importOverrideProfiles = value;
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

        private bool _settingsExists;
        public bool SettingsExists
        {
            get { return _settingsExists; }
            set
            {
                if (value == _settingsExists)
                    return;

                _settingsExists = value;
                OnPropertyChanged();
            }
        }

        private bool _exportSettings;
        public bool ExportSettings
        {
            get { return _exportSettings; }
            set
            {
                if (value == _exportSettings)
                    return;

                _exportSettings = value;
                OnPropertyChanged();
            }
        }

        private bool _profilesExists;
        public bool ProfilesExists
        {
            get { return _profilesExists; }
            set
            {
                if (value == _profilesExists)
                    return;

                _profilesExists = value;
                OnPropertyChanged();
            }
        }

        private bool _exportProfiles;
        public bool ExportProfiles
        {
            get { return _exportProfiles; }
            set
            {
                if (value == _exportProfiles)
                    return;

                _exportProfiles = value;
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
                ImportSettingsExists = importOptions.Contains(ImportExportManager.ImportExportOptions.Settings);
                ImportProfilesExists = importOptions.Contains(ImportExportManager.ImportExportOptions.Profiles);
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

            if (ImportSettingsExists && (ImportEverything || ImportSettings))
                message += Environment.NewLine + Environment.NewLine + string.Format("* {0}", LocalizationManager.GetStringByKey("String_ApplicationIsRestartedAfterwards"));

            if (await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_AreYouSure"), message, MessageDialogStyle.AffirmativeAndNegative, settings) == MessageDialogResult.Affirmative)
            {
                List<ImportExportManager.ImportExportOptions> importOptions = new List<ImportExportManager.ImportExportOptions>();

                if (ImportSettingsExists && (ImportEverything || ImportSettings))
                    importOptions.Add(ImportExportManager.ImportExportOptions.Settings);
                                
                if (ImportProfilesExists && (ImportEverything || ImportProfiles))
                {
                    importOptions.Add(ImportExportManager.ImportExportOptions.Profiles);

                    // Load network interface profile (option: add)
                    if (ProfileManager.Profiles == null)
                        ProfileManager.Load(!ImportOverrideProfiles);
                }

                // Import (copy) files from zip archive
                ImportExportManager.Import(ImportFilePath, importOptions);

                // Do the import (replace or add)
                if (importOptions.Contains(ImportExportManager.ImportExportOptions.Profiles))
                   ProfileManager.Import(ImportEverything || ImportOverrideProfiles);

                // Show the user a message what happened
                if (!ImportExportManager.ForceRestart)
                {
                    settings.AffirmativeButtonText = LocalizationManager.GetStringByKey("String_Button_OK");

                    message = LocalizationManager.GetStringByKey("String_SettingsSuccessfullyImported") + Environment.NewLine;

                    if (importOptions.Contains(ImportExportManager.ImportExportOptions.Profiles))
                        message += Environment.NewLine + string.Format("* {0}", LocalizationManager.GetStringByKey("String_ProfilesReloaded"));

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

            if (SettingsExists && (ExportEverything || ExportSettings))
                exportOptions.Add(ImportExportManager.ImportExportOptions.Settings);

            if (ProfilesExists && (ExportEverything || ExportProfiles))
                exportOptions.Add(ImportExportManager.ImportExportOptions.Profiles);

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

            // Check if files exist
            SettingsExists = File.Exists(SettingsManager.GetSettingsFilePath());
            ProfilesExists = File.Exists(ProfileManager.GetProfilesFilePath());
        }

        public void SetImportLocationFilePathFromDragDrop(string filePath)
        {
            ImportFilePath = filePath;
                        
            OnPropertyChanged(nameof(ImportFilePath));
        }
        #endregion
    }
}