using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Models.Profile;

namespace NETworkManager.Converters
{
    public sealed class IsDefaultProfilesLocationToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as string == ProfileManager.GetDefaultProfilesLocation();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
