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
#pragma warning disable CS0414 // The field 'SettingsApplicationRemoteDesktopViewModel._isLoading' is assigned but its value is never used
        private bool _isLoading = true;
#pragma warning restore CS0414 // The field 'SettingsApplicationRemoteDesktopViewModel._isLoading' is assigned but its value is never used
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