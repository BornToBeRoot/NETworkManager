namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Event arguments when an Error message is returned.
    /// </summary>
    public class DiscoveryProtocolErrorArgs : System.EventArgs
    {
        /// <summary>
        /// Error message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryProtocolErrorArgs"/> class.
        /// </summary>
        public DiscoveryProtocolErrorArgs()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryProtocolWarningArgs"/> class with an error <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Error message.</param>
        public DiscoveryProtocolErrorArgs(string message)
        {
            Message = message;
        }
    }
}
