using System;
using System.Collections.Generic;

namespace NETworkManager.Utilities
{
    public class AutoRefreshTime
    {
        public static List<AutoRefreshTimeInfo> Defaults
        {
            get
            {
                return new List<AutoRefreshTimeInfo>()
                {
                    new AutoRefreshTimeInfo(5, TimeUnit.Second),
                    new AutoRefreshTimeInfo(15, TimeUnit.Second),
                    new AutoRefreshTimeInfo(30, TimeUnit.Second),
                    new AutoRefreshTimeInfo(1, TimeUnit.Minute),
                    new AutoRefreshTimeInfo(5, TimeUnit.Minute),
                };
            }
        }

        public static TimeSpan CalculateTimeSpan(AutoRefreshTimeInfo info)
        {
            // Seconds
            if (info.TimeUnit == TimeUnit.Second) 
                return new TimeSpan(0, 0, info.Value);

            // Minutes
            else if (info.TimeUnit == TimeUnit.Minute)
                return new TimeSpan(0, info.Value * 60, 0);

            // Hours
            else
                return new TimeSpan(info.Value * 60, 0, 0);
        }


        public enum TimeUnit
        {
            Second,
            Minute,
            Hour
        }
    }
}