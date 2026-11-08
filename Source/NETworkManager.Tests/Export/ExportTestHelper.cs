using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NETworkManager.Models.Export;
using Newtonsoft.Json.Linq;
using Xunit;

namespace NETworkManager.Tests.Export;

/// <summary>
///     Helpers to test the exports of <see cref="ExportManager" />: export to a temporary file, parse the CSV / XML / JSON
///     file and check the format rules shared by all tools.
/// </summary>
internal static class ExportTestHelper
{
    /// <summary>
    ///     Exported files of one export (CSV, XML and JSON) - created with the de-DE culture (decimal comma), so a value
    ///     formatted with the current culture would break the CSV file.
    /// </summary>
    internal sealed record ExportFiles(string Csv, string Xml, string Json)
    {
        /// <summary>CSV header (column names).</summary>
        public IReadOnlyList<string> CsvHeader => ParseCsvLine(CsvLines[0]);

        /// <summary>CSV lines without the header, parsed into the values.</summary>
        public IReadOnlyList<IReadOnlyList<string>> CsvRows => CsvLines.Skip(1).Select(ParseCsvLine).ToList();

        /// <summary>Raw CSV lines incl. the header.</summary>
        public IReadOnlyList<string> CsvLines =>
            Csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>Elements of the exported objects in the XML file.</summary>
        public IReadOnlyList<XElement> XmlItems => XDocument.Parse(Xml).Root!.Elements().Single().Elements().ToList();

        /// <summary>Exported objects in the JSON file.</summary>
        public IReadOnlyList<JObject> JsonItems => JArray.Parse(Json).Cast<JObject>().ToList();
    }

    /// <summary>
    ///     Exports with every file type (CSV, XML, JSON) using the de-DE culture.
    /// </summary>
    /// <param name="export">Export method (file path, file type), e.g. <c>(f, t) =&gt; ExportManager.Export(f, t, items)</c>.</param>
    public static ExportFiles ExportAll(Action<string, ExportFileType> export)
    {
        var culture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            return new ExportFiles(Export(export, ExportFileType.Csv), Export(export, ExportFileType.Xml),
                Export(export, ExportFileType.Json));
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    /// <summary>
    ///     Exports to a temporary file and returns its content.
    /// </summary>
    private static string Export(Action<string, ExportFileType> export, ExportFileType fileType)
    {
        var filePath = Path.GetTempFileName();

        try
        {
            export(filePath, fileType);

            return File.ReadAllText(filePath);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    ///     Checks the rules every export must follow: the expected columns (names and order), the same names in the
    ///     CSV header, the XML elements and the JSON keys, and one CSV value per column in every row.
    /// </summary>
    /// <param name="files">Exported files.</param>
    /// <param name="expectedHeader">Expected CSV header (released columns must not move).</param>
    public static void AssertConsistent(ExportFiles files, string expectedHeader)
    {
        var header = expectedHeader.Split(',');

        Assert.Equal(header, files.CsvHeader);

        foreach (var row in files.CsvRows)
            Assert.Equal(header.Length, row.Count);

        foreach (var item in files.XmlItems)
            Assert.Equal(header, item.Elements().Select(x => x.Name.LocalName));

        foreach (var item in files.JsonItems)
            Assert.Equal(header, item.Properties().Select(x => x.Name));
    }

    /// <summary>
    ///     Gets the value of a column of the first CSV row.
    /// </summary>
    public static string CsvValue(this ExportFiles files, string column)
    {
        var index = files.CsvHeader.ToList().IndexOf(column);

        Assert.True(index >= 0, $"Column {column} not found");

        return files.CsvRows[0][index];
    }

    /// <summary>
    ///     Gets the value of an element of the first XML item.
    /// </summary>
    public static string XmlValue(this ExportFiles files, string element)
    {
        return files.XmlItems[0].Element(element)!.Value;
    }

    /// <summary>
    ///     Gets the value of a key of the first JSON item.
    /// </summary>
    public static JToken JsonValue(this ExportFiles files, string key)
    {
        return files.JsonItems[0][key]!;
    }

    /// <summary>
    ///     Parses a CSV line (RFC 4180: quoted values may contain commas and doubled quotes).
    /// </summary>
    public static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var value = new StringBuilder();
        var isQuoted = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (isQuoted)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                {
                    value.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    isQuoted = false;
                }
                else
                {
                    value.Append(c);
                }
            }
            else if (c == '"')
            {
                isQuoted = true;
            }
            else if (c == ',')
            {
                values.Add(value.ToString());
                value.Clear();
            }
            else
            {
                value.Append(c);
            }
        }

        values.Add(value.ToString());

        return values;
    }
}
