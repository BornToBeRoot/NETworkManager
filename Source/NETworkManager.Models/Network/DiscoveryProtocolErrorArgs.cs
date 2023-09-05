namespace NETworkManager.Models.Network;

/// <summary>
/// Contains the error message of a <see cref="DiscoveryProtocol"/> error.
/// </summary>
public class DiscoveryProtocolErrorArgs : System.EventArgs
{
    /// <summary>
    /// Error message of the <see cref="DiscoveryProtocol"/> error.
    /// </summary>
    public string Message { get; private set; }

    /// <summary>
    ///Creates a new instance of <see cref="DiscoveryProtocolErrorArgs"/> with the given error message.
    /// </summary>
    /// <param name="errorMessage">Error message of the <see cref="DiscoveryProtocol"/> error.</param>
    public DiscoveryProtocolErrorArgs(string errorMessage)
    {
        Message = errorMessage;
    }
}
