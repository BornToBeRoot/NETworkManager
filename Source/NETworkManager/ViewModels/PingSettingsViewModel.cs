using NETworkManager.Models.Settings;

namespace NETworkManager.ViewModels
{
    public class PingSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;
        
        private int _timeout;
        public int Timeout
        {
            get => _timeout;
            set
            {
                if (value == _timeout)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Ping_Timeout = value;

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
                    SettingsManager.Current.Ping_Buffer = value;

                _buffer = value;
                OnPropertyChanged();
            }
        }

        private int _ttl;
        public int TTL
        {
            get => _ttl;
            set
            {
                if (value == _ttl)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Ping_TTL = value;

                _ttl = value;
                OnPropertyChanged();
            }
        }

        private bool _dontFragment;
        public bool DontFragment
        {
            get => _dontFragment;
            set
            {
                if (value == _dontFragment)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Ping_DontFragment = value;

                _dontFragment = value;
                OnPropertyChanged();
            }
        }

        private int _waitTime;
        public int WaitTime
        {
            get => _waitTime;
            set
            {
                if (value == _waitTime)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Ping_WaitTime = value;

                _waitTime = value;
                OnPropertyChanged();
            }
        }

        private int _exceptionCancelCount;
        public int ExceptionCancelCount
        {
            get => _exceptionCancelCount;
            set
            {
                if (value == _exceptionCancelCount)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Ping_ExceptionCancelCount = value;

                _exceptionCancelCount = value;
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
                    SettingsManager.Current.Ping_ResolveHostnamePreferIPv4 = value;

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
        
        private bool _highlightTimeouts;
        public bool HighlightTimeouts
        {
            get => _highlightTimeouts;
            set
            {
                if (value == _highlightTimeouts)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Ping_HighlightTimeouts = value;

                _highlightTimeouts = value;
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
                    SettingsManager.Current.Ping_ShowStatistics = value;

                _showStatistics = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor, load settings
        public PingSettingsViewModel()
        {
            _isLoading = true;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {            
            Timeout = SettingsManager.Current.Ping_Timeout;
            Buffer = SettingsManager.Current.Ping_Buffer;
            TTL = SettingsManager.Current.Ping_TTL;
            DontFragment = SettingsManager.Current.Ping_DontFragment;
            WaitTime = SettingsManager.Current.Ping_WaitTime;
            ExceptionCancelCount = SettingsManager.Current.Ping_ExceptionCancelCount;

            if (SettingsManager.Current.Ping_ResolveHostnamePreferIPv4)
                ResolveHostnamePreferIPv4 = true;
            else
                ResolveHostnamePreferIPv6 = true;

            HighlightTimeouts = SettingsManager.Current.Ping_HighlightTimeouts;
            ShowStatistics = SettingsManager.Current.Ping_ShowStatistics;
        }
        #endregion
    }
}