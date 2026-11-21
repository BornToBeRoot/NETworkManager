using System;
using System.Collections.Generic;

namespace NETworkManager.Utilities;

/// <summary>
///     Compares DNS names like DNS does: ASCII letters case-insensitively, all other characters (e.g. UTF-8 labels of
///     mDNS) exactly (RFC 4343, RFC 6762 16). Not <see cref="StringComparer.OrdinalIgnoreCase" /> or
///     <see cref="string.ToLowerInvariant" />: they also fold other letters, so e.g. "Büro" and "BÜRO" or the
///     Kelvin sign and "k" would be the same name.
/// </summary>
public sealed class DNSNameComparer : IEqualityComparer<string>
{
    /// <summary>
    ///     Gets the comparer.
    /// </summary>
    public static DNSNameComparer Instance { get; } = new();

    private DNSNameComparer()
    {
    }

    /// <summary>
    ///     Converts the ASCII letters of a name to lowercase (other characters are kept), e.g. for a key.
    /// </summary>
    /// <param name="name">DNS name.</param>
    /// <returns>Name with lowercase ASCII letters.</returns>
    public static string ToLower(string name)
    {
        return string.Create(name.Length, name, static (span, value) =>
        {
            for (var i = 0; i < value.Length; i++)
                span[i] = ToLower(value[i]);
        });
    }

    /// <summary>
    ///     Checks if a name ends with a suffix (e.g. a service type), see <see cref="DNSNameComparer" />.
    /// </summary>
    /// <param name="name">DNS name.</param>
    /// <param name="suffix">Suffix.</param>
    /// <returns>True if the name ends with the suffix.</returns>
    public static bool EndsWith(string name, string suffix)
    {
        return name.Length >= suffix.Length && SpanEquals(name.AsSpan(name.Length - suffix.Length), suffix);
    }

    /// <inheritdoc />
    public bool Equals(string x, string y)
    {
        if (ReferenceEquals(x, y))
            return true;

        return x != null && y != null && x.Length == y.Length && SpanEquals(x, y);
    }

    /// <inheritdoc />
    public int GetHashCode(string obj)
    {
        var hash = new HashCode();

        foreach (var c in obj)
            hash.Add(ToLower(c));

        return hash.ToHashCode();
    }

    private static bool SpanEquals(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
    {
        for (var i = 0; i < x.Length; i++)
        {
            if (ToLower(x[i]) != ToLower(y[i]))
                return false;
        }

        return true;
    }

    private static char ToLower(char c) => char.IsAsciiLetterUpper(c) ? (char)(c | 0x20) : c;
}
