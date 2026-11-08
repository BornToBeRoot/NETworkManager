using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using System.Xml.Linq;
using NETworkManager.Utilities;
using Newtonsoft.Json;

namespace NETworkManager.Models.Export;

/// <summary>
///     This class will provide methods to export text or objects to files like csv, xml, json or plain text.
/// </summary>
/// <remarks>
///     Every exported type defines its columns once as a list of <see cref="ExportField{T}" /> (e.g.
///     <c>PingInfoExportFields</c>), the CSV, XML and JSON files are written from this list by
///     <see cref="Export{T}" />. The values have the same format in every tool and are never translated:
///     <list type="bullet">
///         <item>Names: CSV column = XML element = JSON key, in the same order. The columns are in a logical order
///         (like shown in the tool, related values next to each other), a new column is added where it belongs - read
///         them by name, not by position.</item>
///         <item>Empty value (<c>null</c>): empty in CSV, empty element in XML, <c>null</c> in JSON.</item>
///         <item>Numbers with a decimal point (invariant culture), without a unit (e.g. ping time in ms, offset in
///         s - the unit is documented). Bools as <c>True</c> / <c>False</c> (CSV), <c>true</c> / <c>false</c> (XML).
///         JSON: numbers and bools as JSON numbers / bools, everything else as text.</item>
///         <item>Date and time as shown in the tool (local time; SNTP Lookup: UTC) as <c>yyyy-MM-dd HH:mm:ss.fff</c>,
///         durations as numbers in a fixed unit (see above; a time span only as fallback:
///         <c>[-][d.]hh:mm:ss[.fffffff]</c>).</item>
///         <item>Enums as their (English) name (e.g. <c>Success</c>, flags: <c>ICMP, TCP</c>).</item>
///         <item>MAC addresses as <c>AA:BB:CC:DD:EE:FF</c>.</item>
///         <item>Lists (e.g. IP addresses) as one text, separated by <c>; </c>.</item>
///         <item>CSV: text that may contain a comma (e.g. hostname, description) is always quoted, any other value
///         only if it contains a comma, a quote or a line break (RFC 4180).</item>
///     </list>
/// </remarks>
public static partial class ExportManager
{
    /// <summary>
    ///     Default declaration for XML documents.
    /// </summary>
    private static readonly XDeclaration DefaultXDeclaration = new("1.0", "utf-8", "yes");

    /// <summary>
    ///     Format of a date and time in the exported files.
    /// </summary>
    private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

    /// <summary>
    ///     Separator of the values of a list (e.g. IP addresses) in the exported files.
    /// </summary>
    private const string ListSeparator = "; ";

    /// <summary>
    ///     Method to export text to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="content">Text to export.</param>
    public static void Export(string filePath, string content)
    {
        File.WriteAllText(filePath, content);
    }

    /// <summary>
    ///     Get the file extension as string from the given <see cref="ExportFileType" />.
    /// </summary>
    /// <param name="fileExtension">File extension as <see cref="ExportFileType" />.</param>
    /// <returns>File extension as string.</returns>
    public static string GetFileExtensionAsString(ExportFileType fileExtension)
    {
        return fileExtension switch
        {
            ExportFileType.Csv => "CSV",
            ExportFileType.Xml => "XML",
            ExportFileType.Json => "JSON",
            ExportFileType.Txt => "TXT",
            _ => string.Empty
        };
    }

    #region Fields

    /// <summary>
    ///     Exported column (CSV) / element (XML) / key (JSON) of an object.
    /// </summary>
    /// <typeparam name="T">Type of the exported objects.</typeparam>
    /// <param name="Name">Name of the column / element / key.</param>
    /// <param name="GetValue">
    ///     Gets the value of an object (e.g. a string, a number, a bool, an enum, an IP address, a date or a list),
    ///     <c>null</c> for an empty value. Formatted by the writer, see <see cref="ExportManager" />.
    /// </param>
    /// <param name="IsQuoted">The value is always quoted in the CSV file (text that may contain a comma).</param>
    private sealed record ExportField<T>(string Name, Func<T, object> GetValue, bool IsQuoted = false);

