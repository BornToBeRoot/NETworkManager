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
    public static class OUILookup
    {
        #region Variables
        private static readonly string OuiFilePath = Path.Combine(ConfigurationManager.Current.ExecutionPath, "Resources", "OUI.xml");

        private static readonly List<OUIInfo> OUIInfoList;
        private static readonly Lookup<string, OUIInfo> OUIInfoLookup;
        #endregion

        #region Constructor
        static OUILookup()
        {
            OUIInfoList = new List<OUIInfo>();

            var document = new XmlDocument();
            document.Load(OuiFilePath);

            // ReSharper disable once PossibleNullReferenceException
            foreach (XmlNode node in document.SelectNodes("/OUIs/OUI"))
            {
                if (node != null)
                    OUIInfoList.Add(new OUIInfo(node.SelectSingleNode("MACAddress")?.InnerText, node.SelectSingleNode("Vendor")?.InnerText));
            }

            OUIInfoLookup = (Lookup<string, OUIInfo>)OUIInfoList.ToLookup(x => x.MACAddress);
        }
        #endregion

        #region Methods
        public static Task<List<OUIInfo>> LookupAsync(string macAddress)
        {
            return Task.Run(() => Lookup(macAddress));
        }

        public static List<OUIInfo> Lookup(string macAddress)
        {
            var ouiKey = Regex.Replace(macAddress, "[-|:|.]", "").Substring(0, 6).ToUpper();

            return OUIInfoLookup[ouiKey].ToList();
        }

        public static Task<List<OUIInfo>> LookupByVendorAsync(List<string> vendors)
        {
            return Task.Run(() => LookupByVendor(vendors));
        }

        public static List<OUIInfo> LookupByVendor(List<string> vendors)
        {
            var list = new List<OUIInfo>();

            foreach (var info in OUIInfoList)
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
