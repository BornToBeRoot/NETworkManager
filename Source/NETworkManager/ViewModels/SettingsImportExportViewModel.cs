using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.ImportExport;
using NETworkManager.Models.Profile;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels
{
    public class SettingsImportExportViewModel : ViewModelBase
    {
        private readonly IDialogCoordinator _dialogCoordinator;

        public Action CloseAction { get; set; }

        private const string ImportExportFileExtensionFilter = "ZIP Archive (*.zip)|*.zip";

        #region Variables
        #region Import
        private string _importFilePath;
        public string ImportFilePath
        {
            get => _importFilePath;
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
            get => _importFileIsValid;
            set
            {
                if (value == _importFileIsValid)
                    return;

                _importFileIsValid = value;
                OnPropertyChanged();
            }
        }

        private bool _importEverything = true;
        public bool ImportEverything
        {
            get => _importEverything;
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
            get => _importSettingsExists;
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
            get => _importSettings;
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
            get => _importProfilesExists;
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
            get => _importProfiles;
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
            get => _importOverrideProfiles;
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
            get => _exportEverything;
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
            get => _settingsExists;
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
            get => _exportSettings;
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
            get => _profilesExists;
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
            get => _exportProfiles;
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
            _dialogCoordinator = instance;
        }
        #endregion

        #region ICommands & Actions
        public ICommand BrowseFileCommand => new RelayCommand(p => BrowseFileAction());

        private void BrowseFileAction()
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = ImportExportFileExtensionFilter
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                ImportFilePath = openFileDialog.FileName;
        }

        public ICommand ValidateImportSettingsCommand => new RelayCommand(p => ValidateImportSettingsAction());

        private async void ValidateImportSettingsAction()
        {
            try
            {
                var importOptions = ImportExportManager.ValidateImportFile(ImportFilePath);

                ImportFileIsValid = true;
                ImportSettingsExists = importOptions.Contains(ImportExportManager.ImportExportOptions.Settings);
                ImportProfilesExists = importOptions.Contains(ImportExportManager.ImportExportOptions.Profiles);
            }
            catch (ImportFileNotValidException)
            {
                await _dialogCoordinator.ShowMessageAsync(this, Resources.Localization.Strings.ValidationFailed, Resources.Localization.Strings.NoValidFileFoundToImport, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
            }
        }

        public ICommand ImportSettingsCommand => new RelayCommand(p => ImportSettingsAction());

        private async void ImportSettingsAction()
        {
            var settings = AppearanceManager.MetroDialog;

            settings.AffirmativeButtonText = Resources.Localization.Strings.Continue;
            settings.NegativeButtonText = Resources.Localization.Strings.Cancel;

            settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            var message = Resources.Localization.Strings.SelectedSettingsAreOverwritten;

            if (ImportSettingsExists && (ImportEverything || ImportSettings))
                message += Environment.NewLine + Environment.NewLine + $"* {Resources.Localization.Strings.ApplicationIsRestartedAfterwards}";

            if (await _dialogCoordinator.ShowMessageAsync(this, Resources.Localization.Strings.AreYouSure, message, MessageDialogStyle.AffirmativeAndNegative, settings) != MessageDialogResult.Affirmative)
                return;

            var importOptions = new List<ImportExportManager.ImportExportOptions>();

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
                settings.AffirmativeButtonText = Resources.Localization.Strings.OK;

                message = Resources.Localization.Strings.SettingsSuccessfullyImported + Environment.NewLine;

                if (importOptions.Contains(ImportExportManager.ImportExportOptions.Profiles))
                    message += Environment.NewLine + $"* {Resources.Localization.Strings.ProfilesReloaded}";

                await _dialogCoordinator.ShowMessageAsync(this, Resources.Localization.Strings.Success, message, MessageDialogStyle.Affirmative, settings);

                return;
            }

            // Close this view (this will restart the application)
            CloseAction();
        }

        public ICommand ExportSettingsCommand => new RelayCommand(p => ExportSettingsAction());

        private async void ExportSettingsAction()
        {
            var exportOptions = new List<ImportExportManager.ImportExportOptions>();

            if (SettingsExists && (ExportEverything || ExportSettings))
                exportOptions.Add(ImportExportManager.ImportExportOptions.Settings);

            if (ProfilesExists && (ExportEverything || ExportProfiles))
                exportOptions.Add(ImportExportManager.ImportExportOptions.Profiles);

            var saveFileDialog = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = ImportExportFileExtensionFilter,
                FileName = $"{Properties.Resources.NETworkManager_ProjectName}_{Resources.Localization.Strings.Backup}_{TimestampHelper.GetTimestamp()}{ImportExportManager.ImportExportFileExtension}"
            };

            if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            ImportExportManager.Export(exportOptions, saveFileDialog.FileName);

            var settings = AppearanceManager.MetroDialog;

            settings.AffirmativeButtonText = Resources.Localization.Strings.OK;

            await _dialogCoordinator.ShowMessageAsync(this, Resources.Localization.Strings.Success, $"{Resources.Localization.Strings.SettingsSuccessfullyExported}\n\n{Resources.Localization.Strings.Path}: {saveFileDialog.FileName}", MessageDialogStyle.Affirmative, settings);
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