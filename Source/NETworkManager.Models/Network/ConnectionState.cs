namespace NETworkManager.Models.Network
{ 
    /// <summary>
    /// Enum indicates the state of a connection.
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// Connection has no state.
        /// </summary>
        None,

        /// <summary>
        /// Connection is OK.
        /// </summary>
        OK,

        /// <summary>
        /// Connection is warning.
        /// </summary>
        Warning,

        /// <summary>
        /// Connection is critical.
        /// </summary>
        Critical
    }
}
