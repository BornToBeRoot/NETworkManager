using System;

namespace NETworkManager.Models.Network;

/// <summary>
///     Contains the error message of a <see cref="LLDPCDPProtocol" /> error.
/// </summary>
public class LLDPCDPErrorArgs : EventArgs
{
    /// <summary>
    ///     Creates a new instance of <see cref="LLDPCDPErrorArgs" /> with the given error message.
    /// </summary>
    /// <param name="errorMessage">Error message of the <see cref="LLDPCDPProtocol" /> error.</param>
    public LLDPCDPErrorArgs(string errorMessage)
    {
        Message = errorMessage;
    }

    /// <summary>
    ///     Error message of the <see cref="LLDPCDPProtocol" /> error.
    /// </summary>
    public string Message { get; private set; }
}