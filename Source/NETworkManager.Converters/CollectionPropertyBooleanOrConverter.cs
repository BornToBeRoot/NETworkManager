using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using NETworkManager.Interfaces.ViewModels;

namespace NETworkManager.Converters
{
    /// <summary>
    /// A generic converter that checks a property of items in a collection.
    /// If ANY item's property is considered "present" (not null, not empty), it returns Visible.
    /// </summary>
    /// <typeparam name="T">The type of item in the collection.</typeparam>
    public class CollectionPropertyBooleanOrConverter<T> : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 1. Validate inputs
            if (value is not IEnumerable<T> collection)
                return false;

            if (parameter is not string propertyName || string.IsNullOrEmpty(propertyName))
                return false;

            // 2. Get PropertyInfo via Reflection or cache.
            if (!Cache.TryGetValue(propertyName, out var propertyInfo))
            {
                propertyInfo = typeof(T).GetProperty(propertyName);
                Cache.TryAdd(propertyName, propertyInfo);
            }

            if (propertyInfo == null)
                return false;

            // 3. Iterate collection and check property
            foreach (var item in collection)
            {
                if (item == null) continue;

                var propValue = propertyInfo.GetValue(item);

                if (propValue is true)
                    return true;
            }

            return false;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private ConcurrentDictionary<string, PropertyInfo> Cache { get; } = new();
    }
    
    // Concrete implementation for XAML usage
    public class FirewallRuleViewModelBooleanOrConverter : CollectionPropertyBooleanOrConverter<IFirewallRuleViewModel>;
}

