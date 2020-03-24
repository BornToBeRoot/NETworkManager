namespace NETworkManager.Models.RemoteDesktop
{
    /// <summary>
    /// Enum indicates the keyboard redirection settings, which specify how and when to apply Windows keyboard shortcut (for example, ALT+TAB).
    /// See also: https://docs.microsoft.com/en-us/windows/win32/termserv/imsrdpclientsecuredsettings-keyboardhookmode
    /// </summary>
    public enum KeyboardHookMode
    {
        /// <summary>
        /// Apply key combinations only locally at the client computer.
        /// </summary>
        OnThisComputer,

        /// <summary>
        /// Apply key combinations at the remote server.
        /// </summary>
        OnTheRemoteComputer,

        // Apply key combinations to the remote server only when the client is running in full-screen mode.
        //OnlyWhenUsingTheFullScreen
    }
}
