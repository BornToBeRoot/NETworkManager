using NETworkManager.Models.Settings;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsGeneralWindowViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;        

        private bool _minimizeInsteadOfTerminating;
        public bool MinimizeInsteadOfTerminating
        {
            get { return _minimizeInsteadOfTerminating; }
            set
            {
                if (value == _minimizeInsteadOfTerminating)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Window_MinimizeInsteadOfTerminating = value;

                _minimizeInsteadOfTerminating = value;
                OnPropertyChanged();
            }
        }

        private bool _minimizeToTrayInsteadOfTaskbar;
        public bool MinimizeToTrayInsteadOfTaskbar
        {
            get { return _minimizeToTrayInsteadOfTaskbar; }
            set
            {
                if (value == _minimizeToTrayInsteadOfTaskbar)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Window_MinimizeToTrayInsteadOfTaskbar = value;

                _minimizeToTrayInsteadOfTaskbar = value;
                OnPropertyChanged();
            }
        }

        private bool _confirmClose;
        public bool ConfirmClose
        {
            get { return _confirmClose; }
            set
            {
                if (value == _confirmClose)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Window_ConfirmClose = value;

                OnPropertyChanged();
                _confirmClose = value;
            }
        }

        private bool _alwaysShowIconInTray;
        public bool AlwaysShowIconInTray
        {
            get { return _alwaysShowIconInTray; }
            set
            {
                if (value == _alwaysShowIconInTray)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.TrayIcon_AlwaysShowIcon = value;

                _alwaysShowIconInTray = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, LoadSettings
        public SettingsGeneralWindowViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {           
            AlwaysShowIconInTray = SettingsManager.Current.TrayIcon_AlwaysShowIcon;
            MinimizeInsteadOfTerminating = SettingsManager.Current.Window_MinimizeInsteadOfTerminating;
            ConfirmClose = SettingsManager.Current.Window_ConfirmClose;
            MinimizeToTrayInsteadOfTaskbar = SettingsManager.Current.Window_MinimizeToTrayInsteadOfTaskbar;
        }
        #endregion
    }
}