namespace NETworkManager.Profiles;

public class ProfileInfoSerializable : ProfileInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileInfoSerializable"/> class.
    /// </summary>
    public ProfileInfoSerializable()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileInfoSerializable"/> class using the specified profile information.
    /// </summary>
    /// <param name="profile">The <see cref="ProfileInfo" /> object that contains the profile data to be serialized. Cannot be null.</param>
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