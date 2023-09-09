using System.Xml.Linq;

namespace NETworkManager.Models.Export;

/// <summary>
/// This class will provide methods to export text or objects to files like csv, xml, json or plain text.
/// </summary>
public static partial class ExportManager
{
    /// <summary>
    /// Default declaration for XML documents.
    /// </summary>
    private static readonly XDeclaration DefaultXDeclaration = new("1.0", "utf-8", "yes");

    /// <summary>
    /// Method to export text to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="content">Text to export.</param>
    public static void Export(string filePath, string content)
    {
        CreateTxt(content, filePath);
    }

    /// <summary>
    /// Create a text file from the given content. 
    /// </summary>
    /// <param name="content">Content of the text file.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateTxt(string content, string filePath)
    {
        System.IO.File.WriteAllText(filePath, content);
    }
   
    /// <summary>
    /// Get the file extension as string from the given <see cref="ExportFileType"/>.
    /// </summary>
    /// <param name="fileExtension">File extension as <see cref="ExportFileType"/>.</param>
    /// <returns>File extension as string.</returns>
    public static string GetFileExtensionAsString(ExportFileType fileExtension)
    {
        return fileExtension switch
        {
            ExportFileType.Csv => "CSV",
            ExportFileType.Xml => "XML",
            ExportFileType.Json => "JSON",
            ExportFileType.Txt => "TXT",
            _ => string.Empty,
        };
    }
}
