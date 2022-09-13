namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Event arguments when a Warning message is returned.
    /// </summary>
    public class DiscoveryProtocolWarningArgs : System.EventArgs
    {
        /// <summary>
        /// Warning message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryProtocolWarningArgs"/> class.
        /// </summary>
        public DiscoveryProtocolWarningArgs()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryProtocolWarningArgs"/> class with a warning <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Warning message.</param>
        public DiscoveryProtocolWarningArgs(string message)
        {
            Message = message;
        }
    }
}
