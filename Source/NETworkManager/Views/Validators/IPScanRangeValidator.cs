using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using NETworkManager.Utilities.Network;
using System.Net;
using NETworkManager.Utilities.Common;

namespace NETworkManager.Views.Validators
{
    public class IPScanRangeValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool isValid = true;

            foreach (string ipOrRange in (value as string).Replace(" ", "").Split(';'))
            {
                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressRegex))
                    continue;

                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressCidrRegex))
                    continue;

                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressSubnetmaskRegex))
                    continue;

                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressRangeRegex))
                {
                    string[] range = ipOrRange.Split('-');

                    if (IPv4AddressHelper.ConvertToInt32(IPAddress.Parse(range[0])) >= IPv4AddressHelper.ConvertToInt32(IPAddress.Parse(range[1])))
                        isValid = false;
                }
                else
                {
                    isValid = false;
                }
            }

            if (isValid)
                return ValidationResult.ValidResult;
            else
                return new ValidationResult(false, Application.Current.Resources["String_ValidateError_EnterValidIPScanRange"] as string);
        }
    }
}
