using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System.Diagnostics;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class PuTTYSettingsViewModel : ViewModelBase
    {
        #region Variables
        private const string ApplicationFileExtensionFilter = "Application (*.exe)|*.exe";

        private bool _isLoading = true;

        private string _puTTYLocation;
        public string PuTTYLocation
        {
            get { return _puTTYLocation; }
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
            get { return _isPuTTYConfigured; }
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
            get { return _puTTYProfile; }
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
            get { return _serialLine; }
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
            get { return _sshPort; }
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
            get { return _telnetPort; }
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
            get { return _baudRate; }
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
            get { return _rloginPort; }
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
        public PuTTYSettingsViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            PuTTYLocation = SettingsManager.Current.PuTTY_PuTTYLocation;
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
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()
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
            Process.Start(SettingsManager.Current.PuTTY_PuTTYLocation);
        }
        #endregion
    }
}