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

    /// <summary>
    ///     Convert the offset of a clock (e.g. the time of a host minus the local time) to a short string with sign and
    ///     the two largest units: "+1.2 s", "-3 min 5 s", "+2 h 3 min", "+1 d 4 h".
    /// </summary>
    /// <param name="offset">Offset.</param>
    /// <param name="provider">Format of the decimal separator, null for the current culture.</param>
    /// <returns>Offset as string.</returns>
    public static string TimeSpanToOffsetString(TimeSpan offset, IFormatProvider provider = null)
    {
        var sign = offset < TimeSpan.Zero ? "-" : "+";
        var value = offset.Duration();

        // Rounded to the smaller unit shown, so e.g. 59.97 s is "1 min 0 s" and not "60.0 s"
        if (value.TotalSeconds < 59.95)
            return sign + string.Format(provider, "{0:0.0} s", value.TotalSeconds);

        var seconds = (long)Math.Round(value.TotalSeconds);

        if (seconds < 3600)
            return $"{sign}{seconds / 60} min {seconds % 60} s";

        var minutes = (long)Math.Round(value.TotalMinutes);

        if (minutes < 24 * 60)
            return $"{sign}{minutes / 60} h {minutes % 60} min";

        var hours = (long)Math.Round(value.TotalHours);

        return $"{sign}{hours / 24} d {hours % 24} h";
    }
}