using Lextm.SharpSnmpLib.Messaging;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsApplicationHTTPHeadersViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;
        
        private int _timeout;
        public int Timeout
        {
            get { return _timeout; }
            set
            {
                if (value == _timeout)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.HTTPHeaders_Timeout = value;

                _timeout = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor, load settings
        public SettingsApplicationHTTPHeadersViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            Timeout = SettingsManager.Current.HTTPHeaders_Timeout;
        }
        #endregion
    }
}