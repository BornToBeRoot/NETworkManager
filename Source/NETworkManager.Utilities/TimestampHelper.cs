using System;
using System.Globalization;
using System.IO;

namespace NETworkManager.Utilities;

public static class TimestampHelper
{
    public static string GetTimestamp()
    {
        return DateTime.Now.ToString("yyyyMMddHHmmss");
    }

    /// <summary>
    /// Generates a filename by prefixing the specified filename with a timestamp string.
    /// </summary>
    /// <param name="fileName">The original filename to be prefixed with a timestamp. Cannot be null or empty.</param>
    /// <returns>A string containing the timestamp followed by an underscore and the original filename.</returns>
    public static string GetTimestampFilename(string fileName)
    {
        return $"{GetTimestamp()}_{fileName}";
    }

    /// <summary>
    /// Determines whether the specified file name begins with a valid timestamp in the format "yyyyMMddHHmmss".
    /// </summary>
    /// <remarks>This method checks only the first 14 characters of the file name for a valid timestamp and
    /// does not validate the remainder of the file name.</remarks>
    /// <param name="fileName">The file name to evaluate. The file name is expected to start with a 14-digit timestamp followed by an
    /// underscore and at least one additional character.</param>
    /// <returns>true if the file name starts with a valid timestamp in the format "yyyyMMddHHmmss"; otherwise, false.</returns>
    public static bool IsTimestampedFilename(string fileName)
    {
        // Ensure the filename is long enough to contain a timestamp, an underscore, and at least one character after it
        if (fileName.Length < 16)
            return false;

        var timestampString = fileName.Substring(0, 14);

        return DateTime.TryParseExact(timestampString, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }

    /// <summary>
    /// Extracts the timestamp from a filename that starts with a timestamp prefix.
    /// </summary>
    /// <remarks>Filenames are expected to start with yyyyMMddHHmmss format (14 characters). 
    /// This method extracts the timestamp portion and parses it as a DateTime. 
    /// If the timestamp cannot be parsed, DateTime.MinValue is returned.</remarks>
    /// <param name="fileName">The full path to the file or just the filename.</param>
    /// <returns>The timestamp extracted from the filename, or DateTime.MinValue if parsing fails.</returns>
    public static DateTime ExtractTimestampFromFilename(string fileName)
    {
        // Extract the timestamp prefix (yyyyMMddHHmmss format, 14 characters)
        var timestampString = fileName.Substring(0, 14);

        return DateTime.ParseExact(timestampString, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
    }
}