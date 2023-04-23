using NETworkManager.Utilities;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace NETworkManager.Validators;

public class SNMPOIDValidator : ValidationRule
{
    public SNMPOIDDependencyObjectWrapper Wrapper { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var oidValue = (value as string).Replace(" ", "");

        if (Wrapper.Mode == Models.Network.SNMPMode.Get && oidValue.Contains(';'))
        {
            foreach (var oid in oidValue.Split(';'))
            {
                if (!Regex.IsMatch(oid, RegexHelper.SNMOIODRegex))
                    return new ValidationResult(false, Localization.Resources.Strings.EnterValidOID);
            }

            return ValidationResult.ValidResult;
        }

        return Regex.IsMatch(oidValue, RegexHelper.SNMOIODRegex) ? ValidationResult.ValidResult : new ValidationResult(false, Localization.Resources.Strings.EnterValidOID);
    }
}
