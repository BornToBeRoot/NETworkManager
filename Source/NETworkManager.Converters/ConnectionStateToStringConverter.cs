using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Translators;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters
{
    /// <summary>
    /// Converter to convert <see cref="ConnectionState"/> to translated <see cref="string"/> or wise versa.
    /// </summary>
    public sealed class ConnectionStateToStringConverter : IValueConverter
    {
        /// <summary>
        /// Method to convert <see cref="ConnectionState"/> to translated <see cref="string"/>. 
        /// </summary>
        /// <param name="value">Object from type <see cref="ConnectionState"/>.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Translated <see cref="ConnectionState"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ConnectionState connectionState))
                return "-/-";
            
            return ConnectionStateTranslator.GetInstance().Translate(connectionState);
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