using NETworkManager.Utilities;
using Xunit;

namespace NETworkManager.Tests.Utilities;

public class PortRangeHelperTests
{
    [Theory]
    [InlineData(1, true)]
    [InlineData(22, true)]
    [InlineData(65535, true)]
    [InlineData(0, false)]
    [InlineData(65536, false)]
    [InlineData(-1, false)]
    public void IsValidPort(int port, bool isValid)
    {
        Assert.Equal(isValid, PortRangeHelper.IsValidPort(port));
    }

    [Theory]
    [InlineData(80, 90, true)]
    [InlineData(1, 65535, true)]
    [InlineData(90, 80, false)]
    [InlineData(80, 80, false)]
    [InlineData(0, 80, false)]
    [InlineData(80, 65536, false)]
    public void IsValidPortRange(int startPort, int endPort, bool isValid)
    {
        Assert.Equal(isValid, PortRangeHelper.IsValidPortRange(startPort, endPort));
    }
}
