using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using NETworkManager.Localization.Resources;
using NETworkManager.Interfaces.ViewModels;

namespace NETworkManager.Validators;

/// <summary>
/// Checks for any network profile to be selected.
/// </summary>
/// <remarks>
/// Has to be called in ValidationStep.UpdatedValue or later. This means that the value update cannot be prevented,
/// however, we can still show the configuration error.
/// </remarks>
public class AnyNetworkProfileValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var bindingExpression = value as BindingExpression;
        if (bindingExpression?.DataItem is not IFirewallRuleViewModel viewModel)
        {
            return new ValidationResult(false,
                "No ViewModel could be found. Is the ValidationStep set correctly?");
        }
        if (bindingExpression.DataItem is true)
            return ValidationResult.ValidResult;
        
        bool[] currentValues =
        [
            viewModel.NetworkProfileDomain,
            viewModel.NetworkProfilePrivate,
            viewModel.NetworkProfilePublic
        ];
        
        return currentValues.Any(x => x) ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.AtLeastOneNetworkProfileMustBeSelected);
    }
}