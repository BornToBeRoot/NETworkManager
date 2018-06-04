using NETworkManager.Models.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels
{
    public class TracerouteSettingsViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        private int _maximumHops;
        public int MaximumHops
        {
            get { return _maximumHops; }
            set
            {
                if (value == _maximumHops)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Traceroute_MaximumHops = value;

                _maximumHops = value;
                OnPropertyChanged();
            }
        }

        private int _timeout;
        public int Timeout
        {
            get { return _timeout; }
            set
            {
                if (value == _timeout)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Traceroute_Timeout = value;

                _timeout = value;
                OnPropertyChanged();
            }
        }

        private int _buffer;
        public int Buffer
        {
            get { return _buffer; }
            set
            {
                if (value == _buffer)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Traceroute_Buffer = value;

                _buffer = value;
                OnPropertyChanged();
            }
        }

        private bool _resolveHostname;
        public bool ResolveHostname
        {
            get { return _resolveHostname; }
            set
            {
                if (value == _resolveHostname)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Traceroute_ResolveHostname = value;

                _resolveHostname = value;
                OnPropertyChanged();
            }
        }

        private bool _resolveHostnamePreferIPv4;
        public bool ResolveHostnamePreferIPv4
        {
            get { return _resolveHostnamePreferIPv4; }
            set
            {
                if (value == _resolveHostnamePreferIPv4)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Traceroute_ResolveHostnamePreferIPv4 = value;

                _resolveHostnamePreferIPv4 = value;
                OnPropertyChanged();
            }
        }

        private bool _resolveHostnamePreferIPv6;
        public bool ResolveHostnamePreferIPv6
        {
            get { return _resolveHostnamePreferIPv6; }
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
            get { return _showStatistics; }
            set
            {
                if (value == _showStatistics)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Traceroute_ShowStatistics = value;

                _showStatistics = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public TracerouteSettingsViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            MaximumHops = SettingsManager.Current.Traceroute_MaximumHops;
            Timeout = SettingsManager.Current.Traceroute_Timeout;
            Buffer = SettingsManager.Current.Traceroute_Buffer;
            ResolveHostname = SettingsManager.Current.Traceroute_ResolveHostname;

            if (SettingsManager.Current.Traceroute_ResolveHostnamePreferIPv4)
                ResolveHostnamePreferIPv4 = true;
            else
                ResolveHostnamePreferIPv6 = true;

            ShowStatistics = SettingsManager.Current.Traceroute_ShowStatistics;
        }
        #endregion
    }
}