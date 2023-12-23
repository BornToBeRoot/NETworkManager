using System;

namespace NETworkManager.Models.Network;

/// <summary>
///     Contains the information of an SNTP lookup result in a <see cref="SNTPLookup" />.
/// </summary>
public class SNTPLookupResultArgs : EventArgs
{
    /// <summary>
    ///     Creates a new instance of <see cref="SNTPLookupResultArgs" /> with the given <see cref="SNTPLookupInfo" />.
    /// </summary>
    /// <param name="args">SNTP lookup information.</param>
    public SNTPLookupResultArgs(SNTPLookupInfo args)
    {
        Args = args;
    }

    /// <summary>
    ///     SNTP lookup information.
    /// </summary>
    public SNTPLookupInfo Args { get; }
}