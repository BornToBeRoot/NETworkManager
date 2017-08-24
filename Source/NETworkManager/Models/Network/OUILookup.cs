using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using NETworkManager.Models.Settings;
using System.Text.RegularExpressions;
using System;
using System.Windows;

namespace NETworkManager.Models.Network
{
    public static class OUILookup
    {
        #region Variables
        private static string OUIFilePath = Path.Combine(ConfigurationManager.Current.ExecutionPath, "Resources", "oui.txt");

        private static List<OUIInfo> OUIList;
        private static Lookup<string, OUIInfo> OUIs;
        #endregion

        #region Constructor
        static OUILookup()
        {
            OUIList = new List<OUIInfo>();

            // Load list from resource folder (.txt-file)
            foreach (string line in File.ReadAllLines(OUIFilePath))
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                string[] ouiData = line.Split('|');

                OUIList.Add(new OUIInfo(ouiData[0], ouiData[1]));
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
                    if (info.Vendor.IndexOf(vendor, StringComparison.OrdinalIgnoreCase) >= 0)
                        list.Add(info);
                }
            }

            return list;
        }
        #endregion
    }
}
