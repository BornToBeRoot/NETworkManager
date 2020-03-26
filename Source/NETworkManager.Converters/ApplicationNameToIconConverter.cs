using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using NETworkManager.Models.Application;

namespace NETworkManager.Converters
{
    /// <summary>
    /// Convert <see cref="ApplicationName"/> to icon (<see cref="Canvas"/>) or wise versa.
    /// </summary>
    public sealed class ApplicationNameToIconConverter : IValueConverter
    {
        /// <summary>
        /// Convert <see cref="ApplicationName"/> to icon (<see cref="Canvas"/>).
        /// </summary>
        /// <param name="value">Object from type <see cref="ApplicationName"/>.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Icon (cref="Canvas"/>).</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ApplicationName name))
                return null;

            return ApplicationManager.GetIcon(name);
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
