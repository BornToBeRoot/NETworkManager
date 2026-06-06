using MahApps.Metro.IconPacks;
using NETworkManager.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows;

namespace NETworkManager;

/// <summary>
/// Shows and manages stackable notification popups. Follows the same pattern as
/// <see cref="DialogHelper"/> — callers pass display parameters; internals are hidden.
/// </summary>
public static class NotificationManager
{
    private static readonly List<NotificationWindow> ActiveWindows = [];

    /// <summary>
    /// Margin (in DIP) between the screen edges and between stacked notification windows.
    /// </summary>
    internal const double WindowMargin = 10.0;

    // Throttles the notification sound (see GlobalStaticConfiguration.NotificationSoundThrottle)
    // so a burst of near-simultaneous status changes collapses into a single sound.
    private static readonly Lock SoundLock = new();
    private static DateTime _lastSoundPlayed = DateTime.MinValue;

    /// <summary>
    /// Shows a notification popup in the bottom-right corner of the primary screen.
    /// Safe to call from any thread.
    /// </summary>
    /// <param name="iconKind">The Material icon shown on the left of the popup.</param>
    /// <param name="iconColor">The icon fill color (e.g. "Red" or "#badc58").</param>
    /// <param name="title">The bold header text (e.g. the host title).</param>
    /// <param name="message">The gray subtext below the header (e.g. "Host is up").</param>
    /// <param name="closeTimeSeconds">Seconds before the popup closes automatically.</param>
    public static void Show(PackIconMaterialKind iconKind, string iconColor, string title, string message, int closeTimeSeconds)
    {
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            var window = new NotificationWindow(iconKind, iconColor, title, message, closeTimeSeconds);

            window.Closed += (_, _) =>
            {
                ActiveWindows.Remove(window);
                RepositionAll();
            };

            ActiveWindows.Add(window);
            window.Show(); // triggers OnSourceInitialized → window positions itself
        });
    }

    /// <summary>
    /// Returns the combined height (including margins) of all windows stacked below the given
    /// window, i.e. the vertical offset from the bottom edge at which it should be placed.
    /// </summary>
    internal static double GetStackOffset(NotificationWindow window)
    {
        return ActiveWindows.TakeWhile(w => !ReferenceEquals(w, window)).Sum(w => w.ActualHeight + WindowMargin);
    }

    /// <summary>
    /// Plays a notification sound, throttled so that a burst of near-simultaneous status changes
    /// (e.g. many hosts going down at once) results in a single sound. Safe to call from any thread.
    /// </summary>
    /// <param name="sound">The system sound to play.</param>
    public static void PlaySound(SystemSound sound)
    {
        lock (SoundLock)
        {
            var now = DateTime.UtcNow;

            if ((now - _lastSoundPlayed).TotalMilliseconds < GlobalStaticConfiguration.NotificationSoundThrottle)
                return;

            _lastSoundPlayed = now;
        }

        sound.Play();
    }

    /// <summary>
    /// Repositions all active windows after a sibling closes and stack indices shift.
    /// </summary>
    internal static void RepositionAll()
    {
        foreach (var window in ActiveWindows)
            window.Reposition();
    }
}
