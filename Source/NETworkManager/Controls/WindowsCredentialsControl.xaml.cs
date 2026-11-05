using System.Windows.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Controls;

/// <summary>
///     Windows credentials of a profile or group in the profile and group dialog (Remote Desktop, IP Scanner), the
///     DataContext is a <see cref="ViewModels.WindowsCredentialsViewModel" />.
/// </summary>
public partial class WindowsCredentialsControl
{
    // Text boxes whose input is invalid: WPF doesn't pass it to the view model (it keeps the last valid value)
    private int _inputErrors;

    public WindowsCredentialsControl()
    {
        InitializeComponent();

        // Raised for the bindings with NotifyOnValidationError, so the dialog can't be saved with invalid input
        Validation.AddErrorHandler(this, (_, e) =>
        {
            _inputErrors += e.Action == ValidationErrorEventAction.Added ? 1 : -1;

            UpdateInputError();
        });

        DataContextChanged += (_, _) => UpdateInputError();
    }

    private void UpdateInputError()
    {
        if (DataContext is WindowsCredentialsViewModel viewModel)
            viewModel.HasInputError = _inputErrors > 0;
    }
}
