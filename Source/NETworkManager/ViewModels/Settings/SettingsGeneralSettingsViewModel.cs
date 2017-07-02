using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsGeneralSettingsViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;

        MetroDialogSettings dialogSettings = new MetroDialogSettings();

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

        private bool _resetApplicationSettings;
        public bool ResetApplicationSettings
        {
            get { return _resetApplicationSettings; }
            set
            {
                if (value == _resetApplicationSettings)
                    return;

                _resetApplicationSettings = value;
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

        private bool _resetNetworkInterfaceConfigTemplates;
        public bool ResetNetworkInterfaceConfigTemplates
        {
            get { return _resetNetworkInterfaceConfigTemplates; }
            set
            {
                if (value == _resetNetworkInterfaceConfigTemplates)
                    return;

                _resetNetworkInterfaceConfigTemplates = value;
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

        private bool _resetWakeOnLANTemplates;
        public bool ResetWakeOnLANTemplates
        {
            get { return _resetWakeOnLANTemplates; }
            set
            {
                if (value == _resetWakeOnLANTemplates)
                    return;

                _resetWakeOnLANTemplates = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, LoadSettings
        public SettingsGeneralSettingsViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            dialogSettings.CustomResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
            };

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            LocationSelectedPath = SettingsManager.SettingsLocationNotPortable;
            IsPortable = SettingsManager.IsPortable;

            ApplicationSettingsExists = File.Exists(SettingsManager.SettingsFilePath);
            NetworkInterfaceConfigTemplatesExists = File.Exists(TemplateManager.NetworkInterfaceConfigTemplatesFilePath);
            WakeOnLANTemplatesExists = File.Exists(TemplateManager.WakeOnLANTemplatesFilePath);
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

        public ICommand ChangeSettingsCommand
        {
            get { return new RelayCommand(p => ChangeSettingsAction()); }
        }

        private async void ChangeSettingsAction()
        {
            MovingFiles = true;

            // Try moving files (permissions, file is in use...)
            try
            {
                await SettingsManager.MoveSettingsAsync(SettingsManager.SettingsLocation, LocationSelectedPath);

                Properties.Settings.Default.Settings_CustomSettingsLocation = LocationSelectedPath;

                // Show the user some awesome animation to indicate we are working on it :)
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Error"] as string, ex.Message, MessageDialogStyle.Affirmative, dialogSettings);
            }

            LocationSelectedPath = string.Empty;
            LocationSelectedPath = Properties.Settings.Default.Settings_CustomSettingsLocation;
            
            MovingFiles = false;
        }

        public ICommand RestoreDefaultSettingsLocationCommand
        {
            get { return new RelayCommand(p => RestoreDefaultSettingsLocationAction()); }
        }

        private void RestoreDefaultSettingsLocationAction()
        {
            LocationSelectedPath = SettingsManager.DefaultSettingsLocation;
        }

        public ICommand ResetSettingsCommand
        {
            get { return new RelayCommand(p => ResetSettingsAction()); }
        }

        public async void ResetSettingsAction()
        {
            MetroDialogSettings settings = dialogSettings;

            settings.AffirmativeButtonText = Application.Current.Resources["String_Button_Continue"] as string;
            settings.NegativeButtonText = Application.Current.Resources["String_Button_Cancel"] as string;

            settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            string message = Application.Current.Resources["String_SelectedSettingsAreReset"] as string;

            if (ResetEverything || ResetApplicationSettings)
                message += Environment.NewLine + Environment.NewLine + Application.Current.Resources["String_ApplicationIsRestartedAfterwards"] as string;

            if (await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_AreYouSure"] as string, message, MessageDialogStyle.AffirmativeAndNegative, settings) != MessageDialogResult.Affirmative)
                return;

            bool forceRestart = false;

            if (ApplicationSettingsExists && (ResetEverything || ResetApplicationSettings))
            {
                SettingsManager.Reset();
                forceRestart = true;
            }

            if (NetworkInterfaceConfigTemplatesExists && (ResetEverything || ResetNetworkInterfaceConfigTemplates))
                TemplateManager.ResetNetworkInterfaceConfigTemplates();

            if (WakeOnLANTemplatesExists && (ResetEverything || ResetWakeOnLANTemplates))
                TemplateManager.ResetWakeOnLANTemplates();

            if (forceRestart)
                CloseAction();
            else
                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Success"] as string, Application.Current.Resources["String_SettingsSuccessfullyReset"] as string, MessageDialogStyle.Affirmative, dialogSettings);
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
                await SettingsManager.MakePortableAsync(isPortable);

                Properties.Settings.Default.Settings_CustomSettingsLocation = string.Empty;
                LocationSelectedPath = SettingsManager.SettingsLocationNotPortable;

                // Show the user some awesome animation to indicate we are working on it :)
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Error"] as string, ex.Message, MessageDialogStyle.Affirmative, dialogSettings);
            }

            MakingPortable = false;
        }
        #endregion
    }
}