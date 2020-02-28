using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Translators;
using static NETworkManager.Models.RemoteDesktop.RemoteDesktop;

namespace NETworkManager.Converters
{
    public sealed class RemoteDesktopNetworkConnectionTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is NetworkConnectionType s))
                return "-/-";

            return RemoteDesktopNetworkConnectionTypeTranslator.GetInstance().Translate(s.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
