﻿using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Utilities;

namespace NETworkManager.Validators;

public class IPAddressOrHostnameAsRangeValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var input = (value as string)?.Trim();

        if (string.IsNullOrEmpty(input))
            return new ValidationResult(false, Strings.EnterValidHostnameOrIPAddress);

        foreach (var item in input.Split(";"))
        {
            var localItem = item.Trim();

            // Check if it is a valid IPv4 address like 192.168.0.1, a valid IPv6 address like "::1" or a valid hostname like "server-01" or "server-01.example.com"
            var isValid = (IPAddress.TryParse(localItem, out var ipAddress) && ipAddress.AddressFamily is System.Net.Sockets.AddressFamily.InterNetwork or System.Net.Sockets.AddressFamily.InterNetworkV6) ||
                          Regex.IsMatch(localItem, RegexHelper.HostnameOrDomainRegex);
            
            if (!isValid)
                return new ValidationResult(false, Strings.EnterValidHostnameOrIPAddress);
        }

        return ValidationResult.ValidResult;
    }
}