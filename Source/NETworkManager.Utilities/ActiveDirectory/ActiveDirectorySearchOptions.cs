using System.Security;

namespace NETworkManager.Utilities.ActiveDirectory;

/// <summary>
///     Options for an Active Directory computer search.
/// </summary>
public sealed class ActiveDirectorySearchOptions
{
    /// <summary>
    /// Distinguished name of the search root (e.g. <c>OU=Servers,DC=contoso,DC=local</c>).
    /// </summary>
    public string SearchBase { get; init; } = string.Empty;

    /// <summary>
    /// Optional domain controller host name. Empty = let DirectoryServices auto-locate.
    /// </summary>
    public string Server { get; init; } = string.Empty;

    /// <summary>
    /// LDAP port. Ignored by the searcher when no <see cref="Server"/> is set (then auto-discovery
    /// via the current domain handles the default port).
    /// </summary>
    public int Port { get; init; } = 389;

    /// <summary>
    /// When true the bind uses <c>LDAPS://</c> with SSL.
    /// </summary>
    public bool UseSsl { get; init; }

    /// <summary>
    /// When true, computer accounts with ACCOUNTDISABLE are skipped server-side.
    /// </summary>
    public bool ExcludeDisabledAccounts { get; init; } = true;

    /// <summary>
    /// Optional username. Empty = bind with current Windows credentials.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Optional password (used together with <see cref="Username" />).
    /// </summary>
    public SecureString Password { get; init; }

    /// <summary>
    /// Optional additional LDAP filter clause that is AND-combined with the base computer filter.
    /// Must be a valid LDAP expression starting with <c>(</c> and ending with <c>)</c>.
    /// </summary>
    public string AdditionalFilter { get; init; } = string.Empty;
}
