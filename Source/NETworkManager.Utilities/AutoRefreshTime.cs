using System;
using System.Collections.Generic;

namespace NETworkManager.Utilities
{
    /// <summary>
    /// Class provides methods to manage auto refresh time.
    /// </summary>
    public static class AutoRefreshTime
    {
        /// <summary>
        /// Method returns a list with default <see cref="AutoRefreshTimeInfo"/>s.
        /// </summary>
        public static IEnumerable<AutoRefreshTimeInfo> GetDefaults => new List<AutoRefreshTimeInfo>
        {
            new AutoRefreshTimeInfo(5, TimeUnit.Second),
            new AutoRefreshTimeInfo(15, TimeUnit.Second),
            new AutoRefreshTimeInfo(30, TimeUnit.Second),
            new AutoRefreshTimeInfo(1, TimeUnit.Minute),
            new AutoRefreshTimeInfo(5, TimeUnit.Minute),
        };

        /// <summary>
        /// Method to calculate a <see cref="TimeSpan"/> based on <see cref="AutoRefreshTimeInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="AutoRefreshTimeInfo"/> to calculate <see cref="TimeSpan"/></param>
        /// <returns>Returns the calculated <see cref="TimeSpan"/>.</returns>
        public static TimeSpan CalculateTimeSpan(AutoRefreshTimeInfo info)
        {
            switch (info.TimeUnit)
            {
                // Calculate the seconds
                case TimeUnit.Second:
                    return new TimeSpan(0, 0, info.Value);
                // Calculate the minutes
                case TimeUnit.Minute:
                    return new TimeSpan(0, info.Value * 60, 0);
                // Calculate the hours
                case TimeUnit.Hour:
                    return new TimeSpan(info.Value * 60, 0, 0);
                case TimeUnit.None:
                default:
                    throw new Exception("Wrong time unit.");
            }
        }              
    }
}