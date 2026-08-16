using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class PingMonitorHostView
{
    /// <summary>
    ///     Delay before a group's action buttons (Start/Pause/Close), revealed via mouse-over or by
    ///     clicking/tapping the "..." toggle, are hidden again after the last interaction.
    /// </summary>
    private static readonly TimeSpan GroupActionsAutoCloseDelay = TimeSpan.FromSeconds(20);

    private readonly PingMonitorHostViewModel _viewModel = new();

    public PingMonitorHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void AddHost(string host)
    {
        if (_viewModel.SetHost(host))
            _ = _viewModel.Start();
    }

    public void OnViewHide()
    {
        _viewModel.OnViewHide();
    }

    public void OnViewVisible()
    {
        _viewModel.OnViewVisible();
    }

    /// <summary>
    ///     Reveals the group's action buttons and cancels any pending auto-close, so hovering the
    ///     header keeps them visible for as long as the mouse stays over it.
    /// </summary>
    private void GroupHeader_MouseEnter(object sender, MouseEventArgs e)
    {
        if (((Grid)sender).FindName("GroupActionsToggle") is not ToggleButton toggle)
            return;

        StopGroupActionsAutoCloseTimer(toggle);
        toggle.IsChecked = true;
    }

    /// <summary>
    ///     Starts the auto-close countdown once the mouse leaves the header, instead of hiding the
    ///     action buttons immediately - this avoids them flickering away while moving the mouse
    ///     from the header onto one of the buttons.
    /// </summary>
    private void GroupHeader_MouseLeave(object sender, MouseEventArgs e)
    {
        if (((Grid)sender).FindName("GroupActionsToggle") is not ToggleButton toggle)
            return;

        if (toggle.IsChecked == true)
            StartGroupActionsAutoCloseTimer(toggle);
    }

    /// <summary>
    ///     Starts the auto-close countdown when the toggle is checked by a click/tap rather than by
    ///     hovering the header (<see cref="GroupHeader_MouseEnter"/> already handles the hover case,
    ///     and cancels this if the mouse is still over the header).
    /// </summary>
    private void GroupActionsToggle_Checked(object sender, RoutedEventArgs e)
    {
        var toggle = (ToggleButton)sender;

        if (!IsMouseOverGroupHeader(toggle))
            StartGroupActionsAutoCloseTimer(toggle);
    }

    private void GroupActionsToggle_Unchecked(object sender, RoutedEventArgs e)
    {
        StopGroupActionsAutoCloseTimer((ToggleButton)sender);
    }

    /// <summary>
    ///     Stops any pending auto-close timer when the group (and its toggle) is torn down, e.g.
    ///     because the group was closed - otherwise the timer would keep the detached toggle alive
    ///     until it fires.
    /// </summary>
    private void GroupActionsToggle_Unloaded(object sender, RoutedEventArgs e)
    {
        StopGroupActionsAutoCloseTimer((ToggleButton)sender);
    }

    private static bool IsMouseOverGroupHeader(ToggleButton toggle)
    {
        return toggle.FindName("GroupHeaderGrid") is Grid headerGrid && headerGrid.IsMouseOver;
    }

    private static void StartGroupActionsAutoCloseTimer(ToggleButton toggle)
    {
        StopGroupActionsAutoCloseTimer(toggle);

        var timer = new DispatcherTimer { Interval = GroupActionsAutoCloseDelay };
        timer.Tick += (_, _) =>
        {
            StopGroupActionsAutoCloseTimer(toggle);
            toggle.IsChecked = false;
        };

        toggle.Tag = timer;
        timer.Start();
    }

    private static void StopGroupActionsAutoCloseTimer(ToggleButton toggle)
    {
        if (toggle.Tag is not DispatcherTimer timer)
            return;

        timer.Stop();
        toggle.Tag = null;
    }
}
