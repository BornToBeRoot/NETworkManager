using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using NETworkManager.Models.Settings;
using System.Text.RegularExpressions;
using System;
using System.Windows;
using System.Xml;

namespace NETworkManager.Models.Network
{
    public static class OUILookup
    {
        #region Variables
        private static string OUIFilePath = Path.Combine(ConfigurationManager.Current.ExecutionPath, "Resources", "OUI.xml");

        private static List<OUIInfo> OUIList;
        private static Lookup<string, OUIInfo> OUIs;
        #endregion

        #region Constructor
        static OUILookup()
        {
            OUIList = new List<OUIInfo>();

            XmlDocument document = new XmlDocument();
            document.Load(OUIFilePath);

            foreach(XmlNode node in document.SelectNodes("/OUIs/OUI"))
            {
                OUIList.Add(new OUIInfo(node.SelectSingleNode("MACAddress").InnerText, node.SelectSingleNode("Vendor").InnerText));
            }

            OUIs = (Lookup<string, OUIInfo>)OUIList.ToLookup(x => x.MACAddress);
        }
        #endregion

        #region Methods
        public static Task<List<OUIInfo>> LookupAsync(string macAddress)
        {
            return Task.Run(() => Lookup(macAddress));
        }

        public static List<OUIInfo> Lookup(string macAddress)
        {
            List<OUIInfo> list = new List<OUIInfo>();

            string ouiKey = Regex.Replace(macAddress, "-|:", "").Substring(0, 6).ToUpper();

            foreach (OUIInfo info in OUIs[ouiKey])
            {
                list.Add(info);
            }

            return list;
        }

        public static Task<List<OUIInfo>> LookupByVendorAsync(List<string> vendors)
        {
            return Task.Run(() => LookupByVendor(vendors));
        }

        public static List<OUIInfo> LookupByVendor(List<string> vendors)
        {
            List<OUIInfo> list = new List<OUIInfo>();

            foreach (OUIInfo info in OUIList)
            {
                foreach (string vendor in vendors)
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
