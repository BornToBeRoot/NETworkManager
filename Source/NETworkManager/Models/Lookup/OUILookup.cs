using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using NETworkManager.Models.Settings;
using System.Text.RegularExpressions;
using System;
using System.Xml;

namespace NETworkManager.Models.Lookup
{
    public static class OuiLookup
    {
        #region Variables
        private static readonly string OuiFilePath = Path.Combine(ConfigurationManager.Current.ExecutionPath, "Resources", "OUI.xml");

        private static readonly List<OuiInfo> OuiList;
        private static readonly Lookup<string, OuiInfo> OuiInfoLookup;
        #endregion

        #region Constructor
        static OuiLookup()
        {
            OuiList = new List<OuiInfo>();

            var document = new XmlDocument();
            document.Load(OuiFilePath);

            // ReSharper disable once PossibleNullReferenceException
            foreach (XmlNode node in document.SelectNodes("/OUIs/OUI"))
            {
                if (node != null)
                    OuiList.Add(new OuiInfo(node.SelectSingleNode("MACAddress")?.InnerText, node.SelectSingleNode("Vendor")?.InnerText));
            }

            OuiInfoLookup = (Lookup<string, OuiInfo>)OuiList.ToLookup(x => x.MacAddress);
        }
        #endregion

        #region Methods
        public static Task<List<OuiInfo>> LookupAsync(string macAddress)
        {
            return Task.Run(() => Lookup(macAddress));
        }

        public static List<OuiInfo> Lookup(string macAddress)
        {
            var ouiKey = Regex.Replace(macAddress, "[-|:|.]", "").Substring(0, 6).ToUpper();

            return OuiInfoLookup[ouiKey].ToList();
        }

        public static Task<List<OuiInfo>> LookupByVendorAsync(List<string> vendors)
        {
            return Task.Run(() => LookupByVendor(vendors));
        }

        public static List<OuiInfo> LookupByVendor(List<string> vendors)
        {
            var list = new List<OuiInfo>();

            foreach (var info in OuiList)
            {
                foreach (var vendor in vendors)
                {
                    if (info.Vendor.IndexOf(vendor, StringComparison.OrdinalIgnoreCase) > -1)
                        list.Add(info);
                }
            }

            return list;
        }
        #endregion
    }
}
