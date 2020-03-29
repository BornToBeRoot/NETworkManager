namespace NETworkManager.Models.RemoteDesktop
{
    /// <summary>
    /// Represents whether the default audio input is redirected from the client to the remote session.
    /// See also: https://docs.microsoft.com/en-us/windows/win32/termserv/imsrdpclientadvancedsettings7-audiocaptureredirectionmode
    /// </summary>
    public enum AudioCaptureRedirectionMode
    {
        /// <summary>
        /// Redirect the default audio input device to the remote session.
        /// </summary>
        RecordFromThisComputer,

        /// <summary>
        /// Don't redirect the default audio input device to the remote session.
        /// </summary>
        DoNotRecord
    }
}
