
namespace NETworkManager.Models.Network;

/// <summary>
/// Class represents a SNMP MIB profile.
/// </summary>
public class SNMPOIDProfileInfo
{
    /// <summary>
    /// Name of the profile.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Object identifier.
    /// </summary>
    public string OID { get; set; }

    /// <summary>
    /// SNMP mode (Get, Walk, Set).
    /// </summary>
    public SNMPMode Mode { get; set; } = SNMPMode.Get;

    /// <summary>
    /// Create new instance of the <see cref="SNMPOIDProfileInfo"/> class.
    /// </summary>
    public SNMPOIDProfileInfo()
    {

    }

    /// <summary>
    /// Create new instance of the <see cref="SNMPOIDProfileInfo"/> class with parameters.
    /// </summary>
    /// <param name="name">Name of the profile.</param>
    /// <param name="oid">Managed object identifier (MIB).</param>
    public SNMPOIDProfileInfo(string name, string oid, SNMPMode mode)
    {
        Name = name;
        OID = oid;
        Mode = mode;
    }
}
