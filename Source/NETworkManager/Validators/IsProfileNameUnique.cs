using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NETworkManager.Models.Settings;

namespace NETworkManager.Validators
{
    public class IsProfileNameUnique : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return new ValidationResult(false, "Design Mode!");

            return ProfileManager.Profiles.Any(x => string.Equals(x.Name, value as string, StringComparison.OrdinalIgnoreCase)) ? new ValidationResult(false, Resources.Localization.Strings.ProfileWithThisNameAlreadyExists) : ValidationResult.ValidResult;
        }
    }
}
