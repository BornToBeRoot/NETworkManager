using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using NETworkManager.Models.PuTTY;

namespace NETworkManager.ViewModels
{
    public class PuTTYSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        private readonly bool _isLoading;

        private string _applicationFilePath;
        public string ApplicationFilePath
        {
            get => _applicationFilePath;
            set
            {
                if (value == _applicationFilePath)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_ApplicationFilePath = value;

                IsConfigured = !string.IsNullOrEmpty(value);

                _applicationFilePath = value;
                OnPropertyChanged();
            }
        }

        private bool _isConfigured;
        public bool IsConfigured
        {
            get => _isConfigured;
            set
            {
                if (value == _isConfigured)
                    return;

                _isConfigured = value;
                OnPropertyChanged();
            }
        }

        private bool _useSSH;
        public bool UseSSH
        {
            get => _useSSH;
            set
            {
                if (value == _useSSH)
                    return;

                if (value)
                    SettingsManager.Current.PuTTY_DefaultConnectionMode = PuTTY.ConnectionMode.SSH;

                _useSSH = value;
                OnPropertyChanged();
            }
        }

        private bool _useTelnet;
        public bool UseTelnet
        {
            get => _useTelnet;
            set
            {
                if (value == _useTelnet)
                    return;

                if (value)
                    SettingsManager.Current.PuTTY_DefaultConnectionMode = PuTTY.ConnectionMode.Telnet;

                _useTelnet = value;
                OnPropertyChanged();
            }
        }

        private bool _useSerial;
        public bool UseSerial
        {
            get => _useSerial;
            set
            {
                if (value == _useSerial)
                    return;

                if (value)
                    SettingsManager.Current.PuTTY_DefaultConnectionMode = PuTTY.ConnectionMode.Serial;

                _useSerial = value;
                OnPropertyChanged();
            }
        }

        private bool _useRlogin;
        public bool UseRlogin
        {
            get => _useRlogin;
            set
            {
                if (value == _useRlogin)
                    return;

                if (value)
                    SettingsManager.Current.PuTTY_DefaultConnectionMode = PuTTY.ConnectionMode.Rlogin;

                _useRlogin = value;
                OnPropertyChanged();
            }
        }

        private bool _useRAW;
        public bool UseRAW
        {
            get => _useRAW;
            set
            {
                if (value == _useRAW)
                    return;

                if (value)
                    SettingsManager.Current.PuTTY_DefaultConnectionMode = PuTTY.ConnectionMode.RAW;

                _useRAW = value;
                OnPropertyChanged();
            }
        }

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (value == _username)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_Username = value;

                _username = value;
                OnPropertyChanged();
            }
        }

        private string _additionalCommandLine;
        public string AdditionalCommandLine
        {
            get => _additionalCommandLine;
            set
            {
                if (value == _additionalCommandLine)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_AdditionalCommandLine = value;

                _additionalCommandLine = value;
                OnPropertyChanged();
            }
        }

        private string _profile;
        public string Profile
        {
            get => _profile;
            set
            {
                if (value == _profile)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_Profile = value;

                _profile = value;
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
            ApplicationFilePath = SettingsManager.Current.PuTTY_ApplicationFilePath;

            switch (SettingsManager.Current.PuTTY_DefaultConnectionMode)
            {
                case PuTTY.ConnectionMode.SSH:
                    UseSSH = true;
                    break;
                case PuTTY.ConnectionMode.Telnet:
                    UseTelnet = true;
                    break;
                case PuTTY.ConnectionMode.Serial:
                    UseSerial = true;
                    break;
                case PuTTY.ConnectionMode.Rlogin:
                    UseRlogin = true;
                    break;
                case PuTTY.ConnectionMode.RAW:
                    UseRAW = true;
                    break;
            }

            IsConfigured = File.Exists(ApplicationFilePath);
            Username = SettingsManager.Current.PuTTY_Username;
            Profile = SettingsManager.Current.PuTTY_Profile;
            AdditionalCommandLine = SettingsManager.Current.PuTTY_AdditionalCommandLine;
            SerialLine = SettingsManager.Current.PuTTY_SerialLine;
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
                Filter = GlobalStaticConfiguration.ApplicationFileExtensionFilter
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                ApplicationFilePath = openFileDialog.FileName;
        }

        public ICommand ConfigureCommand
        {
            get { return new RelayCommand(p => ConfigureAction()); }
        }

        private void ConfigureAction()
        {
            Configure();
        }
        #endregion

        #region Methods
        private async void Configure()
        {
            try
            {
                Process.Start(SettingsManager.Current.PuTTY_ApplicationFilePath);
            }
            catch (Exception ex)
            {
                var settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = Resources.Localization.Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Resources.Localization.Strings.Error, ex.Message, MessageDialogStyle.Affirmative, settings);
            }
        }

        public void SetFilePathFromDragDrop(string filePath)
        {
            ApplicationFilePath = filePath;

            OnPropertyChanged(nameof(ApplicationFilePath));
        }
        #endregion
    }
}