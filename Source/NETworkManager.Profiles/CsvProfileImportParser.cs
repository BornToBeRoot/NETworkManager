using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NETworkManager.Profiles;

/// <summary>
///     Parses a CSV file into <see cref="ProfileImportCandidate" />s.
///     Expected format: <c>Name;Host</c> with an optional third <c>Description</c> column.
///     The delimiter (<c>;</c>, <c>,</c> or tab) is auto-detected and an optional header row is skipped.
/// </summary>
public static class CsvProfileImportParser
{
    private static readonly char[] SupportedDelimiters = [';', ',', '\t'];

    /// <summary>
    ///     Reads the given CSV file and returns one candidate per usable row.
    /// </summary>
    /// <param name="filePath">Path to the CSV file.</param>
    /// <param name="fallbackDescription">
    ///     Description used when a row does not provide its own (third column). May be empty.
    /// </param>
    public static IReadOnlyList<ProfileImportCandidate> Parse(string filePath, string fallbackDescription = null)
    {
        var candidates = new List<ProfileImportCandidate>();

        var lines = File.ReadAllLines(filePath);

        var delimiter = DetectDelimiter(lines);
        var headerChecked = false;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = ParseLine(line, delimiter);

            var name = fields.Count > 0 ? fields[0].Trim() : string.Empty;
            var host = fields.Count > 1 ? fields[1].Trim() : string.Empty;
            var description = fields.Count > 2 ? fields[2].Trim() : string.Empty;

            // Skip an optional header row (only checked on the first non-empty line).
            if (!headerChecked)
            {
                headerChecked = true;

                if (IsHeaderRow(name, host))
                    continue;
            }

            // Ignore completely empty rows.
            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(host))
                continue;

            // Fall back to the host as name so we never create a nameless profile.
            if (string.IsNullOrEmpty(name))
                name = host;

            candidates.Add(new ProfileImportCandidate(
                name: name,
                host: host,
                description: !string.IsNullOrEmpty(description) ? description : fallbackDescription,
                importSource: ProfileImportSource.Csv,
                importSourceId: BuildImportSourceId(name, host)));
        }

        return candidates;
    }

    /// <summary>
    ///     Builds a stable duplicate-detection key from the import source and a hash of name + host.
    /// </summary>
    private static string BuildImportSourceId(string name, string host)
    {
        var raw = $"Csv|{name.Trim().ToLowerInvariant()}|{host.Trim().ToLowerInvariant()}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));

        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    ///     Picks the delimiter that occurs most often across the first non-empty lines.
    /// </summary>
    private static char DetectDelimiter(IReadOnlyList<string> lines)
    {
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var bestDelimiter = SupportedDelimiters[0];
            var bestCount = 0;

            foreach (var delimiter in SupportedDelimiters)
            {
                var count = 0;

                foreach (var c in line)
                    if (c == delimiter)
                        count++;

                if (count <= bestCount)
                    continue;

                bestCount = count;
                bestDelimiter = delimiter;
            }

            return bestDelimiter;
        }

        return SupportedDelimiters[0];
    }

    /// <summary>
    ///     Detects whether the first row is a header (e.g. <c>Name;Host</c>).
    /// </summary>
    private static bool IsHeaderRow(string firstField, string secondField)
    {
        return string.Equals(firstField, "Name", StringComparison.OrdinalIgnoreCase) &&
               (string.Equals(secondField, "Host", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(secondField, "Host/IP", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(secondField, "Hostname", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(secondField, "IP", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Splits a single CSV line by the given delimiter, honoring double-quoted fields.
    /// </summary>
    private static List<string> ParseLine(string line, char delimiter)
    {
        var fields = new List<string>();
        var builder = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    // Escaped quote ("") inside a quoted field.
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        builder.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    builder.Append(c);
                }
            }
            else if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == delimiter)
            {
                fields.Add(builder.ToString());
                builder.Clear();
            }
            else
            {
                builder.Append(c);
            }
        }

        fields.Add(builder.ToString());

        return fields;
    }
}
