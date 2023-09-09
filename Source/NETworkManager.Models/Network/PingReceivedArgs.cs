using System;

namespace NETworkManager.Models.Network;

/// <summary>
/// Contains the information of a received ping in a <see cref="Ping"/>.
/// </summary>
public class PingReceivedArgs : EventArgs
{
    /// <summary>
    /// Ping information.
    /// </summary>
    public PingInfo Args { get; }

    /// <summary>
    /// Creates a new instance of <see cref="PingReceivedArgs"/> with the given <see cref="PingInfo"/>.
    /// </summary>
    /// <param name="args"></param>
    public PingReceivedArgs(PingInfo args)
    {
        Args = args;
    }
}