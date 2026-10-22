namespace NETworkManager;

/// <summary>
///     Part of the syntax hint in the run command dialog (e.g. "[DNS server]").
/// </summary>
/// <param name="Text">Text of the part.</param>
/// <param name="IsCurrent">The part is being typed (highlighted).</param>
public sealed record RunCommandHintPart(string Text, bool IsCurrent);

/// <summary>
///     Value of the input as it was understood (e.g. "DNS server: 1.1.1.1"), shown below the input.
/// </summary>
/// <param name="Label">Name of the value (e.g. "DNS server").</param>
/// <param name="Value">Value (e.g. "1.1.1.1").</param>
public sealed record RunCommandChip(string Label, string Value);
