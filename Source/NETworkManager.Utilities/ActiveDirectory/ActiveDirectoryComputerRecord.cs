namespace NETworkManager.Utilities.ActiveDirectory;

/// <summary>
/// Represents a computer account returned from Active Directory LDAP search.
/// </summary>
/// <param name="ProfileName">Display name for the profile (typically sAMAccountName without trailing '$').</param>
/// <param name="DnsHostName">DNS host name used for RDP when present.</param>
public readonly record struct ActiveDirectoryComputerRecord(string ProfileName, string DnsHostName);
