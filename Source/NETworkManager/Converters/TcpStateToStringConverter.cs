using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Data;
using NETworkManager.Localization.Translators;

namespace NETworkManager.Converters
{
    public sealed class TcpStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TcpState s))
                return "-/-";

            return TcpStateTranslator.GetInstance().Translate(s.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
