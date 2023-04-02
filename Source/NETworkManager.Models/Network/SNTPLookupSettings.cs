namespace NETworkManager.Models.Network;

/// <summary>
/// Class contains the settings for the SNTP lookup.
/// </summary>
public class SNTPLookupSettings
{
    /// <summary>
    /// Timeout in milliseconds after which the request is aborted.
    /// </summary>
    public int Timeout { get; set; }

    /// <summary>
    /// Create an instance of <see cref="SNTPLookupSettings"/> with parameters.
    /// </summary>
    /// <param name="timeout">Timeout in milliseconds after which the request is aborted.</param>
    public SNTPLookupSettings(int timeout)
    {
        Timeout = timeout;
    }
}
