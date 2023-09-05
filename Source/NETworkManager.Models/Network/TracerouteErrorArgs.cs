namespace NETworkManager.Models.Network;

/// <summary>
/// Contains the error message of a <see cref="Traceroute"/> error.
/// </summary>
public class TracerouteErrorArgs : System.EventArgs
{
    /// <summary>
    /// Error message of the <see cref="Traceroute"/> error.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="TracerouteErrorArgs"/> with the given error message.
    /// </summary>
    /// <param name="errorMessage">Error message of the <see cref="Traceroute"/> error.</param>
    public TracerouteErrorArgs(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}
