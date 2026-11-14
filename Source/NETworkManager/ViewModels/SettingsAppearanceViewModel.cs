using NETworkManager.Models.Appearance;
using NETworkManager.Settings;
using System;
using System.Collections.Generic;
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

    public IEnumerable<AccentGlowIntensity> AccentGlowIntensities => Enum.GetValues<AccentGlowIntensity>();

    public AccentGlowIntensity AccentGlowIntensity
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
            {
                SettingsManager.Current.Appearance_AccentGlowIntensity = value;
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
        AccentGlowIntensity = SettingsManager.Current.Appearance_AccentGlowIntensity;
        // If policy is set, show the policy value (on / off); otherwise show the user's setting
        AnimationMode = PolicyManager.Current?.Appearance_Animations is { } animationsPolicy
            ? animationsPolicy ? AnimationMode.On : AnimationMode.Off
            : SettingsManager.Current.Appearance_Animations;
        PowerShellModifyGlobalProfile = SettingsManager.Current.Appearance_PowerShellModifyGlobalProfile;
    }

    #endregion
}