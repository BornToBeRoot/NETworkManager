using System;

namespace NETworkManager.Utilities;

/// <summary>
///     Class provides static methods to convert DateTime to string.
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    ///     Convert a DateTime to a string with the format "yyyy-MM-dd HH:mm:ss.fff".
    /// </summary>
    /// <param name="dateTime">DateTime to convert.</param>
    /// <returns>String with the format "yyyy-MM-dd HH:mm:ss.fff".</returns>
    public static string DateTimeToFullDateTimeString(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }

    /// <summary>
    ///     Convert a DateTime to a string with the format "HH:mm:ss".
    /// </summary>
    /// <param name="dateTime">DateTime to convert.</param>
    /// <returns>String with the format "HH:mm:ss".</returns>
    public static string DateTimeToTimeString(DateTime dateTime)
    {
        return dateTime.ToString("HH:mm:ss");
    }

    /// <summary>
    ///     Convert the time until an event (e.g. the next refresh) to a countdown: "mm:ss", "h:mm:ss" from one hour
    ///     (fixed width while counting down). Negative times are shown as "00:00".
    /// </summary>
    /// <param name="timeSpan">Remaining time.</param>
    /// <returns>Countdown, e.g. "04:32" or "1:00:00".</returns>
    public static string TimeSpanToCountdownString(TimeSpan timeSpan)
    {
        if (timeSpan < TimeSpan.Zero)
            timeSpan = TimeSpan.Zero;

        return timeSpan.TotalHours >= 1
            ? $"{(int)timeSpan.TotalHours}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}"
            : $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
    }
}