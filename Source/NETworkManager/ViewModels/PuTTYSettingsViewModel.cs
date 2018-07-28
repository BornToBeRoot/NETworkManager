using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class PuTTYSettingsViewModel : ViewModelBase
    {
        #region Variables
        private const string ApplicationFileExtensionFilter = "Application (*.exe)|*.exe";
        private readonly IDialogCoordinator _dialogCoordinator;
        
        private readonly bool _isLoading;

        private string _puTTYLocation;
        public string PuTTYLocation
        {
            get => _puTTYLocation;
            set
            {
                if (value == _puTTYLocation)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_PuTTYLocation = value;

                // Path to putty is configured....
                IsPuTTYConfigured = !string.IsNullOrEmpty(value);

                _puTTYLocation = value;                               
                OnPropertyChanged();
            }
        }

        private bool _isPuTTYConfigured;
        public bool IsPuTTYConfigured
        {
            get => _isPuTTYConfigured;
            set
            {
                if (value == _isPuTTYConfigured)
                    return;

                _isPuTTYConfigured = value;
                OnPropertyChanged();
            }
        }

        private string _puTTYProfile;
        public string PuTTYProfile
        {
            get => _puTTYProfile;
            set
            {
                if (value == _puTTYProfile)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_Profile = value;

                _puTTYProfile = value;
                OnPropertyChanged();
            }
        }

        private string _serialLine;
        public string SerialLine
        {
            get => _serialLine;
            set
            {
                if (value == _serialLine)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_SerialLine = value;

                _serialLine = value;
                OnPropertyChanged();
            }
        }

        private int _sshPort;
        public int SSHPort
        {
            get => _sshPort;
            set
            {
                if (value == _sshPort)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_SSHPort = value;

                _sshPort = value;
                OnPropertyChanged();
            }
        }

        private int _telnetPort;
        public int TelnetPort
        {
            get => _telnetPort;
            set
            {
                if (value == _telnetPort)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_TelnetPort = value;

                _telnetPort = value;
                OnPropertyChanged();
            }
        }

        private int _baudRate;
        public int BaudRate
        {
            get => _baudRate;
            set
            {
                if (value == _baudRate)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_BaudRate = value;

                _baudRate = value;
                OnPropertyChanged();
            }
        }

        private int _rloginPort;
        public int RloginPort
        {
            get => _rloginPort;
            set
            {
                if (value == _rloginPort)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_RloginPort = value;

                _rloginPort = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor, load settings
        public PuTTYSettingsViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            PuTTYLocation = SettingsManager.Current.PuTTY_PuTTYLocation;
            IsPuTTYConfigured = File.Exists(PuTTYLocation);
            SerialLine = SettingsManager.Current.PuTTY_SerialLine;
            PuTTYProfile = SettingsManager.Current.PuTTY_Profile;
            SSHPort = SettingsManager.Current.PuTTY_SSHPort;
            TelnetPort = SettingsManager.Current.PuTTY_TelnetPort;
            BaudRate = SettingsManager.Current.PuTTY_BaudRate;
            RloginPort = SettingsManager.Current.PuTTY_RloginPort;
        }
        #endregion

        #region ICommands & Actions
        public ICommand BrowseFileCommand
        {
            get { return new RelayCommand(p => BrowseFileAction()); }
        }

        private void BrowseFileAction()
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = ApplicationFileExtensionFilter
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                PuTTYLocation = openFileDialog.FileName;
        }

        public ICommand ConfigurePuTTYCommand
        {
            get { return new RelayCommand(p => ConfigurePuTTYAction()); }
        }

        private void ConfigurePuTTYAction()
        {
            ConfigurePuTTY();
        }
        #endregion

        #region Methods
        private async void ConfigurePuTTY()
        {
            try
            {
                Process.Start(SettingsManager.Current.PuTTY_PuTTYLocation);
            }
            catch (Exception ex)
            {
                var settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = LocalizationManager.GetStringByKey("String_Button_OK");

                await _dialogCoordinator.ShowMessageAsync(this, Resources.Localization.Strings.Error, ex.Message, MessageDialogStyle.Affirmative, settings);
            }
        }

        public void SetFilePathFromDragDrop(string filePath)
        {
            PuTTYLocation = filePath;

            OnPropertyChanged(nameof(PuTTYLocation));
        }
        #endregion
    }
}