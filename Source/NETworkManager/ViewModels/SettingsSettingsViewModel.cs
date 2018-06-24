using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;
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
        private IDialogCoordinator dialogCoordinator;

        private bool _isLoading = true;

        public Action CloseAction { get; set; }

        private string _locationSelectedPath;
        public string LocationSelectedPath
        {
            get { return _locationSelectedPath; }
            set
            {
                if (value == _locationSelectedPath)
                    return;

                _locationSelectedPath = value;
                OnPropertyChanged();
            }
        }

        private bool _movingFiles;
        public bool MovingFiles
        {
            get { return _movingFiles; }
            set
            {
                if (value == _movingFiles)
                    return;

                _movingFiles = value;
                OnPropertyChanged();
            }
        }
              
        private bool _isPortable;
        public bool IsPortable
        {
            get { return _isPortable; }
            set
            {
                if (value == _isPortable)
                    return;

                if (!_isLoading)
                    MakePortable(value);

                _isPortable = value;
                OnPropertyChanged();
            }
        }

        private bool _makingPortable;
        public bool MakingPortable
        {
            get { return _makingPortable; }
            set
            {
                if (value == _makingPortable)
                    return;

                _makingPortable = value;
                OnPropertyChanged();
            }
        }

        private bool _resetEverything;
        public bool ResetEverything
        {
            get { return _resetEverything; }
            set
            {
                if (value == _resetEverything)
                    return;

                _resetEverything = value;
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

        private bool _resetSettings;
        public bool ResetSettings
        {
            get { return _resetSettings; }
            set
            {
                if (value == _resetSettings)
                    return;

                _resetSettings = value;
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

        private bool _resetProfiles;
        public bool ResetProfiles
        {
            get { return _resetProfiles; }
            set
            {
                if (value == _resetProfiles)
                    return;

                _resetProfiles = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, LoadSettings
        public SettingsSettingsViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            LocationSelectedPath = SettingsManager.GetSettingsLocationNotPortable();
            IsPortable = SettingsManager.GetIsPortable();
        }
        #endregion

        #region ICommands & Actions
        public ICommand BrowseFolderCommand
        {
            get { return new RelayCommand(p => BrowseFolderAction()); }
        }

        private void BrowseFolderAction()
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (Directory.Exists(LocationSelectedPath))
                dialog.SelectedPath = LocationSelectedPath;

            System.Windows.Forms.DialogResult dialogResult = dialog.ShowDialog();

            if (dialogResult == System.Windows.Forms.DialogResult.OK)
                LocationSelectedPath = dialog.SelectedPath;
        }

        public ICommand OpenLocationCommand
        {
            get { return new RelayCommand(p => OpenLocationAction()); }
        }

        private void OpenLocationAction()
        {
            Process.Start("explorer.exe", SettingsManager.GetSettingsLocation());
        }

        public ICommand ChangeSettingsCommand
        {
            get { return new RelayCommand(p => ChangeSettingsAction()); }
        }

        // Check if a file(name) is a settings file
        private bool FilesContainsSettingsFiles(string[] files)
        {
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);

                if (SettingsManager.GetSettingsFileName() == fileName)
                    return true;

                if (ProfileManager.ProfilesFileName == fileName)
                    return true;
            }

            return false;
        }

        private async void ChangeSettingsAction()
        {
            MovingFiles = true;
            bool overwrite = false;
            bool forceRestart = false;

            string[] filesTargedLocation = Directory.GetFiles(LocationSelectedPath);

            // Check if there are any settings files in the folder...
            if (FilesContainsSettingsFiles(filesTargedLocation))
            {
                MetroDialogSettings settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = LocalizationManager.GetStringByKey("String_Button_Overwrite");
                settings.NegativeButtonText = LocalizationManager.GetStringByKey("String_Button_Cancel");
                settings.FirstAuxiliaryButtonText = LocalizationManager.GetStringByKey("String_Button_MoveAndRestart");
                settings.DefaultButtonFocus = MessageDialogResult.FirstAuxiliary;

                MessageDialogResult result = await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_Overwrite"),LocalizationManager.GetStringByKey("String_OverwriteSettingsInTheDestinationFolder"), MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, AppearanceManager.MetroDialog);

                if (result == MessageDialogResult.Negative)
                {
                    MovingFiles = false;
                    return;
                }
                else if (result == MessageDialogResult.Affirmative)
                {
                    overwrite = true;
                }
                else if (result == MessageDialogResult.FirstAuxiliary)
                {
                    forceRestart = true;
                }
            }

            // Try moving files (permissions, file is in use...)
            try
            {
                await SettingsManager.MoveSettingsAsync(SettingsManager.GetSettingsLocation(), LocationSelectedPath, overwrite, filesTargedLocation);

                Properties.Settings.Default.Settings_CustomSettingsLocation = LocationSelectedPath;

                // Show the user some awesome animation to indicate we are working on it :)
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                MetroDialogSettings settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = LocalizationManager.GetStringByKey("String_Button_OK");

               await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_Error") as string, ex.Message, MessageDialogStyle.Affirmative, settings);
            }

            LocationSelectedPath = string.Empty;
            LocationSelectedPath = Properties.Settings.Default.Settings_CustomSettingsLocation;

            if (forceRestart)
            {
                SettingsManager.ForceRestart = true;
                CloseAction();
            }

            MovingFiles = false;
        }

        public ICommand RestoreDefaultSettingsLocationCommand
        {
            get { return new RelayCommand(p => RestoreDefaultSettingsLocationAction()); }
        }

        private void RestoreDefaultSettingsLocationAction()
        {
            LocationSelectedPath = SettingsManager.GetDefaultSettingsLocation();
        }

        public ICommand ResetSettingsCommand
        {
            get { return new RelayCommand(p => ResetSettingsAction()); }
        }

        public async void ResetSettingsAction()
        {
            MetroDialogSettings settings = AppearanceManager.MetroDialog;

            settings.AffirmativeButtonText = LocalizationManager.GetStringByKey("String_Button_Continue");
            settings.NegativeButtonText = LocalizationManager.GetStringByKey("String_Button_Cancel");

            settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            string message = LocalizationManager.GetStringByKey("String_SelectedSettingsAreReset");

            if (ResetEverything || ResetSettings)
            {
                message += Environment.NewLine + Environment.NewLine + string.Format("* {0}", LocalizationManager.GetStringByKey("String_TheSettingsLocationIsNotAffected"));
                message += Environment.NewLine + string.Format("* {0}", LocalizationManager.GetStringByKey("String_ApplicationIsRestartedAfterwards"));
            }

            if (await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_AreYouSure"), message, MessageDialogStyle.AffirmativeAndNegative, settings) != MessageDialogResult.Affirmative)
                return;

            bool forceRestart = false;

            if (SettingsExists && (ResetEverything || ResetSettings))
            {
                SettingsManager.Reset();
                forceRestart = true;
            }

            if (ProfilesExists && (ResetEverything || ResetProfiles))
                ProfileManager.Reset();

            // Restart after reset or show a completed message
            if (forceRestart)
            {
                CloseAction();
            }
            else
            {
                settings.AffirmativeButtonText = LocalizationManager.GetStringByKey("String_Button_OK");

                await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_Success"), LocalizationManager.GetStringByKey("String_SettingsSuccessfullyReset"), MessageDialogStyle.Affirmative, settings);
            }
        }
        #endregion

        #region Methods
        private async void MakePortable(bool isPortable)
        {
            MakingPortable = true;

            // Save settings before moving them
            if (SettingsManager.Current.SettingsChanged)
                SettingsManager.Save();

            // Try moving files (permissions, file is in use...)
            try
            {
                await SettingsManager.MakePortableAsync(isPortable, true);

                Properties.Settings.Default.Settings_CustomSettingsLocation = string.Empty;
                LocationSelectedPath = SettingsManager.GetSettingsLocationNotPortable();

                // Show the user some awesome animation to indicate we are working on it :)
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_Error"), ex.Message, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
            }

            MakingPortable = false;
        }

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

        public void SetLocationPathFromDragDrop(string path)
        {
            LocationSelectedPath = path;

            OnPropertyChanged(nameof(LocationSelectedPath));
        }
        #endregion
    }
}