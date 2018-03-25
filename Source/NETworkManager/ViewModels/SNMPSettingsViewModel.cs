using Lextm.SharpSnmpLib.Messaging;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.ViewModels
{
    public class SNMPSettingsViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;
                
        public List<WalkMode> WalkModes { get; set; }

        private WalkMode _walkMode;
        public WalkMode WalkMode
        {
            get { return _walkMode; }
            set
            {
                if (value == _walkMode)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.SNMP_WalkMode = value;

                _walkMode = value;
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
                    SettingsManager.Current.SNMP_Timeout = value;

                _timeout = value;
                OnPropertyChanged();
            }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set
            {
                if (value == _port)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.SNMP_Port = value;

                _port = value;
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
                    SettingsManager.Current.SNMP_ResolveHostnamePreferIPv4 = value;

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
        #endregion

        #region Contructor, load settings
        public SNMPSettingsViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            WalkModes = System.Enum.GetValues(typeof(WalkMode)).Cast<WalkMode>().OrderBy(x => x.ToString()).ToList();
            WalkMode = WalkModes.First(x => x == SettingsManager.Current.SNMP_WalkMode);
            Timeout = SettingsManager.Current.SNMP_Timeout;
            Port = SettingsManager.Current.SNMP_Port;

            if (SettingsManager.Current.SNMP_ResolveHostnamePreferIPv4)
                ResolveHostnamePreferIPv4 = true;
            else
                ResolveHostnamePreferIPv6 = true;
        }
        #endregion
    }
}