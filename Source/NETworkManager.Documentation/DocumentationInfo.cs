namespace NETworkManager.Documentation;

/// <summary>
///     Class to hold information's about a documentation entry.
/// </summary>
public class DocumentationInfo
{
    /// <summary>
    ///     Create an instance of <see cref="DocumentationInfo" /> with all parameters.
    /// </summary>
    /// <param name="identifier"><see cref="DocumentationIdentifier" /> of the documentation entry.</param>
    /// <param name="path"><see cref="Path" /> to the documentation page.</param>
    public DocumentationInfo(DocumentationIdentifier identifier, string path)
    {
        Identifier = identifier;
        Path = path;
    }

    /// <summary>
    ///     Identifier for identifying a documentation entry.
    /// </summary>
    public DocumentationIdentifier Identifier { get; set; }

    /// <summary>
    ///     Path to the documentation, which is combined with the base path of the documentation.
    /// </summary>
    public string Path { get; set; }
}