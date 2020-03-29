namespace NETworkManager.Models.PuTTY
{
    /// <summary>
    /// Connection modes for PuTTY.
    /// </summary>
    public enum ConnectionMode
    {
        /// <summary>
        /// SSH connection.
        /// </summary>
        SSH,

        /// <summary>
        /// Telnet connection.
        /// </summary>
        Telnet,

        /// <summary>
        /// Serial connection.
        /// </summary>
        Serial,

        /// <summary>
        /// Rlogin connection.
        /// </summary>
        Rlogin,

        /// <summary>
        /// RAW connection.
        /// </summary>
        RAW
    }
}
