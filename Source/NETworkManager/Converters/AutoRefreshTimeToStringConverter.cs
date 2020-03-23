using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Utilities;
using NETworkManager.Localization;

namespace NETworkManager.Converters
{
    public sealed class AutoRefreshTimeToStringConverter : IValueConverter
    {        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TimeUnit timeUnit))
                return "No valid time unit passed!";

            string timeUnitTranslated = Localization.LanguageFiles.Strings.ResourceManager.GetString("TimeUnit_" + timeUnit, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(timeUnitTranslated) ? timeUnit.ToString() : timeUnitTranslated;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}