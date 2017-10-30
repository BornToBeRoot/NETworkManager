using Heijden.DNS;
using NETworkManager.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsApplicationRemoteDesktopViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

       
        #endregion

        #region Constructor, load settings
        public SettingsApplicationRemoteDesktopViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            
        }
        #endregion
    }
}