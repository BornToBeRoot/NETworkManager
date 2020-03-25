using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Data;
using NETworkManager.Localization.Translators;

namespace NETworkManager.Converters
{
    /// <summary>
    /// Convert <see cref="TcpState"/> to translated <see cref="string"/> or wise versa.
    /// </summary>
    public sealed class TcpStateToStringConverter : IValueConverter
    {

        /// <summary>
        /// Convert <see cref="TcpState"/> to translated <see cref="string"/>. 
        /// </summary>
        /// <param name="value">Object from type <see cref="TcpState"/>.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Translated <see cref="TcpState"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TcpState tcpState))
                return "-/-";

            return TcpStateTranslator.GetInstance().Translate(tcpState);
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
