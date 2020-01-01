using LiveCharts.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class LvlChartsHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TooltipData info)
            {
                double index = info.SharedValue ?? -1;

                if (index == -1)
                    return "-/-";

                return info.XFormatter.Invoke(index) as string;
            }

            return "-/-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
