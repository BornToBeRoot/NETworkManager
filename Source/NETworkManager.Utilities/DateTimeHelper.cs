using System;

namespace NETworkManager.Utilities;

/// <summary>
/// Class provides static methods to convert DateTime to string.
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Convert a DateTime to a string with the format "yyyy-MM-dd HH:mm:ss.fff".
    /// </summary>
    /// <param name="dateTime">DateTime to convert.</param>
    /// <returns>String with the format "yyyy-MM-dd HH:mm:ss.fff".</returns>
    public static string DateTimeToString(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }
}
