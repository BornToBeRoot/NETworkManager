using NETworkManager.Localization.Translators;
using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class IPStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IPStatus s))
                return "-/-";
                        
            return IPStatusTranslator.GetInstance().Translate(s.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
