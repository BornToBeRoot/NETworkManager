using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using NETworkManager.Settings;

namespace NETworkManager;

/// <summary>
/// Attached property for the value of a progress bar (bind it instead of <see cref="RangeBase.Value"/>, e.g. the
/// progress of the IP Scanner): the bar moves smoothly to the new value instead of jumping.
///
/// Many updates (e.g. thousands of hosts per second): a new animation starts at most every 100 ms to the latest value,
/// the values in between are skipped - no animation per update. The duration follows the time between the updates
/// (100 ms to 1 s, linear), so the bar moves continuously. The minimum (start of a scan) and the maximum (end) are set
/// immediately, the bar is never behind when the scan is done. Without animations (see
/// <see cref="AppearanceManager.AnimationsEnabled"/>) the value is set immediately.
/// </summary>
public static class SmoothProgress
{
    private static readonly TimeSpan MinInterval = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan MaxDuration = TimeSpan.FromSeconds(1);

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.RegisterAttached(
            "Value",
            typeof(double),
            typeof(SmoothProgress),
            new PropertyMetadata(0.0, OnValueChanged));

    public static void SetValue(RangeBase element, double value) =>
        element.SetValue(ValueProperty, value);

    public static double GetValue(RangeBase element) =>
        (double)element.GetValue(ValueProperty);

    /// <summary>
    /// State of the animation of a progress bar.
    /// </summary>
    private static readonly DependencyProperty StateProperty =
        DependencyProperty.RegisterAttached(
            "State",
            typeof(State),
            typeof(SmoothProgress),
            new PropertyMetadata(null));

    private sealed class State
    {
        public readonly Stopwatch SinceLastAnimation = new();
        public DispatcherTimer Timer;
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RangeBase rangeBase)
            return;

        var target = (double)e.NewValue;

        if (rangeBase.GetValue(StateProperty) is not State state)
        {
            state = new State();
            rangeBase.SetValue(StateProperty, state);
        }

        // Start / end of the progress, not shown or no animations: immediately
        if (target <= rangeBase.Minimum || target >= rangeBase.Maximum || !rangeBase.IsVisible ||
            !AppearanceManager.AnimationsEnabled)
        {
            state.Timer?.Stop();
            state.SinceLastAnimation.Reset();

            rangeBase.BeginAnimation(RangeBase.ValueProperty, null);
            rangeBase.Value = target;

            return;
        }

        // Last animation started less than 100 ms ago: animate to the latest value when the interval is over
        if (state.SinceLastAnimation.IsRunning && state.SinceLastAnimation.Elapsed < MinInterval)
        {
            if (state.Timer is { IsEnabled: true })
                return;

            state.Timer ??= CreateTimer(rangeBase, state);
            state.Timer.Interval = MinInterval - state.SinceLastAnimation.Elapsed;
            state.Timer.Start();

            return;
        }

        Animate(rangeBase, state);
    }

    private static DispatcherTimer CreateTimer(RangeBase rangeBase, State state)
    {
        var timer = new DispatcherTimer(DispatcherPriority.Render, rangeBase.Dispatcher);

        timer.Tick += (_, _) =>
        {
            timer.Stop();
            Animate(rangeBase, state);
        };

        return timer;
    }

    /// <summary>
    /// Animates from the current (animated) value to the latest value.
    /// </summary>
    private static void Animate(RangeBase rangeBase, State state)
    {
        var target = GetValue(rangeBase);

        // Duration: the time since the last animation (the update rate), 100 ms to 1 s
        var duration = state.SinceLastAnimation.IsRunning ? state.SinceLastAnimation.Elapsed : MinInterval;

        if (duration < MinInterval)
            duration = MinInterval;
        else if (duration > MaxDuration)
            duration = MaxDuration;

        state.SinceLastAnimation.Restart();

        rangeBase.BeginAnimation(RangeBase.ValueProperty, new DoubleAnimation(target, new Duration(duration)));
    }
}
