using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using NETworkManager.Models.Settings;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public static class OUILookup
    {
        #region Variables
        private static string OUIPath = Path.Combine(ConfigurationManager.Current.StartupPath, "Resources", "oui.txt");

        public static Dictionary<string, string> OUIDict = new Dictionary<string, string>();

        #endregion

        #region Methods
        static OUILookup()
        {
            // Load list from resource folder (.txt-file)
            foreach (string line in File.ReadAllLines(OUIPath))
            {                
                string[] ouiData = line.Split('|');

                if (!string.IsNullOrEmpty(ouiData[0]) && !OUIDict.Keys.Contains(ouiData[0]))
                    OUIDict.Add(ouiData[0], ouiData[1]);
            }
        }

        public static Task<OUIInfo> LookupAsync(string macAddressOrFirst3Bytes)
        {
            return Task.Run(() => Lookup(macAddressOrFirst3Bytes));
        }

        public static OUIInfo Lookup(string macAddressOrFirst3Bytes)
        {
            string ouiKey = Regex.Replace(macAddressOrFirst3Bytes, "-|:", "").Substring(0,6);

            string vendor = string.Empty;
            OUIDict.TryGetValue(ouiKey, out vendor);

            return new OUIInfo(ouiKey, vendor);
        }

        public static Task<OUIInfo> LookupAsync(PhysicalAddress macAddress)
        {
            return Task.Run(() => Lookup(macAddress));
        }

        public static OUIInfo Lookup(PhysicalAddress macAddress)
        {
            return Lookup(macAddress.ToString());
        }
        #endregion
    }
}
