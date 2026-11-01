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
    public string DisplayName => string.IsNullOrEmpty(Domain) ? Username : $@"{Domain}\{Username}";

    /// <summary>
    ///     Without the password.
    /// </summary>
    public override string ToString()
    {
        return DisplayName;
    }
}
