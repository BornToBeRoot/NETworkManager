using NETworkManager.Utilities;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class LvlChartsPingTimeValueConverter : IValueConverter
    {
 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">ChartPoint.Instance (object)</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is LvlChartsDefaultInfo info)
                return $"{info.Value} ms";

            return "-/-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
