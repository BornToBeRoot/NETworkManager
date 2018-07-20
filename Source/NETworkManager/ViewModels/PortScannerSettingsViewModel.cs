using NETworkManager.Models.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels
{
    public class PortScannerSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        private int _threads;
        public int Threads
        {
            get => _threads;
            set
            {
                if (value == _threads)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_Threads = value;

                _threads = value;
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

        private bool _resolveHostnamePreferIPv4;
        public bool ResolveHostnamePreferIPv4
        {
            get => _resolveHostnamePreferIPv4;
            set
            {
                if (value == _resolveHostnamePreferIPv4)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_ResolveHostnamePreferIPv4 = value;

                _resolveHostnamePreferIPv4 = value;
                OnPropertyChanged();
            }
        }

        private bool _resolveHostnamePreferIPv6;
        public bool ResolveHostnamePreferIPv6
        {
            get => _resolveHostnamePreferIPv6;
            set
            {
                if (value == _resolveHostnamePreferIPv6)
                    return;

                _resolveHostnamePreferIPv6 = value;
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
            Threads = SettingsManager.Current.PortScanner_Threads;
            ShowClosed = SettingsManager.Current.PortScanner_ShowClosed;
            Timeout = SettingsManager.Current.PortScanner_Timeout;

            if (SettingsManager.Current.PortScanner_ResolveHostnamePreferIPv4)
                ResolveHostnamePreferIPv4 = true;
            else
                ResolveHostnamePreferIPv6 = true;

            ShowStatistics = SettingsManager.Current.PortScanner_ShowStatistics;
        }
        #endregion
    }
}