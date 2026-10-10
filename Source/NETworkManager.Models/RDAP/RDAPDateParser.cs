using System;
using System.Globalization;

namespace NETworkManager.Models.RDAP;

/// <summary>
///     Lenient parser for RDAP dates. RFC 9083 requires RFC 3339 dates, but servers send "Z", numeric offsets,
///     fractional seconds and even dates without any offset.
/// </summary>
public static class RDAPDateParser
{
    /// <summary>
    ///     Parses an RDAP date. Dates without an offset are treated as UTC.
    /// </summary>
    /// <param name="value">Date as sent by the server.</param>
    /// <param name="date">Parsed date.</param>
    /// <returns>True if the date could be parsed.</returns>
    public static bool TryParse(string value, out DateTimeOffset date)
    {
        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces, out date);
    }

    /// <summary>
    ///     Formats an RDAP date in local time for display. Returns the original value if it cannot be parsed.
    /// </summary>
    /// <param name="value">Date as sent by the server.</param>
    /// <returns>Formatted date.</returns>
    public static string ToLocalString(string value)
    {
        return TryParse(value, out var date)
            ? date.LocalDateTime.ToString(CultureInfo.CurrentCulture)
            : value;
    }
}
