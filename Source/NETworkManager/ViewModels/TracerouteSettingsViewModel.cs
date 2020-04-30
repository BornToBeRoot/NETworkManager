using NETworkManager.Settings;

namespace NETworkManager.ViewModels
{
    public class TracerouteSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        private int _maximumHops;
        public int MaximumHops
        {
            get => _maximumHops;
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
            get => _timeout;
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
            get => _buffer;
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
            get => _resolveHostname;
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
            get => _resolveHostnamePreferIPv4;
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
            get => _resolveHostnamePreferIPv6;
            set
            {
                if (value == _resolveHostnamePreferIPv6)
                    return;

                _resolveHostnamePreferIPv6 = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public TracerouteSettingsViewModel()
        {
            _isLoading = true;

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

        }
        #endregion
    }
}