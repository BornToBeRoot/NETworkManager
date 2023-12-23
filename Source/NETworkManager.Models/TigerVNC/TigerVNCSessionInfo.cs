namespace NETworkManager.Models.TigerVNC;

/// <summary>
///     Class contains information's about a TigerVNC session.
/// </summary>
public class TigerVNCSessionInfo
{
    /// <summary>
    ///     Creates a new instance of the <see cref="TigerVNCSessionInfo" /> class.
    /// </summary>
    public TigerVNCSessionInfo()
    {
    }

    /// <summary>
    ///     Path to the TigerVNC executable.
    /// </summary>
    public string ApplicationFilePath { get; set; }

    /// <summary>
    ///     Hostname or IP address to connect to.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    ///     Port used for the connection.
    /// </summary>
    public int Port { get; set; }
}