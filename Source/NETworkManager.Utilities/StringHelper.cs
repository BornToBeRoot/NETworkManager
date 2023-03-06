using System.Text.RegularExpressions;

namespace NETworkManager.Utilities;

public static class StringHelper
{
    /// <summary>
    /// Method to remove whitespaces from a string.
    /// </summary>
    /// <param name="input">Text with whitespaces.</param>
    /// <returns>Text without whitespaces.</returns>
    public static string RemoveWhitespace(string input)
    {
        return Regex.Replace(input, @"\s+", "");
    }
}
