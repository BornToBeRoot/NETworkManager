using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Translators;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters
{
    /// <summary>
    /// Converter to convert <see cref="Protocol"/> to translated <see cref="string"/> or wise versa.
    /// </summary>
    public sealed class DiscoveryProtocolToStringConverter : IValueConverter
    {
        /// <summary>
        /// Method to convert <see cref="Protocol"/> to translated <see cref="string"/>. 
        /// </summary>
        /// <param name="value">Object from type <see cref="Protocol"/>.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Translated <see cref="Protocol"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Protocol discoveryProtocol))
                return "-/-";

            return DiscoveryProtocolTranslator.GetInstance().Translate(discoveryProtocol);
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
