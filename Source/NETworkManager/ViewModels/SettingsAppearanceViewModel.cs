using MahApps.Metro;
using NETworkManager.Settings;

namespace NETworkManager.ViewModels
{
    public class SettingsAppearanceViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        private AppTheme _appThemeSelectedItem;
        public AppTheme AppThemeSelectedItem
        {
            get => _appThemeSelectedItem;
            set
            {
                if (value == _appThemeSelectedItem)
                    return;

                if (!_isLoading)
                {
                    AppearanceManager.ChangeAppTheme(value.Name);
                    SettingsManager.Current.Appearance_AppTheme = value.Name;
                }

                _appThemeSelectedItem = value;
                OnPropertyChanged();
            }
        }

        private Accent _accentSelectedItem;
        public Accent AccentSelectedItem
        {
            get => _accentSelectedItem;
            set
            {
                if (value == _accentSelectedItem)
                    return;

                if (!_isLoading)
                {
                    AppearanceManager.ChangeAccent(value.Name);
                    SettingsManager.Current.Appearance_Accent = value.Name;
                }

                _accentSelectedItem = value;
                OnPropertyChanged();
            }
        }
        #endregion        

        #region Constructor, LoadSettings
        public SettingsAppearanceViewModel()
        {
            _isLoading = true;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            AppThemeSelectedItem = ThemeManager.DetectAppStyle().Item1;
            AccentSelectedItem = ThemeManager.DetectAppStyle().Item2;
        }
        #endregion
    }
}