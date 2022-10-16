using NETworkManager.Settings;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;

namespace NETworkManager.ViewModels
{
    public class SettingsAppearanceViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        public ICollectionView Themes { get; private set; }

        private ThemeColorInfo _selectedTheme;
        public ThemeColorInfo SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (value == _selectedTheme)
                    return;

                if (!_isLoading && !UseCustomTheme)
                {
                    AppearanceManager.ChangeTheme(value.Name);
                    SettingsManager.Current.Appearance_Theme = value.Name;
                }

                _selectedTheme = value;
                OnPropertyChanged();
            }
        }


        public ICollectionView Accents { get; private set; }

        private AccentColorInfo _selectedAccent;
        public AccentColorInfo SelectedAccent
        {
            get => _selectedAccent;
            set
            {
                if (value == _selectedAccent)
                    return;

                if (!_isLoading && !UseCustomTheme)
                {
                    AppearanceManager.ChangeAccent(value.Name);
                    SettingsManager.Current.Appearance_Accent = value.Name;
                }

                _selectedAccent = value;
                OnPropertyChanged();
            }
        }

        private bool _useCustomTheme;
        public bool UseCustomTheme
        {
            get => _useCustomTheme;
            set
            {
                if (value == _useCustomTheme)
                    return;

                if (!_isLoading)
                {
                    SettingsManager.Current.Appearance_UseCustomTheme = value;
                    AppearanceManager.Load();
                }

                _useCustomTheme = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView CustomThemes { get; private set; }


        private ThemeInfo _selectedCustomTheme;
        public ThemeInfo SelectedCustomTheme
        {
            get => _selectedCustomTheme;
            set
            {
                if (value == _selectedCustomTheme)
                    return;

                if (!_isLoading && UseCustomTheme)
                {
                    AppearanceManager.ChangeTheme(value);
                    SettingsManager.Current.Appearance_CustomThemeName = value.Name;
                }

                _selectedCustomTheme = value;
                OnPropertyChanged();
            }
        }

        private bool _powerShellModifyGlobalProfile;
        public bool PowerShellModifyGlobalProfile
        {
            get => _powerShellModifyGlobalProfile;
            set
            {
                if (value == _powerShellModifyGlobalProfile)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Appearance_PowerShellModifyGlobalProfile = value;

                _powerShellModifyGlobalProfile = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, LoadSettings
        public SettingsAppearanceViewModel()
        {
            _isLoading = true;

            Themes = new CollectionViewSource { Source = AppearanceManager.Themes }.View;
            Accents = new CollectionViewSource { Source = AppearanceManager.Accents }.View;
            CustomThemes = new CollectionViewSource { Source = AppearanceManager.CustomThemes }.View;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            SelectedTheme = Themes.Cast<ThemeColorInfo>().FirstOrDefault(x => x.Name == SettingsManager.Current.Appearance_Theme);
            SelectedAccent = Accents.Cast<AccentColorInfo>().FirstOrDefault(x => x.Name == SettingsManager.Current.Appearance_Accent);            
            UseCustomTheme = SettingsManager.Current.Appearance_UseCustomTheme;            
            SelectedCustomTheme = CustomThemes.Cast<ThemeInfo>().FirstOrDefault(x => x.Name == SettingsManager.Current.Appearance_CustomThemeName) ?? CustomThemes.Cast<ThemeInfo>().FirstOrDefault();
            PowerShellModifyGlobalProfile = SettingsManager.Current.Appearance_PowerShellModifyGlobalProfile;
        }
        #endregion
    }
}