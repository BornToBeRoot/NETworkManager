using System.Threading.Tasks;
using System.Windows;

namespace NETworkManager.Utilities.WPF;

/// <summary>
///     Extension methods for <see cref="FrameworkElement" />.
/// </summary>
public static class FrameworkElementExtensions
{
    /// <summary>
    ///     Waits until the element is loaded (e.g. before showing a dialog from a view that was just created, which
    ///     MahApps doesn't allow before the view is loaded). Completes immediately if the element is already loaded.
    /// </summary>
    /// <param name="element">Element to wait for.</param>
    /// <returns>Task that completes when the element is loaded.</returns>
    public static Task WaitForLoadedAsync(this FrameworkElement element)
    {
        if (element.IsLoaded)
            return Task.CompletedTask;

        var taskCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            element.Loaded -= OnLoaded;
            taskCompletionSource.TrySetResult();
        }

        element.Loaded += OnLoaded;

        return taskCompletionSource.Task;
    }
}
