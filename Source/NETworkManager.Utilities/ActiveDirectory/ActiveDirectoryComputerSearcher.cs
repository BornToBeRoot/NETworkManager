using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Runtime.InteropServices;

namespace NETworkManager.Utilities.ActiveDirectory;

/// <summary>
/// Queries Active Directory for computer accounts under a search base, including subtrees.
/// Supports current-user binds as well as explicit credentials, optional server/port overrides
/// and LDAPS, plus an optional additional filter clause.
/// </summary>
public static class ActiveDirectoryComputerSearcher
{
    private const int LdapPageSize = 500;

    /// <summary>
    ///     Returns computer accounts under <see cref="ActiveDirectorySearchOptions.SearchBase"/> with subtree scope.
    /// </summary>
    /// <exception cref="ArgumentNullException">When <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException">When the search base is missing.</exception>
    /// <exception cref="InvalidOperationException">When the directory search fails.</exception>
    public static IReadOnlyList<ActiveDirectoryComputerRecord> GetComputersInSubtree(ActiveDirectorySearchOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.SearchBase);

        var ldapPath = BuildLdapPath(options);
        var ldapFilter = BuildLdapFilter(options);

        try
        {
            using var directoryEntry = CreateDirectoryEntry(ldapPath, options);
            using var directorySearcher = new DirectorySearcher(directoryEntry);
            directorySearcher.SearchScope = SearchScope.Subtree;
            directorySearcher.Filter = ldapFilter;
            directorySearcher.PageSize = LdapPageSize;
            directorySearcher.Tombstone = false;
            directorySearcher.ClientTimeout = TimeSpan.FromSeconds(30);
            directorySearcher.ServerTimeLimit = TimeSpan.FromSeconds(30);
            directorySearcher.PropertiesToLoad.Add("dnsHostName");
            directorySearcher.PropertiesToLoad.Add("name");
            directorySearcher.PropertiesToLoad.Add("sAMAccountName");
            directorySearcher.PropertiesToLoad.Add("objectGUID");

            var computers = new List<ActiveDirectoryComputerRecord>();

            using var searchResults = directorySearcher.FindAll();
            foreach (SearchResult searchResult in searchResults)
            {
                var dnsHostName = GetFirstPropertyString(searchResult, "dnsHostName");
                var nameAttribute = GetFirstPropertyString(searchResult, "name");
                var samAccountName = GetFirstPropertyString(searchResult, "sAMAccountName");
                var objectGuid = GetObjectGuid(searchResult);

                var profileName = samAccountName.TrimEnd('$');
                if (string.IsNullOrWhiteSpace(profileName))
                    profileName = nameAttribute;

                if (string.IsNullOrWhiteSpace(profileName))
                    continue;

                computers.Add(new ActiveDirectoryComputerRecord(profileName.Trim(), dnsHostName, objectGuid));
            }

            computers.Sort((left, right) =>
                string.Compare(left.ProfileName, right.ProfileName, StringComparison.OrdinalIgnoreCase));

            return computers;
        }
        catch (COMException exception)
        {
            throw new InvalidOperationException(
                $"Active Directory search failed for '{ldapPath}': {exception.Message}",
                exception);
        }
    }

    /// <summary>
    ///     Creates and returns a <see cref="DirectoryEntry"/> bound to <paramref name="ldapPath"/>.
    ///     When <see cref="ActiveDirectorySearchOptions.Username"/> is empty the entry binds with
    ///     the current Windows identity; otherwise it uses explicit credentials with
    ///     <see cref="AuthenticationTypes.Secure"/> (plus <see cref="AuthenticationTypes.SecureSocketsLayer"/>
    ///     when SSL is requested).
    /// </summary>
    private static DirectoryEntry CreateDirectoryEntry(string ldapPath, ActiveDirectorySearchOptions options)
    {
        var authTypes = AuthenticationTypes.Secure;

        if (options.UseSsl)
            authTypes |= AuthenticationTypes.SecureSocketsLayer;

        if (string.IsNullOrEmpty(options.Username))
            return new DirectoryEntry(ldapPath, null, null, authTypes);

        var plainPassword = SecureStringHelper.ConvertToString(options.Password);
        return new DirectoryEntry(ldapPath, options.Username, plainPassword, authTypes);
    }

    /// <summary>
    ///     Builds the LDAP(S) path from <paramref name="options"/>.
    ///     If the search base already starts with a known protocol prefix (<c>LDAP://</c>, <c>LDAPS://</c>,
    ///     <c>GC://</c>) it is returned as-is. Otherwise the protocol is prepended and, when a server is
    ///     specified, the server and port are inserted between the protocol and the search base.
    /// </summary>
    private static string BuildLdapPath(ActiveDirectorySearchOptions options)
    {
        var trimmedBase = options.SearchBase.Trim();

        if (StartsWithProtocol(trimmedBase))
            return trimmedBase;

        var protocol = options.UseSsl ? "LDAPS://" : "LDAP://";

        var server = options.Server?.Trim() ?? string.Empty;
        if (server.Length == 0)
            return protocol + trimmedBase;

        return $"{protocol}{server}:{options.Port}/{trimmedBase}";
    }

    /// <summary>
    ///     Builds the LDAP search filter.  The base filter targets <c>objectCategory=computer</c> and
    ///     <c>objectClass=computer</c>; when <see cref="ActiveDirectorySearchOptions.ExcludeDisabledAccounts"/>
    ///     is set, an additional clause excludes accounts with the <c>ACCOUNTDISABLE</c> UAC bit.
    ///     If an <see cref="ActiveDirectorySearchOptions.AdditionalFilter"/> is supplied it is AND-combined
    ///     with the base filter.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when <see cref="ActiveDirectorySearchOptions.AdditionalFilter"/> is not a valid LDAP
    ///     expression (must start with <c>(</c> and end with <c>)</c>).
    /// </exception>
    private static string BuildLdapFilter(ActiveDirectorySearchOptions options)
    {
        var baseFilter = options.ExcludeDisabledAccounts
            ? "(&(objectCategory=computer)(objectClass=computer)(!(userAccountControl:1.2.840.113556.1.4.803:=2)))"
            : "(&(objectCategory=computer)(objectClass=computer))";

        var additional = options.AdditionalFilter?.Trim();
        if (string.IsNullOrEmpty(additional))
            return baseFilter;

        if (!additional.StartsWith('(') || !additional.EndsWith(')'))
            throw new InvalidOperationException(
                "Additional LDAP filter must be a valid LDAP expression starting with '(' and ending with ')'.");

        return $"(&{baseFilter}{additional})";
    }

    /// <summary>
    ///     Returns <see langword="true"/> when <paramref name="value"/> begins with a recognised
    ///     LDAP protocol prefix (<c>LDAP://</c>, <c>LDAPS://</c>, or <c>GC://</c>), indicating
    ///     that the search base already contains a fully-qualified path.
    /// </summary>
    private static bool StartsWithProtocol(string value)
    {
        return value.StartsWith("LDAP://", StringComparison.OrdinalIgnoreCase) ||
               value.StartsWith("LDAPS://", StringComparison.OrdinalIgnoreCase) ||
               value.StartsWith("GC://", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Returns the string value of the first entry in a <see cref="SearchResult"/> property collection.
    ///     Returns <see cref="string.Empty"/> when the property is absent or has no values.
    /// </summary>
    private static string GetFirstPropertyString(SearchResult searchResult, string propertyName)
    {
        if (!searchResult.Properties.Contains(propertyName) || searchResult.Properties[propertyName].Count == 0)
            return string.Empty;

        return searchResult.Properties[propertyName][0].ToString() ?? string.Empty;
    }

    /// <summary>
    ///     Reads the <c>objectGUID</c> attribute from a <see cref="SearchResult"/> and returns it as a
    ///     lower-case hyphenated GUID string (e.g. <c>550e8400-e29b-41d4-a716-446655440000</c>).
    ///     Returns <see cref="string.Empty"/> when the attribute is absent or not a 16-byte value.
    /// </summary>
    private static string GetObjectGuid(SearchResult searchResult)
    {
        const string attr = "objectGUID";

        if (!searchResult.Properties.Contains(attr) || searchResult.Properties[attr].Count == 0 || searchResult.Properties[attr][0] is not byte[] { Length: 16 } bytes)
            return string.Empty;

        return new Guid(bytes).ToString();
    }
}
