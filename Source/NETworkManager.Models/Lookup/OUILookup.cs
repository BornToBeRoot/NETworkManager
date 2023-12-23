using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace NETworkManager.Models.Lookup;

/// <summary>
///     Class for looking up OUI information.
/// </summary>
public static class OUILookup
{
    #region Constructor

    /// <summary>
    ///     Loads the OUI XML file and creates the lookup.
    /// </summary>
    static OUILookup()
    {
        OUIInfoList = new List<OUIInfo>();

        var document = new XmlDocument();
        document.Load(OuiFilePath);

        foreach (XmlNode node in document.SelectNodes("/OUIs/OUI")!)
            if (node != null)
                OUIInfoList.Add(new OUIInfo(node.SelectSingleNode("MACAddress")?.InnerText,
                    node.SelectSingleNode("Vendor")?.InnerText));

        OUIInfoLookup = (Lookup<string, OUIInfo>)OUIInfoList.ToLookup(x => x.MACAddress);
    }

    #endregion

    #region Variables

    /// <summary>
    ///     Path to the xml file with the oui information's located in the resources folder.
    /// </summary>
    private static readonly string OuiFilePath =
        Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)!, "Resources", "OUI.xml");

    /// <summary>
    ///     List of <see cref="OUIInfo" /> with OUI information.
    /// </summary>
    private static readonly List<OUIInfo> OUIInfoList;

    /// <summary>
    ///     Lookup of <see cref="OUIInfo" /> with OUI information. Key is the MAC address.
    /// </summary>
    private static readonly Lookup<string, OUIInfo> OUIInfoLookup;

    #endregion

    #region Methods

    /// <summary>
    ///     Get the <see cref="OUIInfo" /> for the given MAC address async.
    /// </summary>
    /// <param name="macAddress">MAC address to get the OUI information's for.</param>
    /// <returns>List of <see cref="OUIInfo" />. Empty if nothing was found.</returns>
    public static Task<List<OUIInfo>> LookupByMacAddressAsync(string macAddress)
    {
        return Task.Run(() => LookupByMacAddress(macAddress));
    }

    /// <summary>
    ///     Get the <see cref="OUIInfo" /> for the given MAC address.
    /// </summary>
    /// <param name="macAddress">MAC address to get the OUI information's for.</param>
    /// <returns>List of <see cref="OUIInfo" />. Empty if nothing was found.</returns>
    public static List<OUIInfo> LookupByMacAddress(string macAddress)
    {
        var ouiKey = Regex.Replace(macAddress, "[-|:|.]", "")[..6].ToUpper();

        return OUIInfoLookup[ouiKey].ToList();
    }

    /// <summary>
    ///     Search <see cref="OUIInfo" /> by the given vendor async.
    /// </summary>
    /// <param name="vendor">Vendor to look up.</param>
    /// <returns><see cref="OUIInfo" /> or null if not found.</returns>
    public static Task<List<OUIInfo>> SearchByVendorAsync(string vendor)
    {
        return Task.Run(() => SearchByVendor(vendor));
    }

    /// <summary>
    ///     Search <see cref="OUIInfo" /> by the given vendor.
    /// </summary>
    /// <param name="vendor">Vendor to look up.</param>
    /// <returns><see cref="OUIInfo" /> or null if not found.</returns>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static List<OUIInfo> SearchByVendor(string vendor)
    {
        return (from info in OUIInfoList
                where info.Vendor.IndexOf(vendor, StringComparison.OrdinalIgnoreCase) > -1
                select info
            ).ToList();
    }

    /// <summary>
    ///     Search <see cref="OUIInfo" /> by the given vendors async.
    /// </summary>
    /// <param name="vendors">Vendors to look up.</param>
    /// <returns>List of <see cref="OUIInfo" />. Empty if nothing was found.</returns>
    public static Task<List<OUIInfo>> SearchByVendorsAsync(IReadOnlyCollection<string> vendors)
    {
        return Task.Run(() => SearchByVendors(vendors));
    }

    /// <summary>
    ///     Search <see cref="OUIInfo" /> by the given vendors.
    /// </summary>
    /// <param name="vendors">Vendors to look up.</param>
    /// <returns>List of <see cref="OUIInfo" />. Empty if nothing was found.</returns>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static List<OUIInfo> SearchByVendors(IReadOnlyCollection<string> vendors)
    {
        return (from info in OUIInfoList
                from vendor in vendors
                where info.Vendor.IndexOf(vendor, StringComparison.OrdinalIgnoreCase) > -1
                select info
            ).ToList();
    }

    #endregion
}