using System.Windows.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Controls;

/// <summary>
///     Override of the SNMP version and credentials in the profile and group dialog (SNMP tool, IP Scanner), the
///     DataContext is a <see cref="ViewModels.SNMPCredentialsViewModel" />.
/// </summary>
public partial class SNMPCredentialsControl
{
    // Text boxes whose input is invalid: WPF doesn't pass it to the view model (it keeps the last valid value)
    private int _inputErrors;

    public SNMPCredentialsControl()
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
        if (DataContext is SNMPCredentialsViewModel viewModel)
            viewModel.HasInputError = _inputErrors > 0;
    }
}
