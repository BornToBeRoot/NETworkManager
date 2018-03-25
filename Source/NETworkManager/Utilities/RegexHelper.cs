namespace NETworkManager.Utilities
{
    public static class RegexHelper
    {
        // Match IPv4-Address like 192.168.178.1
        private const string IPv4AddressValues = @"(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";
        public const string IPv4AddressRegex = "^" + IPv4AddressValues + "$";

        // Match IPv6-Address
        public const string IPv6AddressRegex = @"(?:^|(?<=\s))(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))(?=\s|$)";

        // Match IPv4-Address Range like 192.168.178.1-192.168.178.127
        public const string IPv4AddressRangeRegex = "^" + IPv4AddressValues + "-" + IPv4AddressValues + "$";

        // Match a MAC-Address 000000000000 00:00:00:00:00:00, 00-00-00-00-00-00-00 or 0000.0000.0000
        public const string MACAddressRegex = @"^^[A-Fa-f0-9]{12}$|^[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}$|^[A-Fa-f0-9]{4}.[A-Fa-f0-9]{4}.[A-Fa-f0-9]{4}$$";

        // Matche the first 3 bytes of a MAC-Address 000000, 00:00:00, 00-00-00
        public const string MACAddressFirst3BytesRegex = @"^[A-Fa-f0-9]{6}$|^[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}(:|-){1}[A-Fa-f0-9]{2}$|^[A-Fa-f0-9]{4}.[A-Fa-f0-9]{2}$";

        // Match a Subnetmask like 255.255.255.0
        private const string SubnetmaskValuesFirstOctet = "(255|254|252|248|240|224|192|128)";
        private const string SubnetmaskValues = "(255|254|252|248|240|224|192|128|0)";
        public const string SubnetmaskRegex = "^(" + SubnetmaskValuesFirstOctet + ".0.0.0)|(255." + SubnetmaskValues + ".0.0)|(255.255." + SubnetmaskValues + ".0)|(255.255.255." + SubnetmaskValues + ")$";

        // Match a subnet from 192.168.178.0/1 to 192.168.178.0/32
        public const string IPv4AddressCidrRegex = @"^" + IPv4AddressValues + @"\/([1-9]|[1-2][0-9]|3[0-2])$";

        // Match a subnet from 192.168.178.0/0 to 192.168.178.0/32
        public const string SubnetCalculatorIPv4AddressCidrRegex = @"^" + IPv4AddressValues + @"\/([0-9]|[1-2][0-9]|3[0-2])$";

        // Match a subnet from 192.168.178.0/192.0.0.0 to 192.168.178.0/255.255.255.255
        public const string IPv4AddressSubnetmaskRegex = "^" + IPv4AddressValues + @"\/(" + SubnetmaskValuesFirstOctet + ".0.0.0)|(255." + SubnetmaskValues + ".0.0)|(255.255." + SubnetmaskValues + ".0)|(255.255.255." + SubnetmaskValues + ")$";

        // Match a subnet from 192.168.178.0/0.0.0.0 to 192.168.178.0/255.255.255.255
        public const string SubnetCalculatorIPv4AddressSubnetmaskRegex = "^" + IPv4AddressValues + @"\/(" + SubnetmaskValues + ".0.0.0)|(255." + SubnetmaskValues + ".0.0)|(255.255." + SubnetmaskValues + ".0)|(255.255.255." + SubnetmaskValues + ")$";

        // Match a range like [0-255], [0,2,4] and [2,4-6]
        public const string SpecialRangeRegex = @"\[((?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)|((?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)-(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)))([,]((?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)|((?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)-(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))))*\]";

        // Match a IPv4-Address like 192.168.[50-100].1
        public const string IPv4AddressSpecialRangeRegex = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|" + SpecialRangeRegex + @")\.){3}((?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)|" + SpecialRangeRegex + @")$";

        // Test for http|https uris
        public const string httpAndHttpsUriRegex = @"^http(s)?:\/\/([\w-]+.)+[\w-]+(\/[\w- ./?%&=])?$";

        // OID (SNMP)
        public const string OIDRegex = @"^([1-9][0-9]{0,3}|0)(\.([1-9][0-9]{0,3}|0)){5,13}$";
    }
}
