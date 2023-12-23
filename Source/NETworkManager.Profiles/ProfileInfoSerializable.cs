namespace NETworkManager.Profiles;

public class ProfileInfoSerializable : ProfileInfo
{
    public ProfileInfoSerializable()
    {
    }

    public ProfileInfoSerializable(ProfileInfo profile) : base(profile)
    {
    }

    /// <summary>
    ///     Override the default remote desktop password to make it serializable.
    /// </summary>
    public new string RemoteDesktop_Password { get; set; }

    /// <summary>
    ///     Override the default remote desktop gateway password to make it serializable.
    /// </summary>
    public new string RemoteDesktop_GatewayServerPassword { get; set; }

    /// <summary>
    ///     Override the default snmp community to make it serializable.
    /// </summary>
    public new string SNMP_Community { get; set; }

    /// <summary>
    ///     Override the default snmp auth to make it serializable.
    /// </summary>
    public new string SNMP_Auth { get; set; }

    /// <summary>
    ///     Override the default snmp priv to make it serializable.
    /// </summary>
    public new string SNMP_Priv { get; set; }
}