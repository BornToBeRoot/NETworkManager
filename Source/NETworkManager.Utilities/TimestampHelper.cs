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
    /// Extracts the timestamp from a filename that starts with a timestamp prefix.
    /// </summary>
    /// <remarks>Filenames are expected to start with yyyyMMddHHmmss format (14 characters). 
    /// This method extracts the timestamp portion and parses it as a DateTime. 
    /// If the timestamp cannot be parsed, DateTime.MinValue is returned.</remarks>
    /// <param name="filePath">The full path to the file or just the filename.</param>
    /// <returns>The timestamp extracted from the filename, or DateTime.MinValue if parsing fails.</returns>
    public static DateTime ExtractTimestampFromFilename(string filePath)
    {
        try
        {
            var fileName = Path.GetFileName(filePath);
            
            // Extract the timestamp prefix (yyyyMMddHHmmss format, 14 characters)
            if (fileName.Length >= 14)
            {
                var timestampString = fileName.Substring(0, 14);
                
                // Parse the timestamp
                if (DateTime.TryParseExact(timestampString, "yyyyMMddHHmmss", 
                    CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, out var timestamp))
                {
                    return timestamp;
                }
            }
        }
        catch (ArgumentException)
        {
            // If any error occurs, return MinValue to sort this file as oldest
        }

        return DateTime.MinValue;
    }
}