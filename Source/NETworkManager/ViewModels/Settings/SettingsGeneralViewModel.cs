using System.Linq;
using NETworkManager.Views;
using NETworkManager.Models.Settings;
using System.Collections.ObjectModel;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsGeneralViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        public ObservableCollection<ApplicationViewInfo> ApplicationViewCollection { get; set; }

        private ApplicationViewInfo _defaultApplicationViewSelectedItem;
        public ApplicationViewInfo DefaultApplicationViewSelectedItem
        {
            get { return _defaultApplicationViewSelectedItem; }
            set
            {
                if (value == _defaultApplicationViewSelectedItem)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Application_DefaultApplicationViewName = value.Name;

                _defaultApplicationViewSelectedItem = value;
                OnPropertyChanged();
            }
        }

        private bool _startMaximized;
        public bool StartMaximized
        {
            get { return _startMaximized; }
            set
            {
                if (value == _startMaximized)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Window_StartMaximized = value;

                _startMaximized = value;
                OnPropertyChanged();
            }
        }

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
        public SettingsGeneralViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            if (SettingsManager.Current.DeveloperMode)
                ApplicationViewCollection = new ObservableCollection<ApplicationViewInfo>(ApplicationView.List.OrderBy(x => x.Name));
            else
                ApplicationViewCollection = new ObservableCollection<ApplicationViewInfo>(ApplicationView.List.Where(x => x.IsDev == false).OrderBy(x => x.Name));

            DefaultApplicationViewSelectedItem = ApplicationViewCollection.FirstOrDefault(x => x.Name == SettingsManager.Current.Application_DefaultApplicationViewName);

            StartMaximized = SettingsManager.Current.Window_StartMaximized;
            AlwaysShowIconInTray = SettingsManager.Current.TrayIcon_AlwaysShowIcon;
            MinimizeInsteadOfTerminating = SettingsManager.Current.Window_MinimizeInsteadOfTerminating;
            ConfirmClose = SettingsManager.Current.Window_ConfirmClose;
            MinimizeToTrayInsteadOfTaskbar = SettingsManager.Current.Window_MinimizeToTrayInsteadOfTaskbar;
        }
        #endregion
    }
}