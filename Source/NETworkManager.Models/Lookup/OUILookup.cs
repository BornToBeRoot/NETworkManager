using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Reflection;

namespace NETworkManager.Models.Lookup;

/// <summary>
/// Class for looking up OUI information.
/// </summary>
public static class OUILookup
{
    #region Variables

    /// <summary>
    /// Path to the xml file with the oui information's located in the resources folder.
    /// </summary>
    private static readonly string OuiFilePath =
        Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)!, "Resources", "OUI.xml");

    /// <summary>
    /// List of <see cref="OUIInfo"/> with OUI information.
    /// </summary>
    private static readonly List<OUIInfo> OUIInfoList;

    /// <summary>
    /// Lookup of <see cref="OUIInfo"/> with OUI information. Key is the MAC address.
    /// </summary>
    private static readonly Lookup<string, OUIInfo> OUIInfoLookup;

    #endregion

    #region Constructor

    /// <summary>
    /// Loads the OUI XML file and creates the lookup.
    /// </summary>
    static OUILookup()
    {
        OUIInfoList = new List<OUIInfo>();

        var document = new XmlDocument();
        document.Load(OuiFilePath);

        foreach (XmlNode node in document.SelectNodes("/OUIs/OUI")!)
        {
            if (node != null)
                OUIInfoList.Add(new OUIInfo(node.SelectSingleNode("MACAddress")?.InnerText,
                    node.SelectSingleNode("Vendor")?.InnerText));
        }

        OUIInfoLookup = (Lookup<string, OUIInfo>)OUIInfoList.ToLookup(x => x.MACAddress);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Looks up the OUI information for the given MAC address async.
    /// </summary>
    /// <param name="macAddress">MAC address to look up.</param>
    /// <returns>List of OUI information.</returns>
    public static Task<List<OUIInfo>> LookupAsync(string macAddress)
    {
        return Task.Run(() => Lookup(macAddress));
    }

    /// <summary>
    /// Looks up the OUI information for the given MAC address.
    /// </summary>
    /// <param name="macAddress">MAC address to look up.</param>
    /// <returns>List of OUI information.</returns>
    public static List<OUIInfo> Lookup(string macAddress)
    {
        var ouiKey = Regex.Replace(macAddress, "[-|:|.]", "")[..6].ToUpper();

        return OUIInfoLookup[ouiKey].ToList();
    }

    /// <summary>
    /// Looks up the OUI information's by the given vendor async.
    /// </summary>
    /// <param name="vendor">Vendor to look up.</param>
    /// <returns>OUI information's or null if not found.</returns>
    public static Task<List<OUIInfo>> LookupByVendorAsync(string vendor)
    {
        return Task.Run(() => LookupByVendor(vendor));
    }

    /// <summary>
    /// Looks up the OUI information's by the given vendor.
    /// </summary>
    /// <param name="vendor">Vendor to look up.</param>
    /// <returns>OUI information's or null if not found.</returns>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static List<OUIInfo> LookupByVendor(string vendor)
    {
        return (from info in OUIInfoList
                where info.Vendor.IndexOf(vendor, StringComparison.OrdinalIgnoreCase) > -1
                select info
            ).ToList();
    }
    
    /// <summary>
    /// Looks up the OUI information's by the given vendors async.
    /// </summary>
    /// <param name="vendors">Vendors to look up.</param>
    /// <returns>List of OUI information.</returns>
    public static Task<List<OUIInfo>> LookupByVendorsAsync(IReadOnlyCollection<string> vendors)
    {
        return Task.Run(() => LookupByVendors(vendors));
    }

    /// <summary>
    /// Looks up the OUI information's by the given vendors.
    /// </summary>
    /// <param name="vendors">Vendors to look up.</param>
    /// <returns>List of OUI information.</returns>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static List<OUIInfo> LookupByVendors(IReadOnlyCollection<string> vendors)
    {
        return (from info in OUIInfoList
                from vendor in vendors
                where info.Vendor.IndexOf(vendor, StringComparison.OrdinalIgnoreCase) > -1
                select info
            ).ToList();
    }

    #endregion
}