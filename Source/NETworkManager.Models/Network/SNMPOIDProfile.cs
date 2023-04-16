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
            new SNMPOIDProfileInfo("SNMPv2-MIB (system)", "1.3.6.1.2.1.1", SNMPMode.Walk),
            new SNMPOIDProfileInfo("HOST-RESOURCES-MIB", "1.3.6.1.2.1.25", SNMPMode.Walk)            
        };
    }
}
