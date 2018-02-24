using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using NETworkManager.Helpers;
using System.Net;

namespace NETworkManager.Validators
{
    public class IPScanRangeValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool isValid = true;

            foreach (string ipOrRange in (value as string).Replace(" ", "").Split(';'))
            {
                // like 192.168.0.1
                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressRegex))
                    continue;

                // like 192.168.0.0/24
                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressCidrRegex))
                    continue;

                // like 192.168.0.0/255.255.255.0
                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressSubnetmaskRegex))
                    continue;

                // like 192.168.0.0 - 192.168.0.100
                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressRangeRegex))
                {
                    string[] range = ipOrRange.Split('-');

                    if (IPv4AddressHelper.ConvertToInt32(IPAddress.Parse(range[0])) >= IPv4AddressHelper.ConvertToInt32(IPAddress.Parse(range[1])))
                        isValid = false;

                    continue;
                }

                // like 192.168.[50-100].1
                if (Regex.IsMatch(ipOrRange, RegexHelper.IPv4AddressSpecialRangeRegex))
                {
                    string[] octets = ipOrRange.Split('.');

                    foreach (string octet in octets)
                    {
                        // Match [50-100]
                        if (Regex.IsMatch(octet, RegexHelper.SpecialRangeRegex))
                        {
                            foreach (string numberOrRange in octet.Substring(1, octet.Length - 2).Split(','))
                            {
                                if (numberOrRange.Contains("-"))
                                {
                                    // 50-100 --> {50, 100}
                                    string[] rangeNumber = numberOrRange.Split('-');

                                    if (int.Parse(rangeNumber[0]) > int.Parse(rangeNumber[1]))
                                        isValid = false;
                                }
                            }
                        }
                    }

                    continue;
                }

                isValid = false;
            }

            if (isValid)
                return ValidationResult.ValidResult;
            else
                return new ValidationResult(false, Application.Current.Resources["String_ValidationError_EnterValidIPScanRange"] as string);
        }
    }
}
