using NETworkManager.Localization.Translators;
using NETworkManager.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    /// <summary>
    /// Convert <see cref="Application.Name"/> to translated <see cref="string"/> or wise versa.
    /// </summary>
    public sealed class ApplicationNameToTranslatedStringConverter : IValueConverter
    {
        /// <summary>
        /// Convert <see cref="Application.Name"/> to translated <see cref="string"/>. 
        /// </summary>
        /// <param name="value">Object from type <see cref="Application.Name"/>.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Translated <see cref="Application.Name"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Models.Application.Name name))
                return "-/-";

            return ApplicationNameTranslator.GetInstance().Translate(name);
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
