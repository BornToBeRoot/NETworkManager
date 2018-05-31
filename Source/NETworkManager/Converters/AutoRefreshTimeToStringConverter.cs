using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
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
            TimeUnit timeUnit = (TimeUnit)value;

            string timeUnitTranslated = LocalizationManager.GetStringByKey("String_TimeUnit_" + timeUnit.ToString());

            if (string.IsNullOrEmpty(timeUnitTranslated))
                return timeUnit.ToString();

            return timeUnitTranslated;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
