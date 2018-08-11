using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Threading.Tasks;

namespace NETworkManager.ViewModels
{
    public class SettingsAutostartViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        private readonly bool _isLoading;

        private bool _startWithWindows;
        public bool StartWithWindows
        {
            get => _startWithWindows;
            set
            {
                if (value == _startWithWindows)
                    return;

                if (!_isLoading)
                    EnableDisableAutostart(value);

                _startWithWindows = value;
                OnPropertyChanged();
            }
        }

        private bool _configuringAutostart;
        public bool ConfiguringAutostart
        {
            get => _configuringAutostart;
            set
            {
                if (value == _configuringAutostart)
                    return;

                _configuringAutostart = value;
                OnPropertyChanged();
            }
        }

        private bool _startMinimizedInTray;
        public bool StartMinimizedInTray
        {
            get => _startMinimizedInTray;
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

        #region Constructor
        public SettingsAutostartViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            LoadSettings();

            _isLoading = false;
        }
        #endregion

        #region Load settings
        private void LoadSettings()
        {
            StartWithWindows = AutostartManager.IsEnabled;
            StartMinimizedInTray = SettingsManager.Current.Autostart_StartMinimizedInTray;
        }
        #endregion

        #region Methods
        private async void EnableDisableAutostart(bool enable)
        {
            ConfiguringAutostart = true;

            try
            {
                if (enable)
                    await AutostartManager.EnableAsync();
                else
                    await AutostartManager.DisableAsync();

                // Show the user some awesome animation to indicate we are working on it :)
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, Resources.Localization.Strings.Error, ex.Message, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
            }

            ConfiguringAutostart = false;
        }
        #endregion
    }
}