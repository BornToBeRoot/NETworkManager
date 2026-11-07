using System.Windows;
using System.Windows.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Controls;

/// <summary>
///     Tells the view model of a control (<see cref="IHasInputError" />) if one of its text boxes has invalid input, so
///     a dialog can't be saved with it. Counts the validation errors of the bindings with
///     <c>NotifyOnValidationError</c> in the control.
/// </summary>
public static class InputErrorTracker
{
    /// <summary>
    ///     Tracks the validation errors of a control (call it in the constructor).
    /// </summary>
    /// <param name="control">Control, its DataContext is a <see cref="IHasInputError" />.</param>
    public static void Track(FrameworkElement control)
    {
        var inputErrors = 0;

        void Update()
        {
            if (control.DataContext is IHasInputError viewModel)
                viewModel.HasInputError = inputErrors > 0;
        }

        Validation.AddErrorHandler(control, (_, e) =>
        {
            inputErrors += e.Action == ValidationErrorEventAction.Added ? 1 : -1;

            Update();
        });

        control.DataContextChanged += (_, _) => Update();
    }
}
