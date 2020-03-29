using MahApps.Metro;
using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Translators;

namespace NETworkManager.Converters
{
    /// <summary>
    /// Convert <see cref="AppTheme"/> to translated <see cref="string"/> or wise versa.
    /// </summary>
    public sealed class AppThemeToStringConverter : IValueConverter
    {
        /// <summary>
        /// Convert <see cref="AppTheme"/> to translated <see cref="string"/>. 
        /// </summary>
        /// <param name="value">Object from type <see cref="AppTheme"/>.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Translated <see cref="AppTheme"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is AppTheme appTheme))
                return "-/-";

            return AppThemeTranslator.GetInstance().Translate(appTheme);
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
