using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Translators;
using NETworkManager.Models.Appearance;

namespace NETworkManager.Converters
{
    /// <summary>
    /// Convert <see cref="ThemeColorInfo"/> to translated <see cref="string"/> or wise versa.
    /// </summary>
    public sealed class ThemeToStringConverter : IValueConverter
    {

        /// <summary>
        /// Convert <see cref="ThemeColorInfo"/> to translated <see cref="string"/>. 
        /// </summary>
        /// <param name="value">Object from type <see cref="ThemeColorInfo"/>.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Translated <see cref="ThemeColorInfo"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string theme))
                return "-/-";

            return ThemeTranslator.GetInstance().Translate(theme);
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
