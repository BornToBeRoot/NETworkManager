using System;
using System.Windows;
using NETworkManager.Utilities;

namespace NETworkManager.Settings;

/// <summary>
/// Shared expanded/width state of the profile panel, used by every tool that hosts a profile list. Because all
/// tools observe the same <see cref="Current" /> instance, resizing or collapsing the profile panel in one tool
/// is reflected in every other tool automatically through data binding - no manual synchronization needed.
/// </summary>
/// <remarks>
/// <see cref="Current" /> reads <see cref="SettingsManager.Current" /> the first time it is accessed. This is
/// only safe once <see cref="SettingsManager.Load" /> has completed (done early in App.xaml.cs, before the main
/// window - and therefore any tool view - is created). Do not reference <see cref="Current" /> from eagerly
/// loaded resources (e.g. App.xaml or globally merged style dictionaries).
/// </remarks>
public sealed class ProfileViewState : PropertyChangedBase
{
    public static ProfileViewState Current { get; }

    static ProfileViewState()
    {
        Current = new ProfileViewState();
    }

    private readonly bool _isLoading;
    private bool _canProfileWidthChange = true;
    private double _tempProfileWidth;

    private ProfileViewState()
    {
        _isLoading = true;

        // Must be set before ExpandProfileView below, since assigning it can synchronously trigger
        // ResizeProfile(false), which reads _tempProfileWidth.
        _tempProfileWidth = SettingsManager.Current.Profile_Width;

        ExpandProfileView = SettingsManager.Current.Profile_ExpandView;

        ProfileWidth = ExpandProfileView
            ? new GridLength(SettingsManager.Current.Profile_Width)
            : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

        _isLoading = false;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the profile panel is expanded, shared across all tools.
    /// </summary>
    public bool ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Profile_ExpandView = value;

            field = value;

            if (_canProfileWidthChange)
                ResizeProfile(false);

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the width of the profile panel, shared across all tools.
    /// </summary>
    public GridLength ProfileWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading && Math.Abs(value.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) >
                GlobalStaticConfiguration.FloatPointFix) // Do not save the size when collapsed
                SettingsManager.Current.Profile_Width = value.Value;

            field = value;

            if (_canProfileWidthChange)
                ResizeProfile(true);

            OnPropertyChanged();
        }
    }

    private void ResizeProfile(bool dueToChangedSize)
    {
        _canProfileWidthChange = false;

        if (dueToChangedSize)
        {
            ExpandProfileView = Math.Abs(ProfileWidth.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) >
                                GlobalStaticConfiguration.FloatPointFix;
        }
        else
        {
            if (ExpandProfileView)
            {
                ProfileWidth =
                    Math.Abs(_tempProfileWidth - GlobalStaticConfiguration.Profile_WidthCollapsed) <
                    GlobalStaticConfiguration.FloatPointFix
                        ? new GridLength(GlobalStaticConfiguration.Profile_DefaultWidthExpanded)
                        : new GridLength(_tempProfileWidth);
            }
            else
            {
                _tempProfileWidth = ProfileWidth.Value;
                ProfileWidth = new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);
            }
        }

        _canProfileWidthChange = true;
    }
}
