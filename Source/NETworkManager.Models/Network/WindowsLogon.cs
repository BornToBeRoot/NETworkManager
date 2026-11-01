using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace NETworkManager.Models.Network;

/// <summary>
///     Network logon with other Windows credentials, like <c>runas /netonly</c>: <c>LogonUser</c> with
///     <c>LOGON32_LOGON_NEW_CREDENTIALS</c> creates a token that is a copy of the current user locally and uses the
///     credentials only for outgoing network connections (SMB / RPC). No privileges or admin rights are needed. The
///     credentials are not checked by the logon: a wrong password is reported by the host (e.g.
///     <c>ERROR_LOGON_FAILURE</c>). Unlike <c>WNetAddConnection2</c>, no SMB session of the user is created or
///     changed (no conflict with mapped drives).
/// </summary>
public sealed class WindowsLogon : IDisposable
{
    private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
    private const int LOGON32_PROVIDER_WINNT50 = 3;

    private readonly SafeAccessTokenHandle _token;

    private WindowsLogon(SafeAccessTokenHandle token)
    {
        _token = token;
    }

    /// <summary>
    ///     Creates the network logon. The password is copied to unmanaged memory for the call only (zeroed and freed
    ///     afterwards), it is never converted to a managed string.
    /// </summary>
    /// <param name="credentials">Credentials.</param>
    /// <returns>Logon, dispose it when it is no longer used.</returns>
    /// <exception cref="Win32Exception">The logon failed (e.g. invalid username).</exception>
    public static WindowsLogon Create(WindowsCredentials credentials)
    {
        var (username, domain) = SplitUsername(credentials.Username, credentials.Domain);

        var password = credentials.Password != null
            ? Marshal.SecureStringToGlobalAllocUnicode(credentials.Password)
            : Marshal.StringToHGlobalUni(string.Empty);

        try
        {
            if (!LogonUser(username, domain, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_WINNT50,
                    out var token))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return new WindowsLogon(token);
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(password);
        }
    }

    /// <summary>
    ///     Runs a (blocking) function with the network logon on the current thread. Never use it around an
    ///     <c>await</c>: the impersonation belongs to the thread.
    /// </summary>
    /// <param name="func">Function, e.g. NetApi calls to a host.</param>
    /// <typeparam name="T">Result.</typeparam>
    /// <returns>Result of the function.</returns>
    public T Run<T>(Func<T> func)
    {
        return WindowsIdentity.RunImpersonated(_token, func);
    }

    /// <summary>
    ///     Splits the username into user and domain for <c>LogonUser</c>: "DOMAIN\user" with an empty domain,
    ///     "user@domain" (UPN) needs no domain (null).
    /// </summary>
    /// <param name="username">Username.</param>
    /// <param name="domain">Domain, may be empty.</param>
    /// <returns>Username and domain (null for a UPN without domain).</returns>
    public static (string username, string domain) SplitUsername(string username, string domain)
    {
        username ??= string.Empty;

        if (!string.IsNullOrEmpty(domain))
            return (username, domain);

        var index = username.IndexOf('\\');

        if (index > 0)
            return (username[(index + 1)..], username[..index]);

        // UPN (user@domain) or a user without domain: null, LogonUser takes the UPN as it is
        return (username, null);
    }

    public void Dispose()
    {
        _token.Dispose();
    }

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "LogonUserW")]
    private static extern bool LogonUser(string lpszUsername, string lpszDomain, IntPtr lpszPassword,
        int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);
}
