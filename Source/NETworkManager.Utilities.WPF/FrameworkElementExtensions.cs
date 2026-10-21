using System;
using System.Threading.Tasks;
using System.Windows;

namespace NETworkManager.Utilities.WPF;

/// <summary>
///     Extension methods for <see cref="FrameworkElement" />.
/// </summary>
public static class FrameworkElementExtensions
{
    /// <summary>
    ///     Default time to wait for the <see cref="FrameworkElement.Loaded" /> event. A view that is shown is loaded
    ///     within milliseconds.
    /// </summary>
    private static readonly TimeSpan DefaultLoadedTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Waits until the element is loaded (e.g. before showing a dialog from a view that was just created, which
    ///     MahApps doesn't allow before the view is loaded). Completes immediately if the element is already loaded.
    ///     Stops waiting after the timeout (e.g. the user switched to another view before it was loaded), so an action
    ///     doesn't run unexpectedly when the view is shown later.
    /// </summary>
    /// <param name="element">Element to wait for.</param>
    /// <param name="timeout">Maximum time to wait. Default: 5 seconds.</param>
    /// <returns>True if the element is loaded, false if the timeout elapsed.</returns>
    public static async Task<bool> WaitForLoadedAsync(this FrameworkElement element, TimeSpan? timeout = null)
    {
        if (element.IsLoaded)
            return true;

        var taskCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            taskCompletionSource.TrySetResult();
        }

        element.Loaded += OnLoaded;

        try
        {
            var completedTask = await Task.WhenAny(taskCompletionSource.Task,
                Task.Delay(timeout ?? DefaultLoadedTimeout));

            return completedTask == taskCompletionSource.Task;
        }
        finally
        {
            element.Loaded -= OnLoaded;
        }
    }
}
