namespace NETworkManager.Models.Network;

/// <summary>
///     Operating system of a host guessed from the TTL of its ping reply, see <see cref="IPScannerOSGuessHelper" />.
/// </summary>
public enum IPScannerOSGuess
{
    /// <summary>
    ///     No guess (no ping reply, or no TTL like for IPv6).
    /// </summary>
    None,

    /// <summary>
    ///     Initial TTL 64: Linux, Unix, macOS, Android, iOS.
    /// </summary>
    LinuxUnix,

    /// <summary>
    ///     Initial TTL 128: Windows.
    /// </summary>
    Windows,

    /// <summary>
    ///     Initial TTL 255: network device (e.g. router, switch, printer), Solaris.
    /// </summary>
    NetworkDevice
}
