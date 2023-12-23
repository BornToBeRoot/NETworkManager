namespace NETworkManager.Models.Lookup;

/// <summary>
///     Class to hold OUI information.
/// </summary>
public class OUIInfo
{
    /// <summary>
    ///     Creates a new instance of the <see cref="OUIInfo" /> class with the specified parameters.
    /// </summary>
    /// <param name="macAddress">MAC address.</param>
    /// <param name="vendor">Name of the vendor.</param>
    public OUIInfo(string macAddress, string vendor)
    {
        MACAddress = macAddress;
        Vendor = vendor;
    }

    /// <summary>
    ///     MAC address.
    /// </summary>
    public string MACAddress { get; set; }

    /// <summary>
    ///     Name of the vendor.
    /// </summary>
    public string Vendor { get; set; }
}