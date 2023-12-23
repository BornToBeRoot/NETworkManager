using System.Globalization;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.AWS;

namespace NETworkManager.Validators;

public class EmptyOrAWSRegionExistsValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var region = value as string;

        if (string.IsNullOrEmpty(region))
            return ValidationResult.ValidResult;

        if (AWSRegion.GetInstance().RegionExists(region))
            return ValidationResult.ValidResult;

        return new ValidationResult(false,
            string.Format(Strings.AnAWSRegionNamedXDoesNotExist, region));
    }
}