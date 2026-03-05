using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using NETworkManager.Interfaces.ViewModels;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Validators;

public class FirewallRuleNoPipeValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string valueToCheck;
        bool isProfileValidation = false;
        if (value is not BindingExpression bindingExpression)
        {
            valueToCheck = value?.ToString();
        } else if (bindingExpression.DataItem is not IProfileViewModel viewModel)
        {
            return new ValidationResult(false,
                "No ViewModel could be found. Is the ValidationStep set correctly?");
        }
        else
        {
            isProfileValidation = true;
            if (!viewModel.Firewall_Enabled)
                return ValidationResult.ValidResult;
            valueToCheck = viewModel.Name;
        }
        if (string.IsNullOrWhiteSpace(valueToCheck))
            return ValidationResult.ValidResult;
        return valueToCheck.Contains('|')
            ? new ValidationResult(false,
                isProfileValidation ? Strings.PipeNotAllowedWhenFirewallEnabled : Strings.PipeNotAllowed)
            : ValidationResult.ValidResult;

    }
}