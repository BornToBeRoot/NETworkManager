using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Translators;
using NETworkManager.Models.RemoteDesktop;

namespace NETworkManager.Converters
{
    /// <summary>
    /// Converter to convert <see cref="NetworkConnectionType"/> to translated <see cref="string"/> or wise versa.
    /// </summary>
    public sealed class RemoteDesktopNetworkConnectionTypeToStringConverter : IValueConverter
    {
        /// <summary>
        /// Method to convert <see cref="NetworkConnectionType"/> to translated <see cref="string"/>. 
        /// </summary>
        /// <param name="value">Object from type <see cref="NetworkConnectionType"/>.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Translated <see cref="NetworkConnectionType"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is NetworkConnectionType networkConnectionType))
                return "-/-";

            return RemoteDesktopNetworkConnectionTypeTranslator.GetInstance().Translate(networkConnectionType);
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
