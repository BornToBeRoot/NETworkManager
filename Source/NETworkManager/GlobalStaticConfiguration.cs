using System;

namespace NETworkManager
{
    public class GlobalStaticConfiguration
    {
        // Average type speed --> 187 chars/min
        public static TimeSpan SearchDispatcherTimerTimeSpan => new TimeSpan(0, 0, 0, 0, 750);
        public static TimeSpan CredentialsUILockTime => new TimeSpan(0, 0, 120);
    }
}