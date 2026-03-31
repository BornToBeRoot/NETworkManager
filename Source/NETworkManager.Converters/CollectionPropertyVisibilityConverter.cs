using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using NETworkManager.Interfaces.ViewModels;

namespace NETworkManager.Converters
{
    /// <summary>
    /// A generic converter that checks a property of items in a collection.
    /// If ANY item's property is considered "present" (not null, not empty), it returns Visible.
    /// </summary>
    /// <typeparam name="T">The type of item in the collection.</typeparam>
    public class CollectionPropertyVisibilityConverter<T> : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 1. Validate inputs
            if (value is not IEnumerable<T> collection)
                return Visibility.Collapsed;

            if (parameter is not string propertyName || string.IsNullOrEmpty(propertyName))
                return Visibility.Collapsed;

            // 2. Get PropertyInfo via Reflection or cache.
            if (!Cache.TryGetValue(propertyName, out var propertyInfo))
            {
                propertyInfo = typeof(T).GetProperty(propertyName);
                Cache.TryAdd(propertyName, propertyInfo);
            }

            if (propertyInfo == null)
                return Visibility.Collapsed;

            // 3. Iterate collection and check property
            foreach (var item in collection)
            {
                if (item == null) continue;

                var propValue = propertyInfo.GetValue(item);

                if (HasContent(propValue))
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        private static bool HasContent(object value)
        {
            if (value == null) return false;

            // Handle Strings
            if (value is string str)
                return !string.IsNullOrWhiteSpace(str);

            // Handle Collections (Lists, Arrays, etc.)
            if (value is ICollection col)
                return col.Count > 0;

            // Handle Generic Enumerable (fallback)
            if (value is IEnumerable enumValue)
            {
                var enumerator = enumValue.GetEnumerator();
                var result = enumerator.MoveNext(); // Has at least one item
                (enumerator as IDisposable)?.Dispose();
                return result;
            }

            // Default: If it's an object and not null, it's "True"
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private ConcurrentDictionary<string, PropertyInfo> Cache { get; } = new();
    }

    // Concrete implementation for XAML usage
    public class FirewallRuleViewModelVisibilityConverter : CollectionPropertyVisibilityConverter<IFirewallRuleViewModel>;
}
