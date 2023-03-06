using NETworkManager.Models.AWS;
using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators;

public class AWSRegionExistsValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var region = value as string;
                    
        if (AWSRegion.GetInstance().RegionExists(region))
            return ValidationResult.ValidResult;

        return new ValidationResult(false, string.Format(Localization.Resources.Strings.AnAWSRegionNamedXDoesNotExist, region));
    }
}
