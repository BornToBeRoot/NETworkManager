using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using NETworkManager.Models.Lookup;
using NETworkManager.Models.Network;

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


    private static void CreateTxt(string content, string filePath)
    {
        System.IO.File.WriteAllText(filePath, content);
    }
   
    public static string GetFileExtensionAsString(ExportFileType fileExtension)
    {
        return fileExtension switch
        {
            ExportFileType.CSV => "CSV",
            ExportFileType.XML => "XML",
            ExportFileType.JSON => "JSON",
            ExportFileType.TXT => "TXT",
            _ => string.Empty,
        };
    }
}
