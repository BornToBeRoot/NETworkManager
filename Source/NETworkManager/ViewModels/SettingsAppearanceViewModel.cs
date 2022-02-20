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

                if (!_isLoading)
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

                if (!_isLoading)
                {
                    AppearanceManager.ChangeAccent(value.Name);
                    SettingsManager.Current.Appearance_Accent = value.Name;
                }

                _selectedAccent = value;
                OnPropertyChanged();
            }
        }

        #endregion        

        #region Constructor, LoadSettings
        public SettingsAppearanceViewModel()
        {
            _isLoading = true;

            Accents = new CollectionViewSource { Source = AppearanceManager.Accents }.View;

            // ToDo: Enable White Theme again (Fix: PuTTY and PowerShell)
            Themes = new CollectionViewSource { Source = AppearanceManager.Themes }.View;
            // Themes = new CollectionViewSource { Source = AppearanceManager.Themes.Where(x => x.Name != "Light") }.View;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            SelectedTheme = Themes.Cast<ThemeColorInfo>().FirstOrDefault(x => x.Name == SettingsManager.Current.Appearance_Theme);
            SelectedAccent = Accents.Cast<AccentColorInfo>().FirstOrDefault(x => x.Name == SettingsManager.Current.Appearance_Accent);
        }
        #endregion
    }
}