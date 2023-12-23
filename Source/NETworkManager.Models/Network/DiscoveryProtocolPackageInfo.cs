namespace NETworkManager.Models.Network;

/// <summary>
///     Class contains discovery protocol package information's.
/// </summary>
public class DiscoveryProtocolPackageInfo
{
    /// <summary>
    ///     Creates a new instance of <see cref="DiscoveryProtocolPackageInfo" />.
    /// </summary>
    public DiscoveryProtocolPackageInfo()
    {
    }

    /// <summary>
    ///     Device name.
    /// </summary>
    public string Device { get; init; }

    /// <summary>
    ///     Device description.
    /// </summary>
    public string DeviceDescription { get; init; }

    /// <summary>
    ///     Port name or number.
    /// </summary>
    public string Port { get; init; }

    /// <summary>
    ///     Port description.
    /// </summary>
    public string PortDescription { get; init; }

    /// <summary>
    ///     Device model.
    /// </summary>
    public string Model { get; init; }

    /// <summary>
    ///     Management VLAN.
    /// </summary>
    public string VLAN { get; init; }

    /// <summary>
    ///     IP address(es) of the device.
    /// </summary>
    public string IPAddress { get; init; }

    /// <summary>
    ///     Protocol type.
    /// </summary>
    public string Protocol { get; init; }

    /// <summary>
    ///     Time to live of the LLDP/CDP package.
    /// </summary>
    public string TimeToLive { get; init; }

    /// <summary>
    ///     Device Management.
    /// </summary>
    public string Management { get; init; }

    /// <summary>
    ///     Device Chassis ID.
    /// </summary>
    public string ChassisId { get; init; }

    /// <summary>
    ///     Local connection used to capture the LLDP/CDP package.
    /// </summary>
    public string LocalConnection { get; init; }

    /// <summary>
    ///     Local interface used to capture the LLDP/CDP package.
    /// </summary>
    public string LocalInterface { get; init; }
}