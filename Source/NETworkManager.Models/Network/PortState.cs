namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Represents the state of a port.
    /// </summary>
    public enum PortState
    {
        /// <summary>
        /// Port is not defined.
        /// </summary>
        None,
        
        /// <summary>
        /// Port is open.
        /// </summary>
        Open,

        /// <summary>
        /// Port is closed.
        /// </summary>
        Closed,

        /// <summary>
        /// Port has timed out.
        /// </summary>
        TimedOut
    }
}
