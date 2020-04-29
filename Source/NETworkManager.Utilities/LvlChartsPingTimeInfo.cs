using System;
namespace NETworkManager.Utilities
{
    public class LvlChartsPingTimeInfo
    {
        public DateTime DateTime { get; }
        public double Value { get; set; }

        public LvlChartsPingTimeInfo(DateTime dateTime, double value)
        {
            DateTime = dateTime;
            Value = value;
        }
    }
}
