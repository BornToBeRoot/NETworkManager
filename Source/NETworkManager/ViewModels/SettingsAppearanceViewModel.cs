using NETworkManager.Models.Appearance;
using NETworkManager.Settings;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace NETworkManager.ViewModels;

public class SettingsAppearanceViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    public ICollectionView Themes { get; }

    public ThemeColorInfo SelectedTheme
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading && !UseCustomTheme)
            {
                AppearanceManager.ChangeTheme(value.Name, SelectedAccent.Name);
                SettingsManager.Current.Appearance_Theme = value.Name;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView Accents { get; }

    public AccentColorInfo SelectedAccent
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading && !UseCustomTheme)
            {
                AppearanceManager.ChangeTheme(SelectedTheme.Name, value.Name);
                SettingsManager.Current.Appearance_Accent = value.Name;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool UseCustomTheme
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
            {
                SettingsManager.Current.Appearance_UseCustomTheme = value;
                AppearanceManager.Load();
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView CustomThemes { get; }


    public ThemeInfo SelectedCustomTheme
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading && UseCustomTheme)
            {
                AppearanceManager.ChangeTheme(value.Name);
                SettingsManager.Current.Appearance_CustomThemeName = value.Name;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PowerShellModifyGlobalProfile
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Appearance_PowerShellModifyGlobalProfile = value;

            field = value;
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
        SelectedTheme = Themes.Cast<ThemeColorInfo>()
            .FirstOrDefault(x => x.Name == SettingsManager.Current.Appearance_Theme);
        SelectedAccent = Accents.Cast<AccentColorInfo>()
            .FirstOrDefault(x => x.Name == SettingsManager.Current.Appearance_Accent);
        UseCustomTheme = SettingsManager.Current.Appearance_UseCustomTheme;
        SelectedCustomTheme =
            CustomThemes.Cast<ThemeInfo>()
                .FirstOrDefault(x => x.Name == SettingsManager.Current.Appearance_CustomThemeName) ??
            CustomThemes.Cast<ThemeInfo>().FirstOrDefault();
        PowerShellModifyGlobalProfile = SettingsManager.Current.Appearance_PowerShellModifyGlobalProfile;
    }

    #endregion
}