    /// <summary>
    ///     Value with a list of objects that have fields of their own (e.g. the ports of a host): in the CSV file the
    ///     values of an item are separated by <c>/</c> and the items by <c>;</c>, in the XML file every item is an
    ///     element (<paramref name="ItemName" />), in the JSON file an array of objects.
    /// </summary>
    /// <typeparam name="TItem">Type of the items.</typeparam>
    /// <param name="ItemName">Name of the XML element of an item.</param>
    /// <param name="Items">Items to export.</param>
    /// <param name="Fields">Exported fields of an item.</param>
    private sealed record ExportList<TItem>(
        string ItemName,
        IEnumerable<TItem> Items,
        IReadOnlyList<ExportField<TItem>> Fields) : IExportList
    {
        /// <inheritdoc />
        public IEnumerable<IEnumerable<(string Name, object Value)>> GetItems()
        {
            return Items.Select(item => Fields.Select(field => (field.Name, field.GetValue(item))));
        }
    }

    /// <summary>
    ///     Non-generic access to an <see cref="ExportList{TItem}" /> for the writers.
    /// </summary>
    private interface IExportList
    {
        /// <summary>
        ///     Name of the XML element of an item.
        /// </summary>
        string ItemName { get; }

        /// <summary>
        ///     Gets the names and values of the fields of every item.
        /// </summary>
        IEnumerable<IEnumerable<(string Name, object Value)>> GetItems();
    }

    #endregion

    #region Writer

