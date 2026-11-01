using System;
using System.Windows;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Controls;

/// <summary>
///     Status of a refresh in the header of a tool: nothing while refreshing (the refresh icon spins), a countdown
///     while the next automatic refresh is planned (<see cref="RemainingTime" />), otherwise the time of the last
///     refresh. The tooltip shows the date and time of the last refresh.
/// </summary>
public partial class RefreshStatusControl
{
    /// <summary>
    ///     Identifies the <see cref="LastRefreshTime" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty LastRefreshTimeProperty = DependencyProperty.Register(
        nameof(LastRefreshTime), typeof(DateTime?), typeof(RefreshStatusControl),
        new PropertyMetadata(null, OnChanged));

    /// <summary>
    ///     Identifies the <see cref="RemainingTime" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty RemainingTimeProperty = DependencyProperty.Register(
        nameof(RemainingTime), typeof(TimeSpan?), typeof(RefreshStatusControl),
        new PropertyMetadata(null, OnChanged));

    /// <summary>
    ///     Identifies the <see cref="IsRefreshing" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsRefreshingProperty = DependencyProperty.Register(
        nameof(IsRefreshing), typeof(bool), typeof(RefreshStatusControl), new PropertyMetadata(false, OnChanged));

    /// <summary>
    ///     Identifies the <see cref="LastRefreshFormat" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty LastRefreshFormatProperty = DependencyProperty.Register(
        nameof(LastRefreshFormat), typeof(string), typeof(RefreshStatusControl),
        new PropertyMetadata(Strings.ReloadedAtX, OnChanged));

    /// <summary>
    ///     Identifies the <see cref="RemainingTimeFormat" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty RemainingTimeFormatProperty = DependencyProperty.Register(
        nameof(RemainingTimeFormat), typeof(string), typeof(RefreshStatusControl),
        new PropertyMetadata(Strings.NextRefreshInX, OnChanged));

    public RefreshStatusControl()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Gets or sets the time of the last refresh, null before the first one.
    /// </summary>
    public DateTime? LastRefreshTime
    {
        get => (DateTime?)GetValue(LastRefreshTimeProperty);
        set => SetValue(LastRefreshTimeProperty, value);
    }

    /// <summary>
    ///     Gets or sets the time until the next automatic refresh, null if none is planned (see
    ///     <see cref="NETworkManager.Utilities.WPF.AutoRefreshTimer.RemainingTime" />).
    /// </summary>
    public TimeSpan? RemainingTime
    {
        get => (TimeSpan?)GetValue(RemainingTimeProperty);
        set => SetValue(RemainingTimeProperty, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether a refresh is running (nothing is shown).
    /// </summary>
    public bool IsRefreshing
    {
        get => (bool)GetValue(IsRefreshingProperty);
        set => SetValue(IsRefreshingProperty, value);
    }

    /// <summary>
    ///     Gets or sets the text of the last refresh with the time as <c>{0}</c> (default "Reloaded at {0}").
    /// </summary>
    public string LastRefreshFormat
    {
        get => (string)GetValue(LastRefreshFormatProperty);
        set => SetValue(LastRefreshFormatProperty, value);
    }

    /// <summary>
    ///     Gets or sets the text of the countdown with the remaining time as <c>{0}</c> (default "Next refresh in
    ///     {0}").
    /// </summary>
    public string RemainingTimeFormat
    {
        get => (string)GetValue(RemainingTimeFormatProperty);
        set => SetValue(RemainingTimeFormatProperty, value);
    }

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((RefreshStatusControl)d).Update();
    }

    private void Update()
    {
        string text = null;

        if (!IsRefreshing)
        {
            if (RemainingTime is { } remainingTime)
                text = string.Format(RemainingTimeFormat, DateTimeHelper.TimeSpanToCountdownString(remainingTime));
            else if (LastRefreshTime is { } lastRefreshTime)
                text = string.Format(LastRefreshFormat, lastRefreshTime.ToShortTimeString());
        }

        TextBlockStatus.Text = text;
        Visibility = text == null ? Visibility.Collapsed : Visibility.Visible;

        // Date and time, e.g. for a tool that is open for days
        ToolTip = LastRefreshTime is { } time ? string.Format(LastRefreshFormat, time.ToString("G")) : null;
    }
}
