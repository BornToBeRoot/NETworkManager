using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators;

public class HostsFileEntryHostnameValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var input = (value as string);

        var isValid = true;

        Debug.WriteLine(input);

        foreach (var hostname in input.Split(' '))
        {
            if (RegexHelper.HostnameOrDomainRegex().IsMatch(hostname) == false)
            {
                isValid = false;
                break;
            }
        }

        if (isValid)
            return ValidationResult.ValidResult;

        return new ValidationResult(false, Strings.EnterValidHostsFileEntryHostname);
    }
}