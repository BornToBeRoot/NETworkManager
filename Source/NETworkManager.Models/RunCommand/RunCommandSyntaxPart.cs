namespace NETworkManager.Models.RunCommand;

/// <summary>
///     Part of the syntax of a command (e.g. "&lt;Hosts&gt;" or "[DNS server]"), shown in the hint of the run command
///     dialog.
/// </summary>
/// <param name="Kind">What the part is.</param>
/// <param name="Name">Name of the value (e.g. "DNS server"), also the label of the parsed value.</param>
/// <param name="Text">Text in the hint (e.g. "[DNS server]").</param>
public sealed record RunCommandSyntaxPart(RunCommandSyntaxPartKind Kind, string Name, string Text)
{
    /// <summary>
    ///     Creates a required part (e.g. "&lt;Hosts&gt;").
    /// </summary>
    public static RunCommandSyntaxPart Required(RunCommandSyntaxPartKind kind, string name)
    {
        return new RunCommandSyntaxPart(kind, name, $"<{name}>");
    }

    /// <summary>
    ///     Creates an optional part (e.g. "[DNS server]").
    /// </summary>
    public static RunCommandSyntaxPart Optional(RunCommandSyntaxPartKind kind, string name)
    {
        return new RunCommandSyntaxPart(kind, name, $"[{name}]");
    }

    /// <summary>
    ///     Creates the profile part ("@&lt;Profile&gt;").
    /// </summary>
    public static RunCommandSyntaxPart Profile(string name)
    {
        return new RunCommandSyntaxPart(RunCommandSyntaxPartKind.Profile, name, $"@<{name}>");
    }
}
