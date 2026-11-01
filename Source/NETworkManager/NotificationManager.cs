using NETworkManager.Models;
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
    /// Shows a notification of a tool (popup in the bottom-right corner of the primary screen) and / or plays its sound
    /// (settings of the tool): a gentle info sound if something is up, otherwise a warning sound. Safe to call from any
    /// thread.
    /// </summary>
    /// <param name="notification">The notification.</param>
    /// <param name="showPopup">Show the popup.</param>
    /// <param name="playSound">Play the sound.</param>
    public static void Show(NotificationInfo notification, bool showPopup, bool playSound)
    {
        if (playSound)
            PlaySound(notification.Status == NotificationStatus.Up ? SystemSounds.Asterisk : SystemSounds.Exclamation);

        if (!showPopup)
            return;

        // A late ping callback during application shutdown may find no Application instance.
        if (Application.Current is null)
            return;

        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            var window = new NotificationWindow(notification);

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
    ///     Gets the lines of a notification about changes: up to <paramref name="maxLines" /> changes, one per line,
    ///     otherwise the summary (one line per counter, e.g. "5 New" and "2 Gone"), so the popup stays small.
    /// </summary>
    /// <param name="lines">One line per change.</param>
    /// <param name="summary">Summary of all changes (counters without a value of 0).</param>
    /// <param name="maxLines">Maximum number of lines.</param>
    /// <returns>Lines of the notification.</returns>
    public static IReadOnlyList<NotificationLine> FormatChanges(IReadOnlyList<NotificationLine> lines,
        IReadOnlyList<NotificationLine> summary, int maxLines = 3)
    {
        return lines.Count <= maxLines ? lines : summary;
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
    /// Repositions all active windows, e.g. after one closes or its height changes so the stack
    /// stays bottom-anchored without gaps or overlap.
    /// </summary>
    internal static void RepositionAll()
    {
        foreach (var window in ActiveWindows)
            window.Reposition();
    }
}
