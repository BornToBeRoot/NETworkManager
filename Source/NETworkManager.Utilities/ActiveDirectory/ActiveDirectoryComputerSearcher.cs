using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Runtime.InteropServices;

namespace NETworkManager.Utilities.ActiveDirectory;

/// <summary>
/// Queries Active Directory for computer accounts under a search base, including subtrees.
/// Uses the current Windows identity to bind to the directory.
/// </summary>
public static class ActiveDirectoryComputerSearcher
{
    private const int LdapPageSize = 500;

    /// <summary>
    /// Returns computer accounts under <paramref name="ldapSearchRoot"/> with subtree scope.
    /// </summary>
    /// <param name="ldapSearchRoot">Distinguished name or LDAP path (with or without LDAP:// prefix).</param>
    /// <param name="excludeDisabledComputerAccounts">When true, computer accounts with ACCOUNTDISABLE are omitted.</param>
    /// <returns>Sorted list by profile name.</returns>
    /// <exception cref="ArgumentException">When <paramref name="ldapSearchRoot"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">When the directory search fails.</exception>
    public static IReadOnlyList<ActiveDirectoryComputerRecord> GetComputersInSubtree(
        string ldapSearchRoot,
        bool excludeDisabledComputerAccounts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ldapSearchRoot);

        var ldapPath = NormalizeLdapPath(ldapSearchRoot.Trim());

        var ldapFilter = excludeDisabledComputerAccounts
            ? "(&(&(objectCategory=computer)(objectClass=computer))(!(userAccountControl:1.2.840.113556.1.4.803:=2)))"
            : "(&(objectCategory=computer)(objectClass=computer))";

        try
        {
            using var directoryEntry = new DirectoryEntry(ldapPath);
            using var directorySearcher = new DirectorySearcher(directoryEntry)
            {
                SearchScope = SearchScope.Subtree,
                Filter = ldapFilter,
                PageSize = LdapPageSize,
                Tombstone = false
            };

            directorySearcher.PropertiesToLoad.Add("dnsHostName");
            directorySearcher.PropertiesToLoad.Add("name");
            directorySearcher.PropertiesToLoad.Add("sAMAccountName");

            var computers = new List<ActiveDirectoryComputerRecord>();

            using var searchResults = directorySearcher.FindAll();
            foreach (SearchResult searchResult in searchResults)
            {
                var dnsHostName = GetFirstPropertyString(searchResult, "dnsHostName");
                var nameAttribute = GetFirstPropertyString(searchResult, "name");
                var samAccountName = GetFirstPropertyString(searchResult, "sAMAccountName");

                var profileName = !string.IsNullOrEmpty(samAccountName)
                    ? samAccountName.TrimEnd('$')
                    : nameAttribute;

                if (string.IsNullOrWhiteSpace(profileName))
                    profileName = nameAttribute;

                if (string.IsNullOrWhiteSpace(profileName))
                    continue;

                computers.Add(new ActiveDirectoryComputerRecord(profileName.Trim(), dnsHostName ?? string.Empty));
            }

            computers.Sort((left, right) =>
                string.Compare(left.ProfileName, right.ProfileName, StringComparison.OrdinalIgnoreCase));

            return computers;
        }
        catch (COMException exception)
        {
            throw new InvalidOperationException(
                "Active Directory search failed. Verify the search base, permissions, and domain connectivity.",
                exception);
        }
    }

    private static string NormalizeLdapPath(string input)
    {
        if (input.StartsWith("LDAP://", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("LDAPS://", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("GC://", StringComparison.OrdinalIgnoreCase))
            return input;

        return "LDAP://" + input;
    }

    private static string GetFirstPropertyString(SearchResult searchResult, string propertyName)
    {
        if (!searchResult.Properties.Contains(propertyName) || searchResult.Properties[propertyName].Count == 0)
            return string.Empty;

        return searchResult.Properties[propertyName][0]?.ToString() ?? string.Empty;
    }
}
