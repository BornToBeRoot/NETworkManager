namespace NETworkManager.Models.RemoteDesktop
{
    /// <summary>
    /// Specifies the RD Gateway authentication method.
    /// Docs: https://learn.microsoft.com/en-us/windows/win32/termserv/imsrdpclienttransportsettings-gatewayuserselectedcredssource
    /// </summary>
    public enum GatewayUserSelectedCredsSource : uint
    {
        /// <summary>
        /// Use a password (NTLM) as the authentication method for RD Gateway.
        /// </summary>
        Userpass,

        /// <summary>
        /// Use a smart card as the authentication method for RD Gateway.
        /// </summary>
        Smartcard,

        /// <summary>
        /// Use any authentication method for RD Gateway.
        /// </summary>
        Any
    }
}
