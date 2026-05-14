namespace NETworkManager.Utilities.ActiveDirectory;

/// <summary>
/// Represents a computer account returned from Active Directory LDAP search.
/// </summary>
/// <param name="ProfileName">
///     Suggested profile name derived from the sAMAccountName (trailing '$' stripped),
///     falling back to the CN if sAMAccountName is absent.
/// </param>
/// <param name="DnsHostName">DNS host name of the computer. May be empty if the attribute is not set.</param>
/// <param name="ObjectGuid">
///     String representation of the AD <c>objectGUID</c> (e.g. <c>550e8400-e29b-41d4-a716-446655440000</c>).
///     Stable across renames and OU moves; useful as an idempotency key when re-importing profiles.
///     Empty string if the attribute could not be read.
/// </param>
public readonly record struct ActiveDirectoryComputerRecord(string ProfileName, string DnsHostName, string ObjectGuid);
