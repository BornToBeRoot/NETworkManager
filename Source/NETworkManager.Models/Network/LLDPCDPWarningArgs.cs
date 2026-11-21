using System;

namespace NETworkManager.Models.Network;

/// <summary>
///     Contains the warning message of a <see cref="LLDPCDPProtocol" /> warning.
/// </summary>
public class LLDPCDPWarningArgs : EventArgs
{
    /// <summary>
    ///     Creates a new instance of <see cref="LLDPCDPWarningArgs" /> with the given warning message.
    /// </summary>
    /// <param name="message">Warning message of the <see cref="LLDPCDPProtocol" /> warning.</param>
    public LLDPCDPWarningArgs(string message)
    {
        Message = message;
    }

    /// <summary>
    ///     Warning message of the <see cref="LLDPCDPProtocol" /> warning.
    /// </summary>
    public string Message { get; private set; }
}