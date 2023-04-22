using System.Security;

namespace NETworkManager.Models.Network;

public class SNMPSessionInfo
{
    public string Host { get; set; }
    public SNMPMode Mode { get; set; }
    public string OID { get; set; }
    public SNMPVersion Version { get; set; }
    public SecureString Community { get; set; }
    public SNMPV3Security Security { get; set; }
    public string Username { get; set; }
    public SNMPV3AuthenticationProvider AuthenticationProvider { get; set; }
    public SecureString Auth { get; set; } = null;
    public SNMPV3PrivacyProvider PrivacyProvider { get; set; }
    public SecureString Priv { get; set; } = null;

    public SNMPSessionInfo()
    {

    }
}
