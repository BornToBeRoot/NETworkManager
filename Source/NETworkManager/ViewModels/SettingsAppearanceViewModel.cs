using MahApps.Metro;
using NETworkManager.Models.Settings;

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

        private bool _enableTransparency;
        public bool EnableTransparency
        {
            get => _enableTransparency;
            set
            {
                if (value == _enableTransparency)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Appearance_EnableTransparency = value;

                _enableTransparency = value;
                OnPropertyChanged();
            }
        }

        private int _opacity;
        public int Opacity
        {
            get => _opacity;
            set
            {
                if (value == _opacity)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Appearance_Opacity = (double)value / 100;

                _opacity = value;
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
            EnableTransparency = SettingsManager.Current.Appearance_EnableTransparency;
            Opacity = (int)(SettingsManager.Current.Appearance_Opacity * 100);
        }
        #endregion
    }
}