using System;
using System.Globalization;
using System.Windows.Data;
using LiveCharts.Wpf;

namespace NETworkManager.Converters;

public sealed class LvlChartsHeaderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TooltipData info)
            return "-/-";

        var index = info.SharedValue ?? -1;

        return Math.Abs(index - -1) < 0 ? "-/-" : info.XFormatter.Invoke(index);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}