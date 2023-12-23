using System;

namespace NETworkManager.Utilities;

public class LvlChartsDefaultInfo
{
    public LvlChartsDefaultInfo(DateTime dateTime, double value)
    {
        DateTime = dateTime;
        Value = value;
    }

    public DateTime DateTime { get; }
    public double Value { get; set; }
}