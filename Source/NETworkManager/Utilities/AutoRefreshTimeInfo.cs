using static NETworkManager.Utilities.AutoRefreshTime;

namespace NETworkManager.Utilities
{
    class AutoRefreshTimeInfo
    {
        public int Value { get; set; }
        public TimeUnit TimeUnit { get; set; }

        public AutoRefreshTimeInfo()
        {

        }

        public AutoRefreshTimeInfo(int value, TimeUnit timenUnit)
        {
            Value = value;
            TimeUnit = timenUnit;
        }
    }
}
