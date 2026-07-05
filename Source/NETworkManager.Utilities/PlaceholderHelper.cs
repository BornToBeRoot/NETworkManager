using System.Text.RegularExpressions;

namespace NETworkManager.Utilities;

/// <summary>
///     Helper class to resolve <c>{{name}}</c> placeholders in a string.
/// </summary>
public static class PlaceholderHelper
{
    /// <summary>
    ///     Placeholder name for the host (e.g. of a profile).
    /// </summary>
    public const string Host = "host";

    /// <summary>
    ///     Placeholder name for a hostname.
    /// </summary>
    public const string Hostname = "hostname";

    /// <summary>
    ///     Placeholder name for an IP address.
    /// </summary>
    public const string IPAddress = "ipaddress";

    /// <summary>
    ///     Replaces a <c>{{name}}</c> placeholder (case-insensitive) with the given value.
    /// </summary>
    /// <param name="input">Input string that may contain the placeholder.</param>
    /// <param name="name">Name of the placeholder, without the surrounding braces (e.g. <c>"hostname"</c>).</param>
    /// <param name="value">Value to insert in place of the placeholder.</param>
    /// <returns>The input with the placeholder resolved.</returns>
    public static string Replace(string input, string name, string value)
    {
        return string.IsNullOrEmpty(input)
            ? input
            : Regex.Replace(input, $"\\{{\\{{{Regex.Escape(name)}\\}}\\}}", value ?? string.Empty,
                RegexOptions.IgnoreCase);
    }

    /// <summary>
    ///     Replaces multiple <c>{{name}}</c> placeholders (case-insensitive) with the given values.
    /// </summary>
    /// <param name="input">Input string that may contain the placeholders.</param>
    /// <param name="placeholders">Placeholder name/value pairs to resolve, e.g. <c>(PlaceholderHelper.Host, "10.0.0.1")</c>.</param>
    /// <returns>The input with all placeholders resolved.</returns>
    public static string Resolve(string input, params (string name, string value)[] placeholders)
    {
        foreach (var (name, value) in placeholders)
            input = Replace(input, name, value);

        return input;
    }
}
