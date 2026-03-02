using System;
using System.Globalization;
using System.Windows.Data;


namespace NETworkManager.Converters;

/// <summary>
/// A value converter that facilitates the conversion between enumeration values and their corresponding integer indices or string names.
/// </summary>
/// <remarks>
/// This converter is designed to either:
/// - Convert an enumeration value to its integer index within the enumeration.
/// - Convert an enumeration value to its string representation.
/// When converting back, the same logic is applied to handle appropriate conversion.
/// </remarks>
public class EnumToIntConverter : IValueConverter
{
    /// <summary>
    /// Converts an Enum value to its corresponding integer index or string representation
    /// based on the target type. If the target type is an Enum, the method attempts
    /// to return the name of the enum value. If the provided value is an Enum, it
    /// returns the integer index of the value within the Enum's definition.
    /// </summary>
    /// <param name="value">The value to convert. This can be an Enum instance or null.</param>
    /// <param name="targetType">The target type for the conversion. Typically either Enum or int.</param>
    /// <param name="parameter">An optional parameter for additional input, not used in this method.</param>
    /// <param name="culture">The culture information for the conversion process.</param>
    /// <returns>
    /// If the targetType is an Enum, a string representation of the Enum name is returned.
    /// If the value is an Enum, the integer index of the Enum value is returned.
    /// If neither condition is met, null is returned.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType.IsEnum)
        {
            return value is null ? string.Empty :
                Enum.GetName(targetType, value);
        }
        if (value is Enum enumVal)
        {
            return Array.IndexOf(Enum.GetValues(enumVal.GetType()), enumVal);
        }

        return null;
    }

    /// Converts a value back into its corresponding enumeration value or integer representation.
    /// This method is typically used in two-way data bindings where an integer or string needs
    /// to be converted back to the corresponding enum value.
    /// <param name="value">The value to be converted back. This can be an integer or a string representation of an enumeration value.</param>
    /// <param name="targetType">The type of the target object, typically the type of the enumeration.</param>
    /// <param name="parameter">An optional parameter for the conversion. Not used in this implementation.</param>
    /// <param name="culture">The culture information to use during the conversion process.</param>
    /// <return>The enumeration value corresponding to the input value.</return>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }
}