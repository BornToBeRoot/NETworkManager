using MahApps.Metro;
using NETworkManager.Localization;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    /// <summary>
    /// Converter to convert <see cref="Accent"/> to translated <see cref="string"/> or wise versa.
    /// </summary>
    public sealed class AccentToStringConverter : IValueConverter
    {
        /// <summary>
        /// Method to convert <see cref="Accent"/> to translated <see cref="string"/>. 
        /// </summary>
        /// <param name="value">Object from type <see cref="Accent"/>.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Translated <see cref="Accent"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Accent accent))
                return "No valid accent passed!";

            var name = Localization.LanguageFiles.Strings.ResourceManager.GetString(accent.Name, LocalizationManager.GetInstance().Culture);

            if (string.IsNullOrEmpty(name))
                name = accent.Name;

            return name;
        }

        /// <summary>
        /// !!! Method not implemented !!!
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
