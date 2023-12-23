using System;

namespace NETworkManager.Models.Network;

/// <summary>
///     Contains the information of a scanned port in a <see cref="PortScanner" />.
/// </summary>
public class PortScannerPortScannedArgs : EventArgs
{
    /// <summary>
    ///     Creates a new instance of <see cref="PortScannerPortScannedArgs" /> with the given
    ///     <see cref="PortScannerPortInfo" />.
    /// </summary>
    /// <param name="args">Port scanner port information.</param>
    public PortScannerPortScannedArgs(PortScannerPortInfo args)
    {
        Args = args;
    }

    /// <summary>
    ///     Port scanner port information.
    /// </summary>
    public PortScannerPortInfo Args { get; }
}