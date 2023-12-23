using System;

namespace NETworkManager.Utilities;

/// <summary>
///     Exception which is thrown when the <see cref="DNSClient" /> is not configured.
/// </summary>
public class DNSClientNotConfiguredException : Exception
{
    /// <summary>
    ///     Create an instance of <see cref="DNSClientNotConfiguredException" />.
    /// </summary>
    public DNSClientNotConfiguredException()
    {
    }

    /// <summary>
    ///     Create an instance of<see cref="DNSClientNotConfiguredException" /> with parameters.
    /// </summary>
    /// <param name="message">Message that is shown, if the <see cref="DNSClient" /> is not configured.</param>
    public DNSClientNotConfiguredException(string message) : base(message)
    {
    }
}