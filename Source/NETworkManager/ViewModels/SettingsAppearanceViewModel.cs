using NETworkManager.Models.Appearance;
using NETworkManager.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace NETworkManager.ViewModels;

public class SettingsAppearanceViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    public ICollectionView Themes { get; }

    /// <summary>
    ///     Light themes (Light, Mist, Soft, Paper), shown in an own row.
    /// </summary>
    public ICollectionView LightThemes { get; }

    /// <summary>
    ///     Dark themes (Dark, Black, Midnight, ...), shown in an own row.
    /// </summary>
    public ICollectionView DarkThemes { get; }

    public ThemeColorInfo SelectedTheme
    {
        get;
        set
        {
            // null: the other row (light / dark) clears its selection when a theme of this row is selected
            if (value == null || value == field)
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

    public bool AccentGlow
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
            {
                SettingsManager.Current.Appearance_AccentGlow = value;
                AppearanceManager.ApplyWindowBrush();
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<AnimationMode> AnimationModes => Enum.GetValues<AnimationMode>();

    public AnimationMode AnimationMode
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Appearance_Animations = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets whether the animations are managed by system-wide policy (on / off).
    /// </summary>
    public bool IsAnimationsManagedByPolicy => PolicyManager.Current?.Appearance_Animations.HasValue == true;

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
        LightThemes = new CollectionViewSource { Source = AppearanceManager.Themes }.View;
        LightThemes.Filter = x => x is ThemeColorInfo info && !IsDark(info);
        DarkThemes = new CollectionViewSource { Source = AppearanceManager.Themes }.View;
        DarkThemes.Filter = x => x is ThemeColorInfo info && IsDark(info);
        Accents = new CollectionViewSource { Source = AppearanceManager.Accents }.View;
        CustomThemes = new CollectionViewSource { Source = AppearanceManager.CustomThemes }.View;

        LoadSettings();

        _isLoading = false;
    }

    /// <summary>
    ///     A theme is dark if its background (color of the theme) is dark.
    /// </summary>
    private static bool IsDark(ThemeColorInfo theme) =>
        theme.Color is SolidColorBrush { Color: var color } &&
        0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B < 128;

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
        AccentGlow = SettingsManager.Current.Appearance_AccentGlow;
        // If policy is set, show the policy value (on / off); otherwise show the user's setting
        AnimationMode = PolicyManager.Current?.Appearance_Animations is { } animationsPolicy
            ? animationsPolicy ? AnimationMode.On : AnimationMode.Off
            : SettingsManager.Current.Appearance_Animations;
        PowerShellModifyGlobalProfile = SettingsManager.Current.Appearance_PowerShellModifyGlobalProfile;
    }

    #endregion
}