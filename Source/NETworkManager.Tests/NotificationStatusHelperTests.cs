using NETworkManager.Models;
using Xunit;

namespace NETworkManager.Tests;

public class NotificationStatusHelperTests
{
    [Theory]
    [InlineData(1, 0, 0, NotificationStatus.Up)]
    [InlineData(5, 0, 0, NotificationStatus.Up)]
    [InlineData(0, 1, 0, NotificationStatus.Down)]
    [InlineData(1, 1, 0, NotificationStatus.UpAndDown)]
    [InlineData(2, 3, 1, NotificationStatus.UpAndDown)]
    [InlineData(0, 0, 1, NotificationStatus.Changed)]
    [InlineData(1, 0, 1, NotificationStatus.Changed)]
    [InlineData(0, 1, 1, NotificationStatus.Changed)]
    [InlineData(0, 0, 0, NotificationStatus.Changed)]
    public void GetsTheStatusOfTheChanges(int additions, int removals, int otherChanges, NotificationStatus expected)
    {
        Assert.Equal(expected, NotificationStatusHelper.FromChanges(additions, removals, otherChanges));
    }
}
