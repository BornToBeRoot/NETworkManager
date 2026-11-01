using System;
using System.Collections.Generic;

namespace NETworkManager.Utilities;

/// <summary>
///     Class provides static methods to manage auto refresh time.
/// </summary>
public static class AutoRefreshTime
{
    /// <summary>
    ///     Method returns a list with default <see cref="AutoRefreshTimeInfo" />s.
    /// </summary>
    public static IEnumerable<AutoRefreshTimeInfo> GetDefaults => new List<AutoRefreshTimeInfo>
    {
        new(5, TimeUnit.Second),
        new(15, TimeUnit.Second),
        new(30, TimeUnit.Second),
        new(1, TimeUnit.Minute),
        new(5, TimeUnit.Minute)
    };

    /// <summary>
    ///     Method to calculate a <see cref="TimeSpan" /> based on <see cref="AutoRefreshTimeInfo" />.
    /// </summary>
    /// <param name="info"><see cref="AutoRefreshTimeInfo" /> to calculate <see cref="TimeSpan" /></param>
    /// <returns>Returns the calculated <see cref="TimeSpan" />.</returns>
    public static TimeSpan CalculateTimeSpan(AutoRefreshTimeInfo info)
    {
        return info.TimeUnit switch
        {
            TimeUnit.Second => TimeSpan.FromSeconds(info.Value),
            TimeUnit.Minute => TimeSpan.FromMinutes(info.Value),
            TimeUnit.Hour => TimeSpan.FromHours(info.Value),
            _ => throw new Exception("Wrong time unit.")
        };
    }
}