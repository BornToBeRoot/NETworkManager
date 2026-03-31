using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using NETworkManager.Interfaces.ViewModels;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators;

/// <summary>
/// Provides validation logic for user-defined names or descriptions used in firewall rules.
/// Ensures the value meets specific criteria for validity:
/// - The character '|' is not allowed.
/// - The string length does not exceed 9999 characters.
/// </summary>
public class FirewallRuleNameValidator : ValidationRule
{
    /// <summary>
    /// Validates a string based on the following two conditions:
    /// - The string must not contain the '|' character.
    /// - The string must not exceed a length of 9999 characters.
    /// </summary>
    /// <param name="value">The value to be validated.</param>
    /// <param name="cultureInfo">The culture information used during validation.</param>
    /// <returns>A ValidationResult indicating whether the string is valid or not.</returns>
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var bindingExpression = value as BindingExpression;
        if (bindingExpression?.DataItem is not IFirewallRuleViewModel viewModel)
        {
            return new ValidationResult(false,
                "No ViewModel could be found. Is the ValidationStep set correctly?");
        }

        if (string.IsNullOrEmpty(viewModel.UserDefinedName))
        {
            viewModel.NameHasError = false;
            return ValidationResult.ValidResult;
        }

        if (viewModel.UserDefinedName.Length <= viewModel.MaxLengthName)
        {
            viewModel.NameHasError = false;
            return ValidationResult.ValidResult;
        }

        viewModel.NameHasError = true;
        return new ValidationResult(false, Strings.InputLengthExceeded);
    }

}