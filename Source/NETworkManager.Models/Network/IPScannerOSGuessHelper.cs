namespace NETworkManager.Models.Network;

/// <summary>
///     Guesses the operating system of a host from the TTL of its ping reply. Every operating system starts with a
///     typical TTL (64, 128 or 255) and every router on the way reduces it by one - the initial TTL is the smallest
///     typical TTL that is not lower than the received TTL. It's a hint, not a detection: the TTL can be changed on
///     the host, NAT and VPN can change it.
/// </summary>
public static class IPScannerOSGuessHelper
{
    /// <summary>
    ///     Gets the guessed operating system and the number of hops from the TTL of a ping reply.
    /// </summary>
    /// <param name="ttl">TTL of the ping reply, 0 if there is none (e.g. IPv6).</param>
    /// <returns>Guessed operating system, initial TTL and hops (0 if there is no guess).</returns>
    public static (IPScannerOSGuess osGuess, int initialTTL, int hops) GetFromTTL(int ttl)
    {
        var (osGuess, initialTTL) = ttl switch
        {
            <= 0 or > 255 => (IPScannerOSGuess.None, 0),
            <= 64 => (IPScannerOSGuess.LinuxUnix, 64),
            <= 128 => (IPScannerOSGuess.Windows, 128),
            _ => (IPScannerOSGuess.NetworkDevice, 255)
        };

        return osGuess == IPScannerOSGuess.None ? (osGuess, 0, 0) : (osGuess, initialTTL, initialTTL - ttl);
    }
}
