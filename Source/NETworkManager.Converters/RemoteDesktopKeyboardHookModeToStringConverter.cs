using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Translators;
using NETworkManager.Models.RemoteDesktop;

namespace NETworkManager.Converters
{
    /// <summary>
    /// Converter to convert <see cref="KeyboardHookMode"/> to translated <see cref="string"/> or wise versa.
    /// </summary>
    public sealed class RemoteDesktopKeyboardHookModeToStringConverter : IValueConverter
    {
        /// <summary>
        /// Method to convert <see cref="KeyboardHookMode"/> to translated <see cref="string"/>. 
        /// </summary>
        /// <param name="value">Object from type <see cref="KeyboardHookMode"/>.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Translated <see cref="KeyboardHookMode"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is KeyboardHookMode keyboardHookMode))
                return "-/-";

            return RemoteDesktopKeyboardHookModeTranslator.GetInstance().Translate(keyboardHookMode);                        
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
