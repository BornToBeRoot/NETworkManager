namespace NETworkManager.Utilities;

public static class CsvHelper
{
    /// <summary>
    ///     Wraps a string value in double quotes and escapes any internal double quotes
    ///     per RFC 4180 (doubles them). Null values are returned as an empty quoted field.
    /// </summary>
    /// <param name="value">The string value to quote.</param>
    /// <returns>The value wrapped in double quotes with internal quotes escaped.</returns>
    public static string QuoteString(string? value)
    {
        return $"\"{value?.Replace("\"", "\"\"")}\"";
    }
}