using NETworkManager.Models.Settings;
using System;
using System.Globalization;
using System.Windows.Data;
using static NETworkManager.Utilities.AutoRefreshTime;

namespace NETworkManager.Converters
{
    public sealed class AutoRefreshTimeToStringConverter : IValueConverter
    {        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TimeUnit timeUnit))
                return "No valid time unit passed!";

            var timeUnitTranslated = LocalizationManager.GetStringByKey("String_TimeUnit_" + timeUnit);

            return string.IsNullOrEmpty(timeUnitTranslated) ? timeUnit.ToString() : timeUnitTranslated;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}