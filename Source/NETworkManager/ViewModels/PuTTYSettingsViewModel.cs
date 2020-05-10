using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using NETworkManager.Models.PuTTY;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.ViewModels
{
    public class PuTTYSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        public bool IsPortable => ConfigurationManager.Current.IsPortable;

        public string PortableLogPath => Settings.Application.PuTTY.PortableLogPath;

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
                    SettingsManager.Current.PuTTY_DefaultConnectionMode = ConnectionMode.SSH;

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
                    SettingsManager.Current.PuTTY_DefaultConnectionMode = ConnectionMode.Telnet;

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
                    SettingsManager.Current.PuTTY_DefaultConnectionMode = ConnectionMode.Serial;

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
                    SettingsManager.Current.PuTTY_DefaultConnectionMode = ConnectionMode.Rlogin;

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
                    SettingsManager.Current.PuTTY_DefaultConnectionMode = ConnectionMode.RAW;

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

        private string _privateKeyFile;
        public string PrivateKeyFile
        {
            get => _privateKeyFile;
            set
            {
                if (value == _privateKeyFile)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_PrivateKeyFile = value;

                _privateKeyFile = value;
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

        private bool _enableLog;
        public bool EnableLog
        {
            get => _enableLog;
            set
            {
                if (value == _enableLog)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_EnableSessionLog = value;

                _enableLog = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<LogMode> LogModes => Enum.GetValues(typeof(LogMode)).Cast<LogMode>();

        private LogMode _logMode;
        public LogMode LogMode
        {
            get => _logMode;
            set
            {
                if (Equals(value, _logMode))
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_LogMode = value;

                _logMode = value;
                OnPropertyChanged();
            }
        }

        private string _logPath;
        public string LogPath
        {
            get => _logPath;
            set
            {
                if (value == _logPath)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_LogPath = value;

                _logPath = value;
                OnPropertyChanged();
            }
        }

        private string _logFileName;
        public string LogFileName
        {
            get => _logFileName;
            set
            {
                if (value == _logFileName)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_LogFileName = value;

                _logFileName = value;
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
                case ConnectionMode.SSH:
                    UseSSH = true;
                    break;
                case ConnectionMode.Telnet:
                    UseTelnet = true;
                    break;
                case ConnectionMode.Serial:
                    UseSerial = true;
                    break;
                case ConnectionMode.Rlogin:
                    UseRlogin = true;
                    break;
                case ConnectionMode.RAW:
                    UseRAW = true;
                    break;
            }

            IsConfigured = File.Exists(ApplicationFilePath);
            Username = SettingsManager.Current.PuTTY_Username;
            PrivateKeyFile = SettingsManager.Current.PuTTY_PrivateKeyFile;
            Profile = SettingsManager.Current.PuTTY_Profile;
            EnableLog = SettingsManager.Current.PuTTY_EnableSessionLog;
            LogMode = LogModes.FirstOrDefault(x => x == SettingsManager.Current.PuTTY_LogMode);
            LogPath = SettingsManager.Current.PuTTY_LogPath;
            LogFileName = SettingsManager.Current.PuTTY_LogFileName;
            AdditionalCommandLine = SettingsManager.Current.PuTTY_AdditionalCommandLine;
            SerialLine = SettingsManager.Current.PuTTY_SerialLine;
            SSHPort = SettingsManager.Current.PuTTY_SSHPort;
            TelnetPort = SettingsManager.Current.PuTTY_TelnetPort;
            BaudRate = SettingsManager.Current.PuTTY_BaudRate;
            RloginPort = SettingsManager.Current.PuTTY_RloginPort;
        }
        #endregion

        #region ICommands & Actions
        public ICommand ApplicationBrowseFileCommand => new RelayCommand(p => ApplicationBrowseFileAction());

        private void ApplicationBrowseFileAction()
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = GlobalStaticConfiguration.ApplicationFileExtensionFilter
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                ApplicationFilePath = openFileDialog.FileName;
        }

        public ICommand ConfigureCommand => new RelayCommand(p => ConfigureAction());

        private void ConfigureAction()
        {
            Configure();
        }

        public ICommand PrivateKeyFileBrowseFileCommand => new RelayCommand(p => PrivateKeyFileBrowseFileAction());

        private void PrivateKeyFileBrowseFileAction()
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = GlobalStaticConfiguration.PuTTYPrivateKeyFileExtensionFilter
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                PrivateKeyFile = openFileDialog.FileName;
        }

        public ICommand LogPathBrowseFolderCommand => new RelayCommand(p => LogPathBrowseFolderAction());

        private void LogPathBrowseFolderAction()
        {
            var openFolderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };

            if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                LogPath = openFolderDialog.SelectedPath;
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

                settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, ex.Message, MessageDialogStyle.Affirmative, settings);
            }
        }

        /// <summary>
        /// Method to set the <see cref="ApplicationFilePath"/> from drag and drop.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        public void SetApplicationFilePathFromDragDrop(string filePath)
        {
            ApplicationFilePath = filePath;

            OnPropertyChanged(nameof(ApplicationFilePath));
        }

        /// <summary>
        /// Method to set the <see cref="PrivateKeyFile"/> drag drop.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        public void SetPrivateKeyFilePathFromDragDrop(string filePath)
        {
            PrivateKeyFile = filePath;

            OnPropertyChanged(nameof(PrivateKeyFile));
        }

        /// <summary>
        /// Method to set the <see cref="LogPath"/> from drag and drop.
        /// </summary>
        /// <param name="folderPath">Path to the folder.</param>
        public void SetLogPathFolderPathFromDragDrop(string folderPath)
        {
            LogPath = folderPath;

            OnPropertyChanged(nameof(LogPath));
        }
        #endregion
    }
}