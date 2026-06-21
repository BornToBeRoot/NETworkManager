namespace NETworkManager.Utilities;

public static class PowerShellHelper
{
    /// <summary>
    ///     Escapes a string for safe embedding inside a PowerShell single-quoted string
    ///     by doubling any single-quote characters.
    /// </summary>
    public static string EscapeSingleQuotes(string value) => value.Replace("'", "''");
}
