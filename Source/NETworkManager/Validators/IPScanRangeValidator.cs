using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Utilities;
using System.Net;

namespace NETworkManager.Validators
{
    public class IPScanRangeValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var isValid = true;

            if (value == null)
                return new ValidationResult(false, Resources.Localization.Strings.EnterValidIPScanRange);

            foreach (var ipHostOrRange in ((string) value).Replace(" ", "").Split(';'))
            {
                // like 192.168.0.1
                if (Regex.IsMatch(ipHostOrRange, RegexHelper.IPv4AddressRegex))
                    continue;

                // like 192.168.0.0/24
                if (Regex.IsMatch(ipHostOrRange, RegexHelper.IPv4AddressCidrRegex))
                    continue;

                // like 192.168.0.0/255.255.255.0
                if (Regex.IsMatch(ipHostOrRange, RegexHelper.IPv4AddressSubnetmaskRegex))
                    continue;

                // like 192.168.0.0 - 192.168.0.100
                if (Regex.IsMatch(ipHostOrRange, RegexHelper.IPv4AddressRangeRegex))
                {
                    var range = ipHostOrRange.Split('-');

                    if (IPv4AddressHelper.ConvertToInt32(IPAddress.Parse(range[0])) >=
                        IPv4AddressHelper.ConvertToInt32(IPAddress.Parse(range[1])))
                        isValid = false;

                    continue;
                }

                // like 192.168.[50-100].1
                if (Regex.IsMatch(ipHostOrRange, RegexHelper.IPv4AddressSpecialRangeRegex))
                {
                    var octets = ipHostOrRange.Split('.');

                    foreach (var octet in octets)
                    {
                        // Match [50-100]
                        if (!Regex.IsMatch(octet, RegexHelper.SpecialRangeRegex))
                            continue;

                        foreach (var numberOrRange in octet.Substring(1, octet.Length - 2).Split(','))
                        {
                            if (!numberOrRange.Contains("-"))
                                continue;

                            // 50-100 --> {50, 100}
                            var rangeNumber = numberOrRange.Split('-');

                            if (int.Parse(rangeNumber[0]) > int.Parse(rangeNumber[1]))
                                isValid = false;
                        }
                    }

                    continue;
                }

                // like server-01.example.com
                if (Regex.IsMatch(ipHostOrRange, RegexHelper.HostnameRegex))
                    continue;

                // like server-01.example.com/24
                if (Regex.IsMatch(ipHostOrRange, RegexHelper.HostnameCidrRegex))
                    continue;

                // like server-01.example.com/255.255.255.0
                if (Regex.IsMatch(ipHostOrRange, RegexHelper.HostnameSubnetmaskRegex))
                    continue;

                isValid = false;
            }

            return isValid ? ValidationResult.ValidResult : new ValidationResult(false, Resources.Localization.Strings.EnterValidIPScanRange);
        }
    }
}
