using NETworkManager.Localization.Translators;
using NETworkManager.Settings;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    /// <summary>
    /// Convert <see cref="SettingsViewGroup"/> to translated <see cref="string"/> or wise versa.
    /// </summary>
    public sealed class SettingsViewGroupToTranslatedStringConverter : IValueConverter
    {
        /// <summary>
        /// Convert <see cref="SettingsViewGroup"/> to translated <see cref="string"/>. 
        /// </summary>
        /// <param name="value">Object from type <see cref="SettingsViewGroup"/>.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Translated <see cref="SettingsViewGroup"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SettingsViewGroup group))
                return "-/-";

            return SettingsViewGroupTranslator.GetInstance().Translate(group);
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
