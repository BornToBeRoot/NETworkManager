using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Translators;
using NETworkManager.Models.PuTTY;

namespace NETworkManager.Converters
{
    /// <summary>
    /// Convert <see cref="LogMode"/> to translated <see cref="string"/> or wise versa.
    /// </summary>
    public sealed class PuTTYLogModeToStringConverter : IValueConverter
    {
        /// <summary>
        /// Convert <see cref="LogMode"/> to translated <see cref="string"/>. 
        /// </summary>
        /// <param name="value">Object from type <see cref="LogMode"/>.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Translated <see cref="LogMode"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is LogMode audioRedirectionMode))
                return "-/-";

            return PuTTYLogModeTranslator.GetInstance().Translate(audioRedirectionMode);
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
