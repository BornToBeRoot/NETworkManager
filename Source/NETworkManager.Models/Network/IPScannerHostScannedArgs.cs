using System;

namespace NETworkManager.Models.Network;

/// <summary>
///     Contains the information of a scanned host in a <see cref="IPScanner" />.
/// </summary>
public class IPScannerHostScannedArgs : EventArgs
{
    /// <summary>
    ///     Creates a new instance of <see cref="IPScannerHostScannedArgs" /> with the given <see cref="IPScannerHostInfo" />.
    /// </summary>
    /// <param name="args">IP Scanner host information.</param>
    public IPScannerHostScannedArgs(IPScannerHostInfo args)
    {
        Args = args;
    }

    /// <summary>
    ///     IP Scanner host information.
    /// </summary>
    public IPScannerHostInfo Args { get; }
}