using System.Collections.Generic;

namespace NETworkManager.Models.Network;

public static class SNMPOIDProfile
{
    /// <summary>
    /// Get default list of SNMP MIB profiles.
    /// </summary>
    /// <returns>List of SNMP MIB profiles.</returns>
    public static List<SNMPOIDProfileInfo> GetDefaultList()
    {
        return new List<SNMPOIDProfileInfo>
        {
            new SNMPOIDProfileInfo("HOST-RESOURCES-MIB", ".1.3.6.1.2.1.25", SNMPMode.Walk),
            new SNMPOIDProfileInfo("IF-MIB", ".1.3.6.1.2.1.2", SNMPMode.Walk),
            new SNMPOIDProfileInfo("IP-MIB", ".1.3.6.1.2.1.4", SNMPMode.Walk),
            new SNMPOIDProfileInfo("Linux - Interface names", ".1.3.6.1.2.1.2.2.1.2", SNMPMode.Walk),
            new SNMPOIDProfileInfo("Linux - Load (1, 5, 15 min)", ".1.3.6.1.4.1.2021.10.1.3.1; .1.3.6.1.4.1.2021.10.1.3.2; .1.3.6.1.4.1.2021.10.1.3.3", SNMPMode.Get),
            new SNMPOIDProfileInfo("Linux - Memory (Swap size, total RAM, RAM used, RAM free)", ".1.3.6.1.4.1.2021.4.3.0; .1.3.6.1.4.1.2021.4.5.0; .1.3.6.1.4.1.2021.4.6.0; .1.3.6.1.4.1.2021.4.11.0", SNMPMode.Get),
            new SNMPOIDProfileInfo("Linux - SNMP uptime", ".1.3.6.1.2.1.1.3.0", SNMPMode.Get),
            new SNMPOIDProfileInfo("Linux - System uptime", ".1.3.6.1.2.1.25.1.1.0", SNMPMode.Get),
            new SNMPOIDProfileInfo("SNMPv2-MIB (system)", ".1.3.6.1.2.1.1", SNMPMode.Walk),
            new SNMPOIDProfileInfo("TCP-MIB", ".1.3.6.1.2.1.6", SNMPMode.Walk),
            new SNMPOIDProfileInfo("UDP-MIB", ".1.3.6.1.2.1.7", SNMPMode.Walk),
            new SNMPOIDProfileInfo("UCD-SNMP-MIB", ".1.3.6.1.4.1.2021", SNMPMode.Walk),
        };
    }
}
