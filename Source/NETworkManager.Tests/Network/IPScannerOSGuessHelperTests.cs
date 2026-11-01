using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class IPScannerOSGuessHelperTests
{
    [Theory]
    [InlineData(64, IPScannerOSGuess.LinuxUnix, 64, 0)]
    [InlineData(61, IPScannerOSGuess.LinuxUnix, 64, 3)]
    [InlineData(1, IPScannerOSGuess.LinuxUnix, 64, 63)]
    [InlineData(128, IPScannerOSGuess.Windows, 128, 0)]
    [InlineData(127, IPScannerOSGuess.Windows, 128, 1)]
    [InlineData(65, IPScannerOSGuess.Windows, 128, 63)]
    [InlineData(255, IPScannerOSGuess.NetworkDevice, 255, 0)]
    [InlineData(250, IPScannerOSGuess.NetworkDevice, 255, 5)]
    [InlineData(129, IPScannerOSGuess.NetworkDevice, 255, 126)]
    public void GetFromTTL(int ttl, IPScannerOSGuess expectedOSGuess, int expectedInitialTTL, int expectedHops)
    {
        var (osGuess, initialTTL, hops) = IPScannerOSGuessHelper.GetFromTTL(ttl);

        Assert.Equal(expectedOSGuess, osGuess);
        Assert.Equal(expectedInitialTTL, initialTTL);
        Assert.Equal(expectedHops, hops);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(256)]
    public void GetFromTTL_NoTTL_NoGuess(int ttl)
    {
        Assert.Equal((IPScannerOSGuess.None, 0, 0), IPScannerOSGuessHelper.GetFromTTL(ttl));
    }
}
