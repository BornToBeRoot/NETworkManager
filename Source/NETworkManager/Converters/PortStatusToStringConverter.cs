using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Translators;
using static NETworkManager.Models.Network.PortInfo;

namespace NETworkManager.Converters
{
    public sealed class PortStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is PortStatus s))
                return "-/-";

            return PortStatusTranslator.GetInstance().Translate(s.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
