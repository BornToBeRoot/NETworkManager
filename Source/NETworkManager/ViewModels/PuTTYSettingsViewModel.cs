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

        private string _defaultUsername;
        public string DefaultUsername
        {
            get => _defaultUsername;
            set
            {
                if (value == _defaultUsername)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_DefaultUsername = value;

                _defaultUsername = value;
                OnPropertyChanged();
            }
        }

        private string _defaultAdditionalCommandLine;
        public string DefaultAdditionalCommandLine
        {
            get => _defaultAdditionalCommandLine;
            set
            {
                if (value == _defaultAdditionalCommandLine)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_DefaultAdditionalCommandLine = value;

                _defaultAdditionalCommandLine = value;
                OnPropertyChanged();
            }
        }

        private string _defaultProfile;
        public string DefaultProfile
        {
            get => _defaultProfile;
            set
            {
                if (value == _defaultProfile)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_DefaultProfile = value;

                _defaultProfile = value;
                OnPropertyChanged();
            }
        }

        private string _defaultSerialLine;
        public string DefaultSerialLine
        {
            get => _defaultSerialLine;
            set
            {
                if (value == _defaultSerialLine)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_DefaultSerialLine = value;

                _defaultSerialLine = value;
                OnPropertyChanged();
            }
        }

        private int _defaultSSHPort;
        public int DefaultSSHPort
        {
            get => _defaultSSHPort;
            set
            {
                if (value == _defaultSSHPort)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_DefaultSSHPort = value;

                _defaultSSHPort = value;
                OnPropertyChanged();
            }
        }

        private int _defaultTelnetPort;
        public int DefaultTelnetPort
        {
            get => _defaultTelnetPort;
            set
            {
                if (value == _defaultTelnetPort)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_DefaultTelnetPort = value;

                _defaultTelnetPort = value;
                OnPropertyChanged();
            }
        }

        private int _defaultBaudRate;
        public int DefaultBaudRate
        {
            get => _defaultBaudRate;
            set
            {
                if (value == _defaultBaudRate)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_DefaultBaudRate = value;

                _defaultBaudRate = value;
                OnPropertyChanged();
            }
        }

        private int _defaultRloginPort;
        public int DefaultRloginPort
        {
            get => _defaultRloginPort;
            set
            {
                if (value == _defaultRloginPort)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_DefaultRloginPort = value;

                _defaultRloginPort = value;
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
            DefaultUsername = SettingsManager.Current.PuTTY_DefaultUsername;
            DefaultProfile = SettingsManager.Current.PuTTY_DefaultProfile;
            DefaultAdditionalCommandLine = SettingsManager.Current.PuTTY_DefaultAdditionalCommandLine;
            DefaultSerialLine = SettingsManager.Current.PuTTY_DefaultSerialLine;
            DefaultSSHPort = SettingsManager.Current.PuTTY_DefaultSSHPort;
            DefaultTelnetPort = SettingsManager.Current.PuTTY_DefaultTelnetPort;
            DefaultBaudRate = SettingsManager.Current.PuTTY_DefaultBaudRate;
            DefaultRloginPort = SettingsManager.Current.PuTTY_DefaultRloginPort;
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