using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Translators;
using static NETworkManager.Models.RemoteDesktop.RemoteDesktop;

namespace NETworkManager.Converters
{
    public sealed class RemoteDesktopKeyboardHookModeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is KeyboardHookMode s))
                return "-/-";

            return RemoteDesktopKeyboardHookModeTranslator.GetInstance().Translate(s.ToString());                        
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
