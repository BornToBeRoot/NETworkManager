using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class SettingsSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        public Action CloseAction { get; set; }

        public bool IsPortable => ConfigurationManager.Current.IsPortable;

        private string _location;
        public string Location
        {
            get => _location;
            set
            {
                if (value == _location)
                    return;

                _location = value;
                OnPropertyChanged();
            }
        }

        private bool _movingFiles;
        public bool MovingFiles
        {
            get => _movingFiles;
            set
            {
                if (value == _movingFiles)
                    return;

                _movingFiles = value;
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

        private string _importFilePath;
        public string ImportFilePath
        {
            get => _importFilePath;
            set
            {
                if (value == _importFilePath)
                    return;

                _importFilePath = value;
                OnPropertyChanged();
            }
        }

        private bool _displayImportStatusMessage;
        public bool DisplayImportStatusMessage
        {
            get => _displayImportStatusMessage;
            set
            {
                if (value == _displayImportStatusMessage)
                    return;

                _displayImportStatusMessage = value;
                OnPropertyChanged();
            }
        }

        private string _importStatusMessage;
        public string ImportStatusMessage
        {
            get => _importStatusMessage;
            set
            {
                if (value == _importStatusMessage)
                    return;

                _importStatusMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _displayExportStatusMessage;
        public bool DisplayExportStatusMessage
        {
            get => _displayExportStatusMessage;
            set
            {
                if (value == _displayExportStatusMessage)
                    return;

                _displayExportStatusMessage = value;
                OnPropertyChanged();
            }
        }

        private string _exportStatusMessage;
        public string ExportStatusMessage
        {
            get => _exportStatusMessage;
            set
            {
                if (value == _exportStatusMessage)
                    return;

                _exportStatusMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _resetSettings;
        public bool ResetSettings
        {
            get => _resetSettings;
            set
            {
                if (value == _resetSettings)
                    return;

                _resetSettings = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, LoadSettings
        public SettingsSettingsViewModel(IDialogCoordinator instance)
        {
            _dialogCoordinator = instance;

            LoadSettings();
        }

        private void LoadSettings()
        {
            Location = SettingsManager.GetSettingsLocation();
        }
        #endregion

        #region ICommands & Actions
        public ICommand BrowseLocationFolderCommand => new RelayCommand(p => BrowseLocationFolderAction());

        private void BrowseLocationFolderAction()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (Directory.Exists(Location))
                    dialog.SelectedPath = Location;

                var dialogResult = dialog.ShowDialog();

                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                    Location = dialog.SelectedPath;
            }
        }

        public ICommand OpenLocationCommand => new RelayCommand(p => OpenLocationAction());

        private static void OpenLocationAction()
        {
            Process.Start("explorer.exe", SettingsManager.GetSettingsLocation());
        }

        public ICommand ChangeLocationCommand => new RelayCommand(p => ChangeLocationAction());

        private async Task ChangeLocationAction()
        {
            MovingFiles = true;

            var useFileInOtherLocation = false;

            // Check if settings file exists in new location
            if (File.Exists(Path.Combine(Location, SettingsManager.GetSettingsFileName())))
            {
                var settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = Localization.Resources.Strings.Overwrite;
                settings.NegativeButtonText = Localization.Resources.Strings.Cancel;
                settings.FirstAuxiliaryButtonText = Localization.Resources.Strings.UseOther;
                settings.DefaultButtonFocus = MessageDialogResult.FirstAuxiliary;

                var result = await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Overwrite, Localization.Resources.Strings.OverwriteSettingsInDestinationFolderMessage, MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, AppearanceManager.MetroDialog);

                switch (result)
                {
                    case MessageDialogResult.Negative:
                        MovingFiles = false;
                        return;
                    case MessageDialogResult.FirstAuxiliary:
                        useFileInOtherLocation = true;
                        break;
                }
            }

            // Use other location
            if (useFileInOtherLocation)
            {
                LocalSettingsManager.Settings_CustomSettingsLocation = Location;

                MovingFiles = false;

                // Restart the application
                ConfigurationManager.Current.ForceRestart = true;
                CloseAction();

                return;
            }

            // Move files...
            try
            {
                await SettingsManager.MoveSettingsAsync(Location);

                LocalSettingsManager.Settings_CustomSettingsLocation = Location;

                // Show the user some awesome animation to indicate we are working on it :)
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                var settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, ex.Message, MessageDialogStyle.Affirmative, settings);
            }

            Location = string.Empty;
            Location = LocalSettingsManager.Settings_CustomSettingsLocation;

            MovingFiles = false;
        }

        public ICommand RestoreDefaultSettingsLocationCommand => new RelayCommand(p => RestoreDefaultSettingsLocationAction());

        private void RestoreDefaultSettingsLocationAction()
        {
            Location = SettingsManager.GetDefaultSettingsLocation();
        }

        public ICommand BrowseImportFileCommand => new RelayCommand(p => BrowseFileAction());

        private void BrowseFileAction()
        {
            using (var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = GlobalStaticConfiguration.ZipFileExtensionFilter
            })
            {
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    ImportFilePath = openFileDialog.FileName;
            }
        }

        public ICommand ImportSettingsCommand => new RelayCommand(p => ImportSettingsAction());

        private async Task ImportSettingsAction()
        {
            var settings = AppearanceManager.MetroDialog;

            settings.AffirmativeButtonText = Localization.Resources.Strings.Continue;
            settings.NegativeButtonText = Localization.Resources.Strings.Cancel;

            settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            if (await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.AreYouSure, Localization.Resources.Strings.SelectedSettingsAreOverwritten + Environment.NewLine + Environment.NewLine + Localization.Resources.Strings.ApplicationWillBeRestartedAfterwards, MessageDialogStyle.AffirmativeAndNegative, settings) != MessageDialogResult.Affirmative)
                return;

            try
            {
                SettingsManager.Import(ImportFilePath);

                // Restart the application
                ConfigurationManager.Current.ForceRestart = true;
                CloseAction();
            }
            catch (Exception ex)
            {
                ImportStatusMessage = string.Format(Localization.Resources.Strings.ClouldNotImportFileSeeErrorMessageXX, ex.Message);
                DisplayImportStatusMessage = true;
            }
        }

        public ICommand ExportSettingsCommand => new RelayCommand(p => ExportSettingsAction());

        private void ExportSettingsAction()
        {
            DisplayExportStatusMessage = false;

            using (var saveFileDialog = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = GlobalStaticConfiguration.ZipFileExtensionFilter,
                FileName = $"{AssemblyManager.Current.Name}_{Localization.Resources.Strings.Settings}_{Localization.Resources.Strings.Backup}#{TimestampHelper.GetTimestamp()}.zip"
            })
            {
                if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                try
                {
                    SettingsManager.Export(saveFileDialog.FileName);

                    ExportStatusMessage = string.Format(Localization.Resources.Strings.FileExportedToXX, saveFileDialog.FileName);
                    DisplayExportStatusMessage = true;
                }
                catch (Exception ex)
                {
                    ExportStatusMessage = string.Format(Localization.Resources.Strings.ClouldNotExportFileSeeErrorMessageXX, ex.Message);
                    DisplayExportStatusMessage = true;
                }
            }
        }

        public ICommand ResetSettingsCommand => new RelayCommand(p => ResetSettingsAction());

        public async Task ResetSettingsAction()
        {
            var settings = AppearanceManager.MetroDialog;

            settings.AffirmativeButtonText = Localization.Resources.Strings.Continue;
            settings.NegativeButtonText = Localization.Resources.Strings.Cancel;

            settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            var message = Localization.Resources.Strings.SelectedSettingsAreReset;

            message += Environment.NewLine + Environment.NewLine + $"* {Localization.Resources.Strings.TheSettingsLocationIsNotAffected}";
            message += Environment.NewLine + $"* {Localization.Resources.Strings.ApplicationWillBeRestartedAfterwards}";

            if (await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.AreYouSure, message, MessageDialogStyle.AffirmativeAndNegative, settings) != MessageDialogResult.Affirmative)
                return;

            SettingsManager.Reset();

            message = Localization.Resources.Strings.SettingsSuccessfullyReset;
            message += Environment.NewLine + Environment.NewLine + Localization.Resources.Strings.TheApplicationWillBeRestarted;

            await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Success, message, MessageDialogStyle.Affirmative, settings);

            // Restart the application
            ConfigurationManager.Current.ForceRestart = true;
            CloseAction();
        }
        #endregion

        #region Methods
        public void SaveAndCheckSettings()
        {
            // Save everything
            if (SettingsManager.Current.SettingsChanged)
                SettingsManager.Save();

            // Check if files exist
            SettingsExists = File.Exists(SettingsManager.GetSettingsFilePath());
        }

        public void SetLocationPathFromDragDrop(string path)
        {
            Location = path;

            OnPropertyChanged(nameof(Location));
        }

        public void SetImportFilePathFromDragDrop(string filePath)
        {
            ImportFilePath = filePath;

            OnPropertyChanged(nameof(ImportFilePath));
        }
        #endregion
    }
}