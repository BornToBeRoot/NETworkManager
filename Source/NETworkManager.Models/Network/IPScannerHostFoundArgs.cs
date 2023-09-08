namespace NETworkManager.Models.Network;

/// <summary>
/// Contains the information of a host found in a <see cref="IPScanner"/>.
/// </summary>
public class IPScannerHostFoundArgs : System.EventArgs
{
    /// <summary>
    /// IP Scanner host information.
    /// </summary>
    public IPScannerHostInfo Args { get; } 

    /// <summary>
    /// Creates a new instance of <see cref="IPScannerHostFoundArgs"/> with the given <see cref="IPScannerHostInfo"/>.
    /// </summary>
    /// <param name="args">IP Scanner host information.</param>
    public IPScannerHostFoundArgs(IPScannerHostInfo args)
    {
        Args = args;
    }
}
