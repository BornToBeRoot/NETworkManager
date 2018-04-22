using MahApps.Metro;
using NETworkManager.Models.Settings;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class AppThemeToStringConverter : IValueConverter
    {
        /* Translate the name of the app theme */
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AppTheme theme = value as AppTheme;

            string name = LocalizationManager.GetStringByKey("String_AppTheme_" + theme.Name);

            if (string.IsNullOrEmpty(name))
                name = theme.Name;

            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
