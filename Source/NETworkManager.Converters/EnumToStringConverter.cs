using System;
using System.Collections;
using System.Globalization;
using System.Resources;
using System.Windows.Data;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Converters;

/// <summary>
/// A converter used to transform an <see cref="Enum"/> value into its corresponding string,
/// using localization resources for string mapping. Also provides functionality to convert
/// localized or raw string values back to the corresponding <see cref="Enum"/> value.
/// </summary>
/// <threadsafety>
/// This class is not guaranteed to be thread-safe. It should be used only in the context of the application’s
/// UI thread or with proper synchronization for shared use cases.
/// </threadsafety>
/// <seealso cref="IValueConverter"/>
public sealed class EnumToStringConverter : IValueConverter
{
    /// Converts an enumeration value to its localized string representation and vice versa.
    /// <param name="value">
    /// The value to convert. This can be an enumeration value or a string.
    /// </param>
    /// <param name="targetType">
    /// The expected type of the target binding, typically the type of the enumeration being converted.
    /// </param>
    /// <param name="parameter">
    /// An optional parameter to use in the conversion process, if applicable.
    /// </param>
    /// <param name="culture">
    /// The culture information to use during the conversion, affecting localization.
    /// </param>
    /// <return>
    /// A string representing the localized name of the enumeration, or the enumeration corresponding
    /// to the given string. If the conversion fails, a fallback enumeration value or string is returned.
    /// </return>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Enum enumValue)
        {
            string fallback = Enum.ToObject(targetType, 255) as string;
            if (value is not string strVal)
                return fallback;
            fallback = strVal;
            ResourceSet resourceSet = Strings.ResourceManager.GetResourceSet(Strings.Culture,
                false, true);
            string foundKey = null;
            if (resourceSet is null)
                return fallback;
            foreach (DictionaryEntry item in resourceSet)
            {
                if (item.Value as string != strVal && item.Key as string != strVal)
                    continue;
                foundKey = item.Key as string;
                break;
            }

            if (foundKey is null || !Enum.TryParse(targetType, foundKey, out var result))
                return fallback;
            return result;
        }

        var enumString = Enum.GetName(enumValue.GetType(), value);
        if (enumString is null)
            return string.Empty;
        return Strings.ResourceManager.GetString(enumString, Strings.Culture) ?? enumString;
    }

    /// <summary>
    /// Converts a value back from a string representation to its corresponding enumeration value.
    /// </summary>
    /// <param name="value">The value to be converted back. Expected to be a string representation of an enumeration.</param>
    /// <param name="targetType">The type of the enumeration to which the value will be converted.</param>
    /// <param name="parameter">An optional parameter that can be used for custom conversion logic.</param>
    /// <param name="culture">The culture information to use during the conversion process.</param>
    /// <returns>
    /// Returns the enumeration value corresponding to the string representation. If the conversion fails,
    /// a default value of the enumeration is returned.
    /// </returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }
}