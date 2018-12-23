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
                    SettingsManager.Current.PuTTY_DefaultSerialLine = value;

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
                    SettingsManager.Current.PuTTY_DefaultSSHPort = value;

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
                    SettingsManager.Current.PuTTY_DefaultTelnetPort = value;

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
                    SettingsManager.Current.PuTTY_DefaultBaudRate = value;

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
                    SettingsManager.Current.PuTTY_DefaultRloginPort = value;

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
            IsConfigured = File.Exists(ApplicationFilePath);
            DefaultUsername = SettingsManager.Current.PuTTY_DefaultUsername;
            PuTTYProfile = SettingsManager.Current.PuTTY_Profile;
            DefaultAdditionalCommandLine = SettingsManager.Current.PuTTY_DefaultAdditionalCommandLine;
            SerialLine = SettingsManager.Current.PuTTY_DefaultSerialLine;
            SSHPort = SettingsManager.Current.PuTTY_DefaultSSHPort;
            TelnetPort = SettingsManager.Current.PuTTY_DefaultTelnetPort;
            BaudRate = SettingsManager.Current.PuTTY_DefaultBaudRate;
            RloginPort = SettingsManager.Current.PuTTY_DefaultRloginPort;
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