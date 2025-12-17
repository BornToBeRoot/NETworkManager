using System.Collections.Generic;
using System.Windows;

namespace NETworkManager.Utilities;

/// <summary>
/// Provides helper methods for traversing and querying the visual tree of WPF elements.
/// </summary>
/// <remarks>The VisualTreeHelper class contains static methods that facilitate searching for and enumerating
/// visual child elements within a WPF application's visual tree. These methods are useful for scenarios where you need
/// to locate elements of a specific type or perform operations on all descendants of a visual element.</remarks>
public class VisualTreeHelper
{
    /// <summary>
    /// Enumerates all descendant visual children of a specified type from the visual tree starting at the given
    /// dependency object.
    /// </summary>
    /// <remarks>This method performs a recursive depth-first traversal of the visual tree. It yields each
    /// descendant of the specified type, including those nested at any depth. The enumeration is deferred and elements
    /// are returned as they are discovered.</remarks>
    /// <typeparam name="T">The type of visual child to search for. Must derive from DependencyObject.</typeparam>
    /// <param name="depObj">The root of the visual tree to search. Cannot be null.</param>
    /// <returns>An enumerable collection of all descendant elements of type T found in the visual tree. The collection is empty
    /// if no matching elements are found.</returns>
    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj == null)
            yield break;

        for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);

            if (child is T variable) yield return variable;

            foreach (var childOfChild in FindVisualChildren<T>(child)) yield return childOfChild;
        }
    }
}
