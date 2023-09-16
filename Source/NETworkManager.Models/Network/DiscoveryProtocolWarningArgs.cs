namespace NETworkManager.Models.Network;

/// <summary>
/// Contains the warning message of a <see cref="DiscoveryProtocol"/> warning.
/// </summary>
public class DiscoveryProtocolWarningArgs : System.EventArgs
{
    /// <summary>
    /// Warning message of the <see cref="DiscoveryProtocol"/> warning.
    /// </summary>
    public string Message { get; private set; }

    /// <summary>
    /// Creates a new instance of <see cref="DiscoveryProtocolWarningArgs"/> with the given warning message.
    /// </summary>
    /// <param name="message">Warning message of the <see cref="DiscoveryProtocol"/> warning.</param>
    public DiscoveryProtocolWarningArgs(string message)
    {
        Message = message;
    }
}
