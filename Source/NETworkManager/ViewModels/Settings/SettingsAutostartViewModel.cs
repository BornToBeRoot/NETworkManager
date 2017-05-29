using NETworkManager.Models.Settings;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsAutostartViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        private bool _startWithWindows;
        public bool StartWithWindows
        {
            get { return _startWithWindows; }
            set
            {
                if (value == _startWithWindows)
                    return;

                if (!_isLoading)
                {
                    if (value)
                        AutostartManager.Enable();
                    else
                        AutostartManager.Disable();
                }

                _startWithWindows = value;
                OnPropertyChanged();
            }
        }

        private bool _startMinimizedInTray;
        public bool StartMinimizedInTray
        {
            get { return _startMinimizedInTray; }
            set
            {
                if (value == _startMinimizedInTray)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Autostart_StartMinimizedInTray = value;

                _startMinimizedInTray = value;
                OnPropertyChanged();
            }
        }
        #endregion       

        #region Constructor, LoadSettings
        public SettingsAutostartViewModel()
        {

        }

        private void LoadSettings()
        {
            StartWithWindows = AutostartManager.IsEnabled;
            StartMinimizedInTray = SettingsManager.Current.Autostart_StartMinimizedInTray;
        }
        #endregion       
    }
}