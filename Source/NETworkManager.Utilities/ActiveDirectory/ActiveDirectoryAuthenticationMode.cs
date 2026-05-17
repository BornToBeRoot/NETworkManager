namespace NETworkManager.Utilities.ActiveDirectory;

/// <summary>
///     How the LDAP bind authenticates against the directory.
/// </summary>
public enum ActiveDirectoryAuthenticationMode
{
    /// <summary>
    /// Bind with the current Windows identity (no explicit credentials).
    /// </summary>
    CurrentUser,

    /// <summary>
    /// Bind with an explicit username and password.
    /// </summary>
    Custom
}