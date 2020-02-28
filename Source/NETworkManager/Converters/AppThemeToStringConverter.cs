using MahApps.Metro;
using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization;

namespace NETworkManager.Converters
{
    public sealed class AppThemeToStringConverter : IValueConverter
    {
        /* Translate the name of the app theme */
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is AppTheme theme))
                return "No valid theme passed!";

            var name = Localization.LanguageFiles.Strings.ResourceManager.GetString(theme.Name, LocalizationManager.GetInstance().Culture);

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
