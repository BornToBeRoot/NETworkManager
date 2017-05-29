using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Models.Settings;

namespace NETworkManager.Converters
{
    public sealed class IsSettingsLocationToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value as string == SettingsManager.SettingsLocation)
                return true;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