    /// <summary>
    ///     Exports the objects with the given fields to a CSV, XML or JSON file.
    /// </summary>
    /// <typeparam name="T">Type of the exported objects.</typeparam>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects to export.</param>
    /// <param name="fields">Exported fields (columns) of an object.</param>
    /// <param name="applicationName">Tool of the objects (root element of the XML file).</param>
    private static void Export<T>(string filePath, ExportFileType fileType, IEnumerable<T> collection,
        IReadOnlyList<ExportField<T>> fields, ApplicationName applicationName)
    {
        switch (fileType)
        {
            case ExportFileType.Csv:
                CreateCsv(filePath, collection, fields);
                break;
            case ExportFileType.Xml:
                CreateXml(filePath, collection, fields, applicationName);
                break;
            case ExportFileType.Json:
                CreateJson(filePath, collection, fields);
                break;
            case ExportFileType.Txt:
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    ///     Creates a CSV file: header with the names of the fields, one line per object.
    /// </summary>
    private static void CreateCsv<T>(string filePath, IEnumerable<T> collection, IReadOnlyList<ExportField<T>> fields)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(string.Join(",", fields.Select(field => field.Name)));

        foreach (var info in collection)
            stringBuilder.AppendLine(string.Join(",", fields.Select(field =>
            {
                var fieldValue = field.GetValue(info);

                var value = fieldValue is IExportList list
                    ? string.Join(";", list.GetItems().Select(item => string.Join("/", item.Select(x => ToText(x.Value)))))
                    : ToText(fieldValue);

                return field.IsQuoted || IsQuoteRequired(value) ? CsvHelper.QuoteString(value) : value;
            })));

        File.WriteAllText(filePath, stringBuilder.ToString());
    }

    /// <summary>
    ///     Creates a XML file: <c>&lt;Tool&gt;&lt;Types&gt;&lt;Type&gt;</c> with one element per field.
    /// </summary>
    private static void CreateXml<T>(string filePath, IEnumerable<T> collection, IReadOnlyList<ExportField<T>> fields,
        ApplicationName applicationName)
    {
        var name = typeof(T).Name;

        var document = new XDocument(DefaultXDeclaration,
            new XElement(applicationName.ToString(),
                new XElement(name + "s",
                    from info in collection
                    select new XElement(name,
                        from field in fields
                        from element in ToXmlElements(field.Name, field.GetValue(info))
                        select element))));

        document.Save(filePath);
    }

    /// <summary>
    ///     Gets the XML element(s) of a value: one element, or one per item of an <see cref="IExportList" /> (no element
    ///     for the list itself).
    /// </summary>
    private static IEnumerable<XElement> ToXmlElements(string name, object value)
    {
        if (value is IExportList list)
            return list.GetItems().Select(item =>
                new XElement(list.ItemName, item.Select(x => new XElement(x.Name, ToXmlValue(x.Value)))));

        return [new XElement(name, ToXmlValue(value))];
    }

    /// <summary>
    ///     Gets the value of a XML element: bools as XML bools (<c>true</c> / <c>false</c>), everything else as text.
    /// </summary>
    private static object ToXmlValue(object value)
    {
        return value is bool ? value : ToText(value);
    }

    /// <summary>
    ///     Creates a JSON file: array with one object per exported object.
    /// </summary>
    private static void CreateJson<T>(string filePath, IEnumerable<T> collection,
        IReadOnlyList<ExportField<T>> fields)
    {
        var jsonData = collection
            .Select(info => ToJsonObject(fields.Select(field => (field.Name, field.GetValue(info)))))
            .ToArray();

        File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }

    /// <summary>
    ///     Gets the JSON object of the values (names in the order of the fields).
    /// </summary>
    private static Dictionary<string, object> ToJsonObject(IEnumerable<(string Name, object Value)> values)
    {
        var jsonObject = new Dictionary<string, object>();

        foreach (var (name, value) in values)
            jsonObject.Add(name, value switch
            {
                IExportList list => list.GetItems().Select(ToJsonObject).ToArray(),
                null or bool => value,
                _ when IsNumber(value) => value,
                _ => ToText(value)
            });

        return jsonObject;
    }

    #endregion

    #region Format

    /// <summary>
    ///     Formats a value as text for the CSV / XML file (and non-number values in the JSON file), see
    ///     <see cref="ExportManager" />.
    /// </summary>
    /// <param name="value">Value to format.</param>
    /// <returns>Formatted value, <c>null</c> for an empty value.</returns>
    private static string ToText(object value)
    {
        return value switch
        {
            null => null,
            string text => text,
            DateTime dateTime => dateTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
            TimeSpan timeSpan => timeSpan.ToString("c", CultureInfo.InvariantCulture),
            PhysicalAddress physicalAddress => ToText(physicalAddress),
            // Empty list: empty value
            IEnumerable list => list.Cast<object>().Select(ToText).ToList() is { Count: > 0 } items
                ? string.Join(ListSeparator, items)
                : null,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }

    /// <summary>
    ///     Formats a MAC address as <c>AA:BB:CC:DD:EE:FF</c> (any length, e.g. 8 bytes of a tunnel adapter).
    /// </summary>
    /// <param name="physicalAddress">MAC address to format.</param>
    /// <returns>Formatted MAC address, <c>null</c> if it is empty.</returns>
    private static string ToText(PhysicalAddress physicalAddress)
    {
        var bytes = physicalAddress.GetAddressBytes();

        return bytes.Length == 0 ? null : string.Join(":", bytes.Select(x => x.ToString("X2")));
    }

    /// <summary>
    ///     Checks if a value is a number (exported as JSON number).
    /// </summary>
    private static bool IsNumber(object value)
    {
        return value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal
            or BigInteger;
    }

    /// <summary>
    ///     Checks if a CSV value must be quoted (RFC 4180): it contains a comma, a quote or a line break.
    /// </summary>
    private static bool IsQuoteRequired(string value)
    {
        return value != null && value.IndexOfAny([',', '"', '\r', '\n']) >= 0;
    }

    /// <summary>
    ///     Checks if a ping got a reply (time, TTL and bytes are only set with a reply).
    /// </summary>
    /// <param name="status">Status of the ping.</param>
    /// <returns>True if the ping got a reply (also <see cref="IPStatus.TtlExpired" /> of a traceroute hop).</returns>
    private static bool HasPingReply(IPStatus status)
    {
        return status is IPStatus.Success or IPStatus.TtlExpired;
    }

    #endregion
}
