using NETworkManager.Models.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels
{
    public class PingSettingsViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        private int _attempts;
        public int Attempts
        {
            get { return _attempts; }
            set
            {
                if (value == _attempts)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Ping_Attempts = value;

                _attempts = value;
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
                    SettingsManager.Current.Ping_Timeout = value;

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
                    SettingsManager.Current.Ping_Buffer = value;

                _buffer = value;
                OnPropertyChanged();
            }
        }

        private int _ttl;
        public int TTL
        {
            get { return _ttl; }
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
            get { return _dontFragment; }
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
            get { return _waitTime; }
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
            get { return _exceptionCancelCount; }
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
            get { return _resolveHostnamePreferIPv4; }
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
            get { return _resolveHostnamePreferIPv6; }
            set
            {
                if (value == _resolveHostnamePreferIPv6)
                    return;

                _resolveHostnamePreferIPv6 = value;
                OnPropertyChanged();
            }
        }

        private bool _hideStatistics;
        public bool HideStatistics
        {
            get { return _hideStatistics; }
            set
            {
                if (value == _hideStatistics)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Ping_HideStatistics = value;

                _hideStatistics = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor, load settings
        public PingSettingsViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            Attempts = SettingsManager.Current.Ping_Attempts;
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

            HideStatistics = SettingsManager.Current.Ping_HideStatistics;
        }
        #endregion
    }
}