using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;

namespace NETworkManager.Models.RDAP;

/// <summary>
///     Public suffix list (ICANN section, see Scripts/Create-PublicSuffixListFromWeb.ps1) to get the registered domain of
///     a hostname (e.g. "bbc.co.uk" for "www.bbc.co.uk"). Registries only know registered domains, not their
///     subdomains or public suffixes like "co.uk".
/// </summary>
public static class PublicSuffixList
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(PublicSuffixList));

    private const string Resource = "NETworkManager.Models.Resources.PublicSuffixList.dat";

    private static readonly Lazy<(HashSet<string> Rules, HashSet<string> Wildcards, HashSet<string> Exceptions)>
        List = new(Load);

    /// <summary>
    ///     Gets the registered domain (public suffix plus one label) of a domain name, following the algorithm of
    ///     https://publicsuffix.org/list/: an exception rule wins, otherwise the longest matching rule, otherwise the
    ///     TLD ("*").
    /// </summary>
    /// <param name="domain">Domain name in lowercase A-label form (e.g. "www.bbc.co.uk").</param>
    /// <returns>Registered domain (e.g. "bbc.co.uk"), or null if the domain is a public suffix itself.</returns>
    public static string GetRegisteredDomain(string domain)
    {
        var (rules, wildcards, exceptions) = List.Value;

        var labels = domain.Split('.');

        // Index of the first label of the public suffix, the TLD by default.
        var suffixIndex = labels.Length - 1;

        for (var i = 0; i < labels.Length; i++)
        {
            var name = string.Join('.', labels[i..]);

            if (exceptions.Contains(name))
            {
                suffixIndex = i + 1;
                break;
            }

            if (rules.Contains(name) || i + 1 < labels.Length && wildcards.Contains(string.Join('.', labels[(i + 1)..])))
            {
                suffixIndex = i;
                break;
            }
        }

        return suffixIndex > 0 ? string.Join('.', labels[(suffixIndex - 1)..]) : null;
    }

    private static (HashSet<string>, HashSet<string>, HashSet<string>) Load()
    {
        HashSet<string> rules = new(StringComparer.Ordinal);
        HashSet<string> wildcards = new(StringComparer.Ordinal);
        HashSet<string> exceptions = new(StringComparer.Ordinal);

        try
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Resource);

            if (stream == null)
                return (rules, wildcards, exceptions);

            using var reader = new StreamReader(stream);

            while (reader.ReadLine() is { } line)
            {
                if (line.Length == 0 || line.StartsWith("//"))
                    continue;

                if (line.StartsWith("*."))
                    wildcards.Add(line[2..]);
                else if (line.StartsWith('!'))
                    exceptions.Add(line[1..]);
                else
                    rules.Add(line);
            }
        }
        catch (Exception ex)
        {
            // Without the list, the TLD is used as public suffix (default rule).
            Log.Error("Could not load the public suffix list.", ex);
        }

        return (rules, wildcards, exceptions);
    }
}
