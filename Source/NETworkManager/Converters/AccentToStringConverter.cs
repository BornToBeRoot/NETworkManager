using MahApps.Metro;
using NETworkManager.Localization;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class AccentToStringConverter : IValueConverter
    {
        /* Translate the name of the accent */
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Accent accent))
                return "No valid accent passed!";

            var name = Localization.LanguageFiles.Strings.ResourceManager.GetString(accent.Name, LocalizationManager.GetInstance().Culture);

            if (string.IsNullOrEmpty(name))
                name = accent.Name;

            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
