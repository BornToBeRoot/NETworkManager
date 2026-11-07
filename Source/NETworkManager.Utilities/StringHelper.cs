using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NETworkManager.Utilities;

public static class StringHelper
{
    /// <summary>
    ///     Method to remove whitespaces from a string.
    /// </summary>
    /// <param name="input">Text with whitespaces.</param>
    /// <returns>Text without whitespaces.</returns>
    public static string RemoveWhitespace(string input)
    {
        return Regex.Replace(input, @"\s+", "");
    }

    /// <summary>
    ///     Removes the control characters (e.g. 0x00, line breaks) of a text received from the network (e.g. a hostname
    ///     or title), so it can be shown and exported (XML doesn't allow most of them).
    /// </summary>
    /// <param name="value">Text, may be null.</param>
    /// <returns>Text without control characters, empty for null.</returns>
    public static string RemoveControlCharacters(string value)
    {
        return string.IsNullOrEmpty(value) ? string.Empty : new string([.. value.Where(c => !char.IsControl(c))]);
    }

    /// <summary>
    ///     Replaces the control characters of a text received from the network with spaces and shows several spaces as
    ///     one (e.g. a multi-line description on one line: "a\r\nb" -> "a b"), without leading / trailing spaces.
    /// </summary>
    /// <param name="value">Text, may be null.</param>
    /// <returns>Text on one line, empty for null.</returns>
    public static string ReplaceControlCharactersWithSpace(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var text = new string([.. value.Select(c => char.IsControl(c) ? ' ' : c)]);

        return string.Join(' ', text.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}