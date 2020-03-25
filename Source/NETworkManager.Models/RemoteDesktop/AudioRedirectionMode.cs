namespace NETworkManager.Models.RemoteDesktop
{
    /// <summary>
    /// Represents the audio redirection mode and different audio redirection options.
    /// See also: https://docs.microsoft.com/en-us/windows/win32/termserv/imsrdpclientadvancedsettings5-audioredirectionmode
    /// </summary>
    public enum AudioRedirectionMode
    {
        /// <summary>
        /// Audio redirection is enabled and the option for redirection is "Bring to this computer". This is the default mode.
        /// </summary>
        PlayOnThisComputer,

        /// <summary>
        /// Audio redirection is enabled and the option is "Leave at remote computer"
        /// </summary>
        PlayOnRemoteComputer,

        /// <summary>
        /// Audio redirection is enabled and the mode is "Do not play".
        /// </summary>
        DoNotPlay
    }
}
