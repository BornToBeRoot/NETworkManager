using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class ApplicationNameToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ApplicationViewManager.Name name))
                return null;

            return ApplicationViewManager.GetIconByName(name);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
