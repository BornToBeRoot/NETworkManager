namespace NETworkManager.Utilities.Common
{
    public static class RegexHelper
    {
        // Match IPv4-Address like 192.168.178.1
        public const string IPv4AddressRegex = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

        // Match IPv4-Address Range like 192.168.178.1-192.168.178.127
        public const string IPv4AddressRangeRegex = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)-(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

        // Match a MAC-Address 00-00-00-00-00-00-00
        public const string MACAddressRegex = @"^[A-Fa-f0-9]{12}$|^[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}$";

        // Match a Subnetmask like 255.255.255.0
        private const string SubnetmaskValues = @"(255|254|252|248|240|224|192|128|0)";
        public const string SubnetmaskRegex = @"^(" + SubnetmaskValues + ".0.0.0)|(255." + SubnetmaskValues + ".0.0)|(255.255." + SubnetmaskValues + ".0)|(255.255.255." + SubnetmaskValues + ")$";

        // Match a subnet like 192.168.178.0/24
        public const string IPv4AddressCidrRegex = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\/([0-9]|[1-2][0-9]|3[0-2])$";

        // Match a subnet like 192.168.178.0/255.255.255.0
        public const string IPv4AddressSubnetmaskRegex = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\/(" + SubnetmaskValues + ".0.0.0)|(255." + SubnetmaskValues + ".0.0)|(255.255." + SubnetmaskValues + ".0)|(255.255.255." + SubnetmaskValues + ")$";
    }
}
