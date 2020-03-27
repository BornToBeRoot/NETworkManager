using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels
{
    public class PortScannerSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        private int _hostThreads;
        public int HostThreads
        {
            get => _hostThreads;
            set
            {
                if (value == _hostThreads)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_HostThreads = value;

                _hostThreads = value;
                OnPropertyChanged();
            }
        }

        private int _portThreads;
        public int PortThreads
        {
            get => _portThreads;
            set
            {
                if (value == _portThreads)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_PortThreads = value;

                _portThreads = value;
                OnPropertyChanged();
            }
        }

        private bool _showClosed;
        public bool ShowClosed
        {
            get => _showClosed;
            set
            {
                if (value == _showClosed)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_ShowClosed = value;

                _showClosed = value;
                OnPropertyChanged();
            }
        }

        private int _timeout;
        public int Timeout
        {
            get => _timeout;
            set
            {
                if (value == _timeout)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_Timeout = value;

                _timeout = value;
                OnPropertyChanged();
            }
        }

        private bool _resolveHostname;
        public bool ResolveHostname
        {
            get => _resolveHostname;
            set
            {
                if (value == _resolveHostname)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_ResolveHostname = value;

                _resolveHostname = value;
                OnPropertyChanged();
            }
        }

        private bool _showStatistics;
        public bool ShowStatistics
        {
            get => _showStatistics;
            set
            {
                if (value == _showStatistics)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_ShowStatistics = value;

                _showStatistics = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public PortScannerSettingsViewModel()
        {
            _isLoading = true;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            HostThreads = SettingsManager.Current.PortScanner_HostThreads;
            PortThreads = SettingsManager.Current.PortScanner_PortThreads;
            ShowClosed = SettingsManager.Current.PortScanner_ShowClosed;
            Timeout = SettingsManager.Current.PortScanner_Timeout;
            ResolveHostname = SettingsManager.Current.PortScanner_ResolveHostname;
            ShowStatistics = SettingsManager.Current.PortScanner_ShowStatistics;
        }
        #endregion
    }
}