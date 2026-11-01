using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using System;
using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators;

public class MultipleHostsRangeValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var isValid = true;

        // At least one input that is not an exclusion (e.g. not only "!10.0.0.1")
        var hasInclusion = false;

        if (value == null)
            return new ValidationResult(false, Strings.EnterValidIPScanRange);

        foreach (var input in ((string)value)
            .Replace(" ", "")
            .Split([';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            // Exclusion (e.g. "!10.0.0.1"), validated like the other inputs
            var isExclusion = HostRangeHelper.IsExclusion(input);

            if (!isExclusion)
                hasInclusion = true;

            // Same formats as the scan (also a range whose start is after its end is invalid)
            if (HostRangeHelper.Parse(isExclusion ? input[1..] : input).Kind == HostRangeInputKind.Invalid)
                isValid = false;
        }

        return isValid && hasInclusion
            ? ValidationResult.ValidResult
            : new ValidationResult(false, Strings.EnterValidIPScanRange);
    }
}