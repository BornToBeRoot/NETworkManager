namespace NETworkManager.Models.PuTTY
{
    /// <summary>
    /// Logging modes for PuTTY.
    /// </summary>
    public enum LogMode
    {
        /// <summary>
        /// All session output will be logged.
        /// </summary>
        SessionLog,

        /// <summary>
        /// SSH packages will be logged.
        /// </summary>
        SSHLog,

        /// <summary>
        /// SSH packages and raw data will be logged.
        /// </summary>
        SSHRawLog
    }
}
