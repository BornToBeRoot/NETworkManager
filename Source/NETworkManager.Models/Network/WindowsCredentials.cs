using System.Security;

namespace NETworkManager.Models.Network;

/// <summary>
///     Windows credentials of a profile / group as kept by an IP Scanner tab for the Windows host information. The
///     password stays protected (<see cref="SecureString" />): it is only copied to unmanaged memory for the logon of a
///     scan, never converted to a managed string.
/// </summary>
/// <param name="Username">Username (also "DOMAIN\user" or "user@domain" with an empty domain).</param>
/// <param name="Domain">Domain, "." for a local account of the host or empty.</param>
/// <param name="Password">Password.</param>
public sealed record WindowsCredentials(string Username, string Domain, SecureString Password)
{
    /// <summary>
    ///     User as shown in the IP Scanner (e.g. "CORP\admin"), without the password.
    /// </summary>
    public string DisplayName
    {
        get
        {
            // As used for the logon (e.g. not "CORP\CORP\admin" for "CORP\admin" with the domain "CORP")
            var (username, domain) = WindowsLogon.SplitUsername(Username, Domain);

            return domain == null ? username : $@"{domain}\{username}";
        }
    }

    /// <summary>
    ///     Local account of the scanned hosts ("." as domain, e.g. ".\admin").
    /// </summary>
    public bool IsLocalAccount => WindowsLogon.SplitUsername(Username, Domain).domain == ".";

    /// <summary>
    ///     Gets the credentials of a local account for one host: its computer name as domain (e.g. "PC01\admin"
    ///     instead of ".\admin"), so the host checks them with its own accounts - never passed to a domain controller.
    /// </summary>
    /// <param name="computerName">NetBIOS name of the host (of its NTLM challenge).</param>
    /// <returns>Credentials for the host (same password), null if they are not a local account.</returns>
    public WindowsCredentials ForHost(string computerName)
    {
        if (!IsLocalAccount || string.IsNullOrEmpty(computerName))
            return null;

        return new WindowsCredentials(WindowsLogon.SplitUsername(Username, Domain).username, computerName, Password);
    }

    /// <summary>
    ///     Without the password.
    /// </summary>
    public override string ToString()
    {
        return DisplayName;
    }
}
