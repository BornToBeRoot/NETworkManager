using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Translators;
using static NETworkManager.Models.Network.DiscoveryProtocol;

namespace NETworkManager.Converters
{
    public sealed class DiscoveryProtocolToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Protocol s))
                return "-/-";

            return DiscoveryProtocolTranslator.GetInstance().Translate(s.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
