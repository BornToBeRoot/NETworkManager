namespace NETworkManager.Models.PuTTY;

/// <summary>
/// Class contains information's about a PuTTY session.
/// </summary>
public class PuTTYSessionInfo
{
    /// <summary>
    /// Path to the PuTTY executable.
    /// </summary>
    public string ApplicationFilePath { get; set; }

    /// <summary>
    /// Mode (SSH, Telnet, etc.), which is used to establish the connection.
    /// </summary>
    public ConnectionMode Mode { get; init; }

    /// <summary>
    /// Hostname or SerialLine. Depends on the <see cref="ConnectionMode"/>.
    /// </summary>
    public string HostOrSerialLine { get; init; }

    /// <summary>
    /// Port or Baud. Depends on the <see cref="ConnectionMode"/>.
    /// </summary>
    public int PortOrBaud { get; init; }

    /// <summary>
    /// Username for login.
    /// </summary>
    public string Username { get; init; }

    /// <summary>
    /// Path to the private key.
    /// </summary>
    public string PrivateKey { get; init; }

    /// <summary>
    /// PuTTY profile to use.
    /// </summary>
    public string Profile { get; init; }

    /// <summary>
    /// PuTTY host key. Multiple keys are separated by a comma.
    /// </summary>
    public string Hostkey { get; init; }

    /// <summary>
    /// Enables session log.
    /// </summary>
    public bool EnableLog { get; init; }

    /// <summary>
    /// PuTTY log mode.
    /// </summary>
    public LogMode LogMode { get; init; }

    /// <summary>
    /// Path to the PuTTY log files like "C:\temp".
    /// </summary>
    public string LogPath { get; init; }

    /// <summary>
    /// Filename of the PuTTY log like "PuTTY.log".
    /// </summary>
    public string LogFileName { get; init; }

    /// <summary>
    /// Additional command line argument. Everything putty can handle.
    /// </summary>
    public string AdditionalCommandLine { get; init; }

    /// <summary>
    /// Create an instance of <see cref="PuTTYSessionInfo"/>.
    /// </summary>
    public PuTTYSessionInfo()
    {

    } 
}